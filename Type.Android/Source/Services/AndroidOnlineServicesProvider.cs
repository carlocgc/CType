using System;
using Type.Interfaces.Service;

namespace Type.Android.Source.Services
{
    /// <summary>
    /// Android has no SDK lifecycle to own here.
    /// </summary>
    /// <remarks>
    /// Google Play's services reach the game through the engine's <c>CompetitiveManager</c>,
    /// which is loaded from <c>Game.LoadContent</c> under <c>__ANDROID__</c> and manages itself.
    /// This exists so the shared service has a provider on both platforms rather than a
    /// preprocessor hole, and reports unavailable so nothing routes through it by mistake.
    /// </remarks>
    public sealed class AndroidOnlineServicesProvider : IOnlineServicesProvider
    {
        /// <inheritdoc />
        public Boolean Available => false;

        /// <inheritdoc />
        public void Initialise()
        {
        }

        /// <inheritdoc />
        public void Update()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
