using System;
using Type.Data;
using Type.Interfaces.Service;

namespace Type.Android.Source.Services
{
    /// <summary>
    /// Display provider for Android, where the game always fills the screen
    /// </summary>
    public sealed class AndroidDisplayProvider : IDisplayProvider
    {
        /// <inheritdoc />
        /// <remarks> The activity is always fullscreen, so there is nothing for the player to choose. </remarks>
        public Boolean CanChangeMode => false;

        /// <inheritdoc />
        public void SetMode(DisplayMode mode)
        {
        }
    }
}
