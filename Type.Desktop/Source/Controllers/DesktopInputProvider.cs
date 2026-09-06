using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using Type.Buttons;
using Type.Data;
using Type.Input;
using Type.Interfaces;
using Type.Interfaces.Control;
using Type.Services;

namespace Type.Desktop.Source.Controllers
{
    /// <summary>
    /// Input provider for the <see cref="InputService"/> when in Desktop configuration.
    /// Polls the keyboard and every connected gamepad, resolves them through
    /// <see cref="InputBindings"/>, and reports edge triggered action changes to listeners.
    /// </summary>
    public class DesktopInputProvider : IInputProvider, INotifier<IInputListener>, IUpdatable
    {
        /// <summary> Number of gamepad slots the platform exposes </summary>
        private const Int32 PadSlots = 4;

        /// <summary>
        /// How far a trigger must be pulled to count as pressed.
        /// </summary>
        /// <remarks>
        /// Deliberately well clear of centre. An idle or phantom device on this platform reports
        /// both triggers at 0.502, which a 0.5 threshold read as permanently held: fire and nuke
        /// were stuck on before the player touched anything.
        /// </remarks>
        private const Single TriggerThreshold = 0.65f;

        /// <summary> List of all the <see cref="IInputListener"/>'s listening to the <see cref="InputService"/></summary>
        private readonly List<IInputListener> _Listeners = new List<IInputListener>();
        /// <summary> Action to input mapping </summary>
        private readonly InputBindings _Bindings;
        /// <summary> Turns per update readings into edge triggered state changes </summary>
        private readonly ActionStateTracker _Tracker;
        /// <summary> Applies the radial deadzone and response curve to stick readings </summary>
        private readonly AnalogProcessor _Analog;
        /// <summary> Keyboard keys bound to each action, resolved once from the binding names </summary>
        private readonly Dictionary<ButtonData.Type, List<Key>> _ResolvedKeys = new Dictionary<ButtonData.Type, List<Key>>();
        /// <summary> Every key a capture will consider, built once from the platform's key enum </summary>
        private readonly Key[] _CapturableKeys = BuildCapturableKeys();
        /// <summary> Every gamepad button a capture will consider </summary>
        private readonly GamepadButton[] _CapturablePadButtons = BuildCapturablePadButtons();
        /// <summary> Drives the rumble motors, which OpenTK cannot </summary>
        private readonly XInputRumble _Rumble = new XInputRumble();

        /// <summary> Index of the gamepad currently driving input, negative when none is connected </summary>
        private Int32 _ActivePad = -1;
        /// <summary> How hard the motors are being driven, zero when they are off </summary>
        private Single _VibrationStrength;
        /// <summary> Wall clock time the current rumble should stop at </summary>
        private DateTime _VibrationUntil;
        /// <summary> Whether the most recent real input came from a gamepad rather than the keyboard </summary>
        private Boolean _LastInputWasGamepad;
        /// <summary>
        /// The bindings to dispatch each update, rebuilt whenever the mapping changes
        /// </summary>
        /// <remarks>
        /// A snapshot rather than the live collection. A listener can change the mapping from
        /// inside a dispatch — RESET DEFAULTS on the controls screen does exactly that — and
        /// iterating the collection it is changing threw. <see cref="InputBindings.CopyFrom"/>
        /// no longer restructures for that reason, but dispatching from an array means no
        /// future change to the mapping can invalidate this loop either.
        /// </remarks>
        private ActionBinding[] _DispatchOrder = new ActionBinding[0];
        /// <summary> Reports the input a capture in progress collects, null when none is running </summary>
        private Action<InputSource> _OnCaptured;
        /// <summary> Whether a capture has seen everything released and may now take a press </summary>
        private Boolean _CaptureArmed;
        /// <summary> Whether the capture in progress is waiting for a pad button rather than a key </summary>
        private Boolean _CaptureGamepad;
        /// <summary> Whether a capture has decided, and is only waiting for the input to be let go </summary>
        private Boolean _CaptureResolved;
        /// <summary> The input a capture settled on, null when the player backed out </summary>
        private InputSource _Captured;

        /// <summary> Whether the provider is in pause mode </summary>
        public Boolean Paused { get; set; }

        /// <inheritdoc />
        public InputBindings Bindings => _Bindings;

        /// <inheritdoc />
        public Boolean Capturing => _OnCaptured != null;

        /// <inheritdoc />
        /// <remarks>
        /// Reports the device the player last actually used, not merely whether a pad is plugged
        /// in. A machine can report a pad that nobody is holding, and prompting for buttons on a
        /// controller that does not exist is worse than assuming the keyboard.
        /// </remarks>
        public Boolean GamepadActive => _LastInputWasGamepad;

