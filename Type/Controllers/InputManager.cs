using System;
using System.Collections.Generic;
using System.Text;
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
