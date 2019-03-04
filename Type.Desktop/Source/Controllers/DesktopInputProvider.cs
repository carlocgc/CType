using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Buttons;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Control;
using Type.Services;

namespace Type.Desktop.Source.Controllers
{
    /// <summary>
    /// Input provider for the <see cref="InputService"/> when in Desktop configuration
    /// </summary>
    public class DesktopInputProvider : IInputProvider, INotifier<IInputListener>, IUpdatable
    {
        /// <summary> List of all the <see cref="IInputListener"/>'s listening to the <see cref="InputService"/></summary>
        private readonly List<IInputListener> _Listeners = new List<IInputListener>();
        /// <summary> Direction the analog stick is pushed </summary>
        private Vector2 _Velocity;
        /// <summary> Distance the analog stick is pushed </summary>
        private Single _VelocityMagnitude;
        /// <summary> Deadzone for positive axis on the analog stick </summary>
        private Single _PositiveDeadZone = 0.2f;
        /// <summary> Deadzone for negative axis on the analog stick </summary>
        private Single _NegativeDeadZone = -0.2f;
        /// <summary> Whether the nuke button was pressed last update </summary>
        private Boolean _NukePressed;
        /// <summary> Whether start was pressed last update </summary>
        private Boolean _StartPressed;
        /// <summary> Call back to end controller vibration </summary>
        private TimedCallback _VibrationCallback;

        private Boolean _YPressed;
        private Boolean _BackPressed;

        /// <summary> Whether the provider is in pause mode </summary>
        public Boolean Paused { get; set; }

        public DesktopInputProvider()
        {
            UpdateManager.Instance.AddUpdatable(this);
        }

        #region Implementation of IUpdatable

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            // ---------------- START BUTTON

            if (GamePad.GetState(0).Buttons.Start == ButtonState.Pressed)
            {
                ButtonData.State state = _StartPressed ? ButtonData.State.HELD : ButtonData.State.PRESSED;

                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.START, state));
                }
                _StartPressed = true;
            }
            else if (GamePad.GetState(0).Buttons.Start == ButtonState.Released)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.START, ButtonData.State.RELEASED));
                }
                _StartPressed = false;
            }

            // ---------------- Dont update anything else if game is paused

            if (Paused) return;

            // ---------------- ANALOG STICK

            if (GamePad.GetState(0).ThumbSticks.Left.Y > _PositiveDeadZone ||
                GamePad.GetState(0).ThumbSticks.Left.Y < _NegativeDeadZone ||
                GamePad.GetState(0).ThumbSticks.Left.X > _PositiveDeadZone ||
                GamePad.GetState(0).ThumbSticks.Left.X < _NegativeDeadZone)
            {
                _Velocity = GamePad.GetState(0).ThumbSticks.Left;
                _VelocityMagnitude = GamePad.GetState(0).ThumbSticks.Left.Length;
            }
            else
            {
                _Velocity = Vector2.Zero;
            }

            // ---------------- A BUTTON

            if (GamePad.GetState(0).Buttons.A == ButtonState.Pressed)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.FIRE, ButtonData.State.PRESSED));
                }
            }
            else if (GamePad.GetState(0).Buttons.A == ButtonState.Released)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.FIRE, ButtonData.State.RELEASED));
                }
            }

            // ---------------- B BUTTON

            if (GamePad.GetState(0).Buttons.B == ButtonState.Pressed)
            {
                ButtonData.State state = _NukePressed ? ButtonData.State.HELD : ButtonData.State.PRESSED;

                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.NUKE, state));
                }
                _NukePressed = true;
            }
            else if (GamePad.GetState(0).Buttons.B == ButtonState.Released)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.NUKE, ButtonData.State.RELEASED));
                }
                _NukePressed = false;
            }

            // ---------------- Y BUTTON

            if (GamePad.GetState(0).Buttons.Y == ButtonState.Pressed)
            {
                ButtonData.State state = _YPressed ? ButtonData.State.HELD : ButtonData.State.PRESSED;

                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.GAMMA_SELECT, state));
                }
                _YPressed = true;
            }
            else if (GamePad.GetState(0).Buttons.Y == ButtonState.Released)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.GAMMA_SELECT, ButtonData.State.RELEASED));
                }
                _YPressed = false;
            }

            // ---------------- BACK BUTTON

            if (GamePad.GetState(0).Buttons.Back == ButtonState.Pressed)
            {
                ButtonData.State state = _BackPressed ? ButtonData.State.HELD : ButtonData.State.PRESSED;

                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.BACK, state));
                }
                _BackPressed = true;
            }
            else if (GamePad.GetState(0).Buttons.X == ButtonState.Released)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.UpdateInputData(new ButtonEventData(ButtonData.Type.BACK, ButtonData.State.RELEASED));
                }
                _BackPressed = false;
            }

            // ---------------- SEND ANALOG DATA TO LISTENERS

            foreach (IInputListener listener in _Listeners)
            {
                listener.UpdateDirectionData(_Velocity, _VelocityMagnitude);
            }
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

        /// <summary> Virtual analog stick </summary>
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
            Boolean result = GamePad.SetVibration(index, left, right);
            if (result) return;
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
        /// Registers a <see cref="IVirtualButton"/> with the Input provider
        /// </summary>
        /// <param name="button"></param>
        public void RegisterButton(IVirtualButton button)
        {
        }

        /// <summary>
        /// Deregisters a <see cref="IVirtualButton"/> from the Input provider
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
            _VibrationCallback.CancelAndComplete();
            _VibrationCallback.Dispose();
        }

        #endregion
    }
}