        /// <inheritdoc />
        public Action OnInputDeviceLost { get; set; }

        public DesktopInputProvider()
        {
            _Bindings = InputBindings.CreateDefaults();
            _Tracker = new ActionStateTracker();
            _Analog = new AnalogProcessor();
            ReloadBindings();

            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Resolves the platform independent key names in the bindings to OpenTK keys, so that
        /// polling does not parse strings every update. Unrecognised names are skipped. Called
        /// once at construction and again whenever the player rebinds something.
        /// <para>
        /// **The tracked press states are deliberately not cleared.** They are keyed by action
        /// rather than by input, so a mapping change cannot make them wrong — the next update
        /// reads the new binding and reports a release if nothing is on it any more. Clearing
        /// them made every held input read as freshly pressed, which meant holding confirm on
        /// RESET DEFAULTS re-ran the reset every frame, and with it seven writes to the save
        /// file per frame.
        /// </para>
        /// </remarks>
        public void ReloadBindings()
        {
            _ResolvedKeys.Clear();

            List<ActionBinding> order = new List<ActionBinding>();

            foreach (ActionBinding binding in _Bindings.All)
            {
                List<Key> keys = new List<Key>();

                foreach (String name in binding.Keys)
                {
                    if (Enum.TryParse(name, true, out Key key)) keys.Add(key);
                }

                _ResolvedKeys[binding.Action] = keys;
                order.Add(binding);
            }

            // Assigned rather than filled in place, so an update already iterating the previous
            // array finishes against a set that is whole.
            _DispatchOrder = order.ToArray();
        }

        /// <summary>
        /// Builds the set of keys a capture will consider, once, from the platform's key enum
        /// </summary>
        /// <remarks>
        /// Escape is excluded because it backs out of a capture rather than being bound by it,
        /// and the enum's placeholders name no key at all. Everything else is offered: a player
        /// who wants to fire with a modifier or a numpad key is not talked out of it.
        /// </remarks>
        private static Key[] BuildCapturableKeys()
        {
            List<Key> keys = new List<Key>();

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key == Key.Unknown || key == Key.LastKey || key == Key.Escape) continue;
                if (!keys.Contains(key)) keys.Add(key);
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Builds the set of gamepad buttons a capture will consider, once, so that polling one
        /// does not walk the enum every update
        /// </summary>
        private static GamepadButton[] BuildCapturablePadButtons()
        {
            List<GamepadButton> buttons = new List<GamepadButton>();

            foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
            {
                if (button != GamepadButton.NONE) buttons.Add(button);
            }

            return buttons.ToArray();
        }

        #region Implementation of IUpdatable

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            KeyboardState keyboard = Keyboard.GetState();
            GamePadState pad = GetActivePad();

            TrackActiveDevice(keyboard, pad);
            UpdateVibration();

            if (Capturing)
            {
                PollCapture(keyboard, pad);
                return;
            }

            foreach (ActionBinding binding in _DispatchOrder)
            {
                // While paused only the ship stops listening. The menu the pause put on screen
                // still needs to be navigable, and an action held at the moment of pausing is
                // reported as released so it does not resume firing when play does.
                Boolean suppressed = Paused && IsGameplayAction(binding.Action);
                Boolean isDown = !suppressed && IsActionDown(binding.Action, keyboard, pad);

                DispatchAction(binding.Action, isDown);

                // A listener can open a capture from inside that dispatch — confirming a row on
                // the controls screen does exactly that. The rest of this frame is abandoned so
                // the actions not yet visited are not reported over the releases the capture
                // has just sent out.
                if (Capturing) return;
            }

            DispatchDirection(Paused ? default(GamePadState) : pad, keyboard);
        }

        /// <inheritdoc />
        public void BeginCapture(Boolean gamepad, Action<InputSource> onCaptured)
        {
            if (onCaptured == null) return;

            CancelCapture();
            _CaptureGamepad = gamepad;

            // Listeners are told everything is released before the screen goes quiet, so an
            // action held at the moment the capture opened does not stay held for its duration.
            foreach (ActionBinding binding in _DispatchOrder) DispatchAction(binding.Action, false);

            for (Int32 i = _Listeners.Count - 1; i >= 0; i--) _Listeners[i].UpdateDirectionData(Vector2.Zero, 0);

            _OnCaptured = onCaptured;
            _CaptureArmed = false;
            _CaptureResolved = false;
            _Captured = null;
        }

