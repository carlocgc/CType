using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using Type.Base;
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
        private readonly Dictionary<ButtonData.Type, List<Key>> _ResolvedKeys;

        /// <summary> Index of the gamepad currently driving input, negative when none is connected </summary>
        private Int32 _ActivePad = -1;
        /// <summary> Call back to end controller vibration </summary>
        private TimedCallback _VibrationCallback;
        /// <summary> Whether the most recent real input came from a gamepad rather than the keyboard </summary>
        private Boolean _LastInputWasGamepad;

        /// <summary> Whether the provider is in pause mode </summary>
        public Boolean Paused { get; set; }

        /// <inheritdoc />
        public InputBindings Bindings => _Bindings;

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
            _ResolvedKeys = ResolveKeys(_Bindings);

            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Resolves the platform independent key names in the bindings to OpenTK keys once, so
        /// that polling does not parse strings every update. Unrecognised names are skipped.
        /// </summary>
        private static Dictionary<ButtonData.Type, List<Key>> ResolveKeys(InputBindings bindings)
        {
            Dictionary<ButtonData.Type, List<Key>> resolved = new Dictionary<ButtonData.Type, List<Key>>();

            foreach (ActionBinding binding in bindings.All)
            {
                List<Key> keys = new List<Key>();

                foreach (String name in binding.Keys)
                {
                    if (Enum.TryParse(name, true, out Key key)) keys.Add(key);
                }

                resolved[binding.Action] = keys;
            }

            return resolved;
        }

        #region Implementation of IUpdatable

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            KeyboardState keyboard = Keyboard.GetState();
            GamePadState pad = GetActivePad();

            TrackActiveDevice(keyboard, pad);

            foreach (ActionBinding binding in _Bindings.All)
            {
                // While paused only the ship stops listening. The menu the pause put on screen
                // still needs to be navigable, and an action held at the moment of pausing is
                // reported as released so it does not resume firing when play does.
                Boolean suppressed = Paused && IsGameplayAction(binding.Action);
                Boolean isDown = !suppressed && IsActionDown(binding.Action, keyboard, pad);

                DispatchAction(binding.Action, isDown);
            }

            DispatchDirection(Paused ? default(GamePadState) : pad, keyboard);
        }

        /// <summary>
        /// Returns the state of the gamepad driving input, selecting a newly connected pad and
        /// reporting a disconnection of the one in use
        /// </summary>
        private GamePadState GetActivePad()
        {
            if (_ActivePad >= 0)
            {
                GamePadState current = GamePad.GetState(_ActivePad);
                if (current.IsConnected) return current;

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

        /// <summary>
        /// Vibrates a controller
        /// </summary>
        /// <param name="index"> Index of the controller to vibrate </param>
        /// <param name="strong"> Whether to use strong vibration </param>
        /// <param name="duration"> How long the vbration should last </param>
        public void Vibrate(Int32 index, Boolean strong, TimeSpan duration)
        {
            Single left = strong ? 1f : 0.5f;
            Single right = strong ? 1f : 0.2f;

            // Previously this returned early when SetVibration succeeded, so the callback that
            // stops the motors was only ever scheduled when starting them had failed.
            if (!GamePad.SetVibration(index, left, right)) return;

            _VibrationCallback?.CancelAndComplete();
            _VibrationCallback = new TimedCallback(duration, () => GamePad.SetVibration(index, 0, 0));
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
            _VibrationCallback?.CancelAndComplete();
            _VibrationCallback?.Dispose();
            _Listeners.Clear();
        }

        #endregion
    }
}
