using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Buttons;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Control;

namespace Type.Android.Source.Controllers
{
    public class AndroidInputProvider : IInputProvider, INotifier<IInputListener>, IUpdatable
    {
        private readonly List<IInputListener> _Listeners = new List<IInputListener>();

        private readonly List<IVirtualButton> _Buttons = new List<IVirtualButton>();

        private Vector2 _Velocity;

        private Single _VelocityMagnitude;

        /// <summary> Whether the provider is in pause mode </summary>
        public Boolean Paused { get; set; }

        public AndroidInputProvider()
        {
            UpdateManager.Instance.AddUpdatable(this);
        }

        #region Implementation of IUpdatable

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            if (VirtualAnalogStick?.Y > 0 || VirtualAnalogStick?.Y < 0 || VirtualAnalogStick?.X > 0 || VirtualAnalogStick?.X < 0)
            {
                _Velocity = new Vector2(VirtualAnalogStick.X, VirtualAnalogStick.Y);
                _VelocityMagnitude = VirtualAnalogStick.Magnitude;
            }
            else
            {
                _Velocity = Vector2.Zero;
            }

            foreach (IInputListener listener in _Listeners)
            {
                listener.UpdateDirectionData(_Velocity, _VelocityMagnitude);

                foreach (IVirtualButton button in _Buttons)
                {
                    listener.UpdateInputData(new ButtonEventData(button.ID, button.State));
                }
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

        #region Implementation of INotifier<in IInputListener>

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
            if (!_Buttons.Contains(button)) _Buttons.Add(button);
        }

        /// <summary>
        /// Deregisters a <see cref="IVirtualButton"/> from the Input provider
        /// </summary>
        /// <param name="button"></param>
        public void DeregisterButton(IVirtualButton button)
        {
            if (_Buttons.Contains(button)) _Buttons.Remove(button);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _Listeners.Clear();
            _Buttons.Clear();
            UpdateManager.Instance.RemoveUpdatable(this);
        }

        #endregion
    }
}