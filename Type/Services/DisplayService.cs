using System;
using Type.Data;
using Type.Interfaces.Service;
#if __ANDROID__
using Type.Android.Source.Services;
#elif __DESKTOP__
using Type.Desktop.Source.Services;
#endif

namespace Type.Services
{
    /// <summary>
    /// Service that applies the player's choice of how the game fills the display
    /// </summary>
    public sealed class DisplayService
    {
        /// <summary> The instance of the DisplayService </summary>
        private static DisplayService _Instance;

        /// <summary> The instance of the DisplayService </summary>
        public static DisplayService Instance => _Instance ?? (_Instance = new DisplayService());

        /// <summary> Platform specific display provider </summary>
        private readonly IDisplayProvider _DisplayProvider;

        /// <summary> Whether the platform lets the player choose a display mode </summary>
        public Boolean CanChangeMode => _DisplayProvider.CanChangeMode;

        private DisplayService()
        {
#if __ANDROID__
            _DisplayProvider = new AndroidDisplayProvider();
#elif __DESKTOP__
            _DisplayProvider = new DesktopDisplayProvider();
#endif
        }

        /// <summary>
        /// Applies a display mode
        /// </summary>
        /// <param name="mode"> The mode to apply </param>
        public void SetMode(DisplayMode mode)
        {
            _DisplayProvider.SetMode(mode);
        }
    }
}
