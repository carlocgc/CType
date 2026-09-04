using System;
using Type.Buttons;
using Type.Interfaces.Control;
using Type.Input;
#if __ANDROID__
using Type.Android.Source.Controllers;
#elif __DESKTOP__
using Type.Desktop.Source.Controllers;
#endif

namespace Type.Services
{
    /// <summary>
    /// Service that provides data to listeners about input devices that have been registered
    /// </summary>
    public sealed class InputService
    {
        /// <summary> The instance of the Input manager </summary>
        private static InputService _Instance;
        /// <summary> The instance of the Input manager </summary>
        public static InputService Instance => _Instance ?? (_Instance = new InputService());

        /// <summary> Virtual analog stick </summary>
        public VirtualAnalogStick VirtualAnalogStick
        {
            get => _InputProvider.VirtualAnalogStick;
            set => _InputProvider.VirtualAnalogStick = value;
        }

        /// <summary>
        /// The active action to input mapping, used to label on screen prompts
        /// </summary>
        public InputBindings Bindings => _InputProvider.Bindings;

        /// <summary>
        /// Whether a gamepad is currently driving input, so the interface can show the right
        /// prompts
        /// </summary>
        public Boolean GamepadActive => _InputProvider.GamepadActive;

        /// <summary>
        /// Invoked when the input device driving the game becomes unavailable, for example a
        /// gamepad being unplugged mid game. Platforms with no detachable input device never
        /// invoke it.
        /// </summary>
        public Action OnInputDeviceLost
        {
            get => _InputProvider.OnInputDeviceLost;
            set => _InputProvider.OnInputDeviceLost = value;
        }

        /// <summary> Platform specific input provider </summary>
        private readonly IInputProvider _InputProvider;

        private InputService()
        {
#if __ANDROID__
            _InputProvider = new AndroidInputProvider();
#elif __DESKTOP__
            _InputProvider = new DesktopInputProvider();
#endif
        }

        /// <summary>
        /// Whether the input system is paused
        /// </summary>
        /// <param name="paused"></param>
        public void SetPaused(Boolean paused)
        {
            _InputProvider.Paused = paused;
        }

        /// <summary>
        /// Vibrates a controller
        /// </summary>
        /// <param name="index"> Index of the controller to vibrate </param>
        /// <param name="strong"> Whether to use strong vibration </param>
        /// <param name="duration"> How long the vbration should last </param>
        public void Vibrate(Int32 index, Boolean strong, TimeSpan duration)
        {
            _InputProvider.Vibrate(index, strong, duration);
        }

        /// <summary>
        /// Add a listener
        /// </summary>
        public void RegisterListener(IInputListener listener)
        {
            _InputProvider.RegisterListener(listener);
        }

        /// <summary>
        /// Remove a listener
        /// </summary>
        public void DeregisterListener(IInputListener listener)
        {
            _InputProvider.DeregisterListener(listener);
        }

        /// <summary>
        /// Add a <see cref="IVirtualButton"/> to the <see cref="InputService"/>
        /// </summary>
        public void RegisterButton(IVirtualButton button)
        {
            _InputProvider.RegisterButton(button);
        }

        /// <summary>
        /// Remove a <see cref="IVirtualButton"/> from the <see cref="InputService"/>
        /// </summary>
        public void DeregisterButton(IVirtualButton button)
        {
            _InputProvider.DeregisterButton(button);
        }
    }
}
