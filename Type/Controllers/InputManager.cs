using Type.Buttons;
using Type.Interfaces.Control;
#if __ANDROID__
using Type.Android.Source.Controllers;
#elif __DESKTOP__
using Type.Desktop.Source.Controllers;
#endif

namespace Type.Controllers
{
    public class InputManager
    {
        /// <summary> The instance of the Input manager </summary>
        private static InputManager _Instance;
        /// <summary> The instance of the Input manager </summary>
        public static InputManager Instance => _Instance ?? (_Instance = new InputManager());

        /// <summary> Virtual analog stick </summary>
        public VirtualAnalogStick VirtualAnalogStick
        {
            get => _InputProvider.VirtualAnalogStick;
            set => _InputProvider.VirtualAnalogStick = value;
        }

        /// <summary> Virtual nuke button </summary>
        public NukeButton NukeButton
        {
            get => _InputProvider.NukeButton;
            set => _InputProvider.NukeButton = value;
        }

        /// <summary> Virtual firebutton </summary>
        public FireButton FireButton
        {
            get => _InputProvider.FireButton;
            set => _InputProvider.FireButton = value;
        }

        /// <summary> Virtual pausebutton </summary>
        public PauseButton PauseButton
        {
            get => _InputProvider.PauseButton;
            set => _InputProvider.PauseButton = value;
        }

        /// <summary> Virtual resumebutton</summary>
        public ResumeButton ResumeButton
        {
            get => _InputProvider.ResumeButton;
            set => _InputProvider.ResumeButton = value;
        }

        /// <summary> Platform specific input provider </summary>
        private readonly IInputProvider _InputProvider;

        public InputManager()
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
