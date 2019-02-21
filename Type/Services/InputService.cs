using Type.Buttons;
using Type.Interfaces.Control;
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
    }
}
