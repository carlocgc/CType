using AmosShared.Base;
using AmosShared.Interfaces;
using System;
using Type.Interfaces.Service;
#if __ANDROID__
using Type.Android.Source.Services;
#elif __DESKTOP__
using Type.Desktop.Source.Services;
#endif

namespace Type.Services
{
    /// <summary>
    /// Service that owns the platform's online SDK: starting it, ticking it, and shutting it
    /// down. On desktop that is Steam.
    /// </summary>
    /// <remarks>
    /// One place for the lifecycle, because everything else that talks to the platform depends
    /// on it having been started and being ticked — achievements now, rich presence and
    /// matchmaking later. See ROADMAP S6 and Phase 8.
    /// </remarks>
    public sealed class OnlineService : IUpdatable
    {
        /// <summary> The instance of the OnlineService </summary>
        private static OnlineService _Instance;

        /// <summary> The instance of the OnlineService </summary>
        public static OnlineService Instance => _Instance ?? (_Instance = new OnlineService());

        /// <summary> Platform specific online services provider </summary>
        private readonly IOnlineServicesProvider _Provider;

        /// <summary> Whether the platform's SDK started and can be used </summary>
        public Boolean Available => _Provider.Available;

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        private OnlineService()
        {
#if __ANDROID__
            _Provider = new AndroidOnlineServicesProvider();
#elif __DESKTOP__
            _Provider = new SteamOnlineServicesProvider();
#endif
        }

        /// <summary>
        /// Starts the platform's SDK. Call once during content loading.
        /// </summary>
        /// <remarks>
        /// Registering for updates only once the SDK is up avoids ticking a provider that has
        /// nothing to tick, which is the normal case when the client is not running.
        /// </remarks>
        public void Initialise()
        {
            _Provider.Initialise();

            if (_Provider.Available) UpdateManager.Instance.AddUpdatable(this);
        }

        #region Implementation of IUpdatable

        /// <inheritdoc />
        /// <remarks>
        /// The SDK delivers its callbacks on this tick rather than on a thread of its own, so
        /// missing it means the platform goes quiet rather than merely slow.
        /// </remarks>
        public void Update(TimeSpan timeTilUpdate)
        {
            _Provider.Update();
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Shuts the platform's SDK down
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            UpdateManager.Instance.RemoveUpdatable(this);
            _Provider.Dispose();
            _Instance = null;
        }
    }
}