        /// <inheritdoc />
        public void CancelCapture()
        {
            if (!Capturing) return;

            Action<InputSource> onCaptured = _OnCaptured;
            EndCapture();
            onCaptured(null);
        }

        /// <summary>
        /// Clears the capture state without reporting anything
        /// </summary>
        private void EndCapture()
        {
            _OnCaptured = null;
            _CaptureArmed = false;
            _CaptureResolved = false;
            _Captured = null;
        }

        /// <summary>
        /// Advances a capture in progress by one update
        /// </summary>
        /// <remarks>
        /// Three stages, each waiting on the player rather than on a timer. Nothing is taken
        /// until every input has been released, so the press that opened the capture is not the
        /// one bound. Escape backs out. Once an input has been chosen the capture stays open
        /// until it is let go, which keeps that press out of the game: a key just bound to FIRE
        /// must not also fire the instant the screen closes.
        /// </remarks>
        private void PollCapture(KeyboardState keyboard, GamePadState pad)
        {
            Boolean anythingDown = IsAnythingDown(keyboard, pad);

            if (!_CaptureArmed)
            {
                _CaptureArmed = !anythingDown;
                return;
            }

            if (!_CaptureResolved)
            {
                if (keyboard.IsKeyDown(Key.Escape))
                {
                    _CaptureResolved = true;
                    return;
                }

                _Captured = ReadPressedInput(keyboard, pad);
                _CaptureResolved = _Captured != null;
                return;
            }

            if (anythingDown) return;

            Action<InputSource> onCaptured = _OnCaptured;
            InputSource captured = _Captured;
            EndCapture();
            onCaptured(captured);
        }

        /// <summary>
        /// Whether any input a capture would consider is currently down
        /// </summary>
        private Boolean IsAnythingDown(KeyboardState keyboard, GamePadState pad)
        {
            if (keyboard.IsKeyDown(Key.Escape)) return true;

            foreach (Key key in _CapturableKeys)
            {
                if (keyboard.IsKeyDown(key)) return true;
            }

            return PadHasInput(pad);
        }

        /// <summary>
        /// Returns the first input found pressed, or null if none is
        /// </summary>
        /// <remarks>
        /// Only the device the capture was opened for is read. The screen binds one cell at a
        /// time and a cell holds one device, so pressing the other one leaves the prompt up
        /// rather than binding something the cell cannot hold.
        /// </remarks>
        private InputSource ReadPressedInput(KeyboardState keyboard, GamePadState pad)
        {
            if (_CaptureGamepad)
            {
                if (!pad.IsConnected) return null;

                foreach (GamepadButton button in _CapturablePadButtons)
                {
                    if (IsPadButtonDown(button, pad)) return InputSource.FromPad(button);
                }

                return null;
            }

            foreach (Key key in _CapturableKeys)
            {
                if (keyboard.IsKeyDown(key)) return InputSource.FromKey(key.ToString());
            }

            return null;
        }

        /// <summary>
        /// Returns the state of the gamepad driving input, selecting a newly connected pad and
        /// reporting a disconnection of the one in use
        /// </summary>
        private GamePadState GetActivePad()
        {
            // A slot that reports connected is not necessarily a pad anyone is holding: this
            // machine reports one whose triggers rest at half pull. So a pad that is actually
            // producing input always wins, whichever slot it is in. Picking the lowest connected
            // slot instead meant a real controller could be ignored in favour of a phantom.
            for (Int32 i = 0; i < PadSlots; i++)
            {
                GamePadState state = GamePad.GetState(i);
                if (!PadHasInput(state)) continue;

                _ActivePad = i;
                return state;
            }

            if (_ActivePad >= 0)
            {
                GamePadState current = GamePad.GetState(_ActivePad);
                if (current.IsConnected) return current;

                // A controller that reappears must not come back mid-rumble.
                _VibrationStrength = 0;
                StopVibration();

                _ActivePad = -1;
                _Tracker.Reset();
                OnInputDeviceLost?.Invoke();
            }

            for (Int32 i = 0; i < PadSlots; i++)
            {
                GamePadState state = GamePad.GetState(i);
                if (!state.IsConnected) continue;

                _ActivePad = i;
                return state;
            }

            return default(GamePadState);
        }

        /// <summary>
        /// Whether an action drives the game rather than the interface, and so must stop while
        /// the game is paused
        /// </summary>
        /// <remarks>
        /// Everything else, including the menu actions and the pause button itself, keeps
        /// flowing so that a menu opened by pausing can be used.
        /// </remarks>
        private static Boolean IsGameplayAction(ButtonData.Type action)
        {
            return action == ButtonData.Type.FIRE || action == ButtonData.Type.NUKE;
        }

