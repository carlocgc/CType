using System;
using Type.Data;

namespace Type.Interfaces.Service
{
    /// <summary>
    /// Interface for a platform specific window provider, so shared code can offer a display
    /// setting without knowing what a window is on this platform.
    /// </summary>
    public interface IDisplayProvider
    {
        /// <summary> Whether the platform lets the player choose how the game fills the display </summary>
        Boolean CanChangeMode { get; }

        /// <summary>
        /// Applies a display mode. Platforms that cannot change mode ignore this.
        /// </summary>
        /// <param name="mode"> The mode to apply </param>
        void SetMode(DisplayMode mode);
    }
}