        /// <summary>
        /// Notes which device the player is actually using, so prompts can name the right one
        /// </summary>
        /// <remarks>
        /// Whichever device produced input most recently wins, and neither being touched changes
        /// nothing. Presence is not enough: a machine can report a pad nobody is holding.
        /// </remarks>
        private void TrackActiveDevice(KeyboardState keyboard, GamePadState pad)
        {
            if (PadHasInput(pad))
            {
                _LastInputWasGamepad = true;
                return;
            }

            if (KeyboardHasInput(keyboard)) _LastInputWasGamepad = false;
        }

        /// <summary>
        /// Whether the pad is reporting anything the game would act on
        /// </summary>
        private Boolean PadHasInput(GamePadState pad)
        {
            if (!pad.IsConnected) return false;

            if (pad.Buttons.IsAnyButtonPressed) return true;
            if (pad.DPad.IsUp || pad.DPad.IsDown || pad.DPad.IsLeft || pad.DPad.IsRight) return true;
            if (pad.Triggers.Left > TriggerThreshold || pad.Triggers.Right > TriggerThreshold) return true;

            return pad.ThumbSticks.Left.Length > _Analog.InnerDeadzone
                || pad.ThumbSticks.Right.Length > _Analog.InnerDeadzone;
        }

        /// <summary>
        /// Whether any key the game binds is down
        /// </summary>
        private Boolean KeyboardHasInput(KeyboardState keyboard)
        {
            foreach (List<Key> keys in _ResolvedKeys.Values)
            {
                foreach (Key key in keys)
                {
                    if (keyboard.IsKeyDown(key)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether any input bound to the action is currently down
        /// </summary>
        private Boolean IsActionDown(ButtonData.Type action, KeyboardState keyboard, GamePadState pad)
        {
            ActionBinding binding = _Bindings[action];
            if (binding == null) return false;

            if (_ResolvedKeys.TryGetValue(action, out List<Key> keys))
            {
                foreach (Key key in keys)
                {
                    if (keyboard.IsKeyDown(key)) return true;
                }
            }

            if (!pad.IsConnected) return false;

            foreach (GamepadButton button in binding.PadButtons)
            {
                if (IsPadButtonDown(button, pad)) return true;
            }

            return false;
        }

        /// <summary>
        /// Maps a platform independent gamepad button onto the OpenTK pad state
        /// </summary>
        private static Boolean IsPadButtonDown(GamepadButton button, GamePadState pad)
        {
            switch (button)
            {
                case GamepadButton.A: return pad.Buttons.A == ButtonState.Pressed;
                case GamepadButton.B: return pad.Buttons.B == ButtonState.Pressed;
                case GamepadButton.X: return pad.Buttons.X == ButtonState.Pressed;
                case GamepadButton.Y: return pad.Buttons.Y == ButtonState.Pressed;
                case GamepadButton.LEFT_SHOULDER: return pad.Buttons.LeftShoulder == ButtonState.Pressed;
                case GamepadButton.RIGHT_SHOULDER: return pad.Buttons.RightShoulder == ButtonState.Pressed;
                case GamepadButton.LEFT_TRIGGER: return pad.Triggers.Left > TriggerThreshold;
                case GamepadButton.RIGHT_TRIGGER: return pad.Triggers.Right > TriggerThreshold;
                case GamepadButton.LEFT_STICK: return pad.Buttons.LeftStick == ButtonState.Pressed;
                case GamepadButton.RIGHT_STICK: return pad.Buttons.RightStick == ButtonState.Pressed;
                case GamepadButton.START: return pad.Buttons.Start == ButtonState.Pressed;
                case GamepadButton.BACK: return pad.Buttons.Back == ButtonState.Pressed;
                case GamepadButton.DPAD_UP: return pad.DPad.IsUp;
                case GamepadButton.DPAD_DOWN: return pad.DPad.IsDown;
                case GamepadButton.DPAD_LEFT: return pad.DPad.IsLeft;
                case GamepadButton.DPAD_RIGHT: return pad.DPad.IsRight;
                default: return false;
            }
        }

        /// <summary>
        /// Reports an action to listeners if its state changed or it is being held
        /// </summary>
        private void DispatchAction(ButtonData.Type action, Boolean isDown)
        {
            if (!_Tracker.TryGetState(action, isDown, out ButtonData.State state)) return;

            for (Int32 i = _Listeners.Count - 1; i >= 0; i--)
            {
                _Listeners[i].UpdateInputData(new ButtonEventData(action, state));
            }
        }

        /// <summary>
        /// Works out the movement direction from the left stick, or from the digital inputs when
        /// the stick is centred, and reports it to listeners
        /// </summary>
        private void DispatchDirection(GamePadState pad, KeyboardState keyboard)
        {
            Vector2 direction;
            Single strength;

            _Analog.Process(pad.IsConnected ? pad.ThumbSticks.Left : Vector2.Zero, out direction, out strength);

            if (strength <= 0)
            {
                // Digital inputs are normalised so that a diagonal is not faster than a cardinal,
                // which it was when the raw key vector was passed straight through.
                Vector2 digital = Vector2.Zero;

                if (IsDigitalDown(ButtonData.Type.MENU_UP, keyboard, pad)) digital += new Vector2(0, 1);
                if (IsDigitalDown(ButtonData.Type.MENU_DOWN, keyboard, pad)) digital += new Vector2(0, -1);
                if (IsDigitalDown(ButtonData.Type.MENU_LEFT, keyboard, pad)) digital += new Vector2(-1, 0);
                if (IsDigitalDown(ButtonData.Type.MENU_RIGHT, keyboard, pad)) digital += new Vector2(1, 0);

                if (digital != Vector2.Zero)
                {
                    digital.Normalize();
                    direction = digital;
                    strength = 1;
                }
            }

            for (Int32 i = _Listeners.Count - 1; i >= 0; i--)
            {
                _Listeners[i].UpdateDirectionData(direction, strength);
            }
        }

        /// <summary>
        /// Whether a directional action is down, ignoring a disconnected pad
        /// </summary>
        private Boolean IsDigitalDown(ButtonData.Type action, KeyboardState keyboard, GamePadState pad)
        {
            return IsActionDown(action, keyboard, pad);
        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        #endregion

        #region Implementation of INotifier<IInputListener>

        /// <summary> Virtual analog stick, unused on desktop </summary>
        public VirtualAnalogStick VirtualAnalogStick { get; set; }

        /// <inheritdoc />
        /// <remarks>
        /// Not gated on a pad being the device currently driving input. A controller plugged in
        /// while somebody plays on the keyboard will rumble on the desk, which is a smaller
        /// problem than rumble silently doing nothing because the gate was not satisfied — and
        /// the player has a slider that turns it off.
        /// </remarks>
        public void Vibrate(Single strength, TimeSpan duration)
        {
            if (strength <= 0 || duration <= TimeSpan.Zero) return;

            DateTime until = DateTime.UtcNow + duration;

            if (strength > _VibrationStrength) _VibrationStrength = strength;
            if (until > _VibrationUntil) _VibrationUntil = until;

            _Rumble.Set(_VibrationStrength);
        }

        /// <summary>
        /// Stops the motors once the current rumble has run its course
        /// </summary>
        /// <remarks>
        /// Timed against the wall clock rather than <c>TimedCallback</c>, which runs on game
        /// time. Pausing sets that clock's multiplier to zero, so a rumble started just before a
        /// pause would have been left running until the game was unpaused — and quitting from
        /// the pause menu would have left the motors on with nothing left to stop them.
        /// </remarks>
        private void UpdateVibration()
        {
            if (_VibrationStrength <= 0) return;
            if (DateTime.UtcNow < _VibrationUntil) return;

            _VibrationStrength = 0;
            StopVibration();
        }

        /// <summary>
        /// Silences the motors
        /// </summary>
        private void StopVibration()
        {
            _Rumble.Set(0);
        }

        /// <summary>
        /// Add a listener
        /// </summary>
        public void RegisterListener(IInputListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener
        /// </summary>
        public void DeregisterListener(IInputListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

        /// <summary>
        /// Registers a <see cref="IVirtualButton"/> with the Input provider. Desktop has no on
        /// screen controls, so this does nothing.
        /// </summary>
        /// <param name="button"></param>
        public void RegisterButton(IVirtualButton button)
        {
        }

        /// <summary>
        /// Deregisters a <see cref="IVirtualButton"/> from the Input provider. Desktop has no on
        /// screen controls, so this does nothing.
        /// </summary>
        /// <param name="button"></param>
        public void DeregisterButton(IVirtualButton button)
        {
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            UpdateManager.Instance.RemoveUpdatable(this);
            EndCapture();

            // Nothing will tick this again, so the motors have to be silenced here or they run
            // until the pad is unplugged.
            _VibrationStrength = 0;
            StopVibration();

            _Listeners.Clear();
        }

        #endregion
    }
}
