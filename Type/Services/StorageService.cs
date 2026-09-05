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
    /// Service that keeps the player's settings and progress between sessions
    /// </summary>
    /// <remarks>
    /// The engine's <c>DataLoader</c> is deliberately not used for the game's own values. It
    /// opens its store with <c>GetUserStoreForAssembly</c>, which for an assembly with no strong
    /// name resolves through the path of the running executable, so moving or reinstalling the
    /// game silently produced an empty store and lost everything the player had. See ROADMAP
    /// item S11. Achievement and leaderboard values written by the engine still live there, and
    /// are inert on desktop because <c>CompetitiveManager</c> only loads under Android.
    /// </remarks>
    public sealed class StorageService
    {
        /// <summary> The instance of the StorageService </summary>
        private static StorageService _Instance;

        /// <summary> The instance of the StorageService </summary>
        public static StorageService Instance => _Instance ?? (_Instance = new StorageService());

        /// <summary> Platform specific storage provider </summary>
        private readonly IStorageProvider _StorageProvider;

        private StorageService()
        {
#if __ANDROID__
            _StorageProvider = new AndroidStorageProvider();
#elif __DESKTOP__
            _StorageProvider = new DesktopStorageProvider();
#endif
        }

        /// <summary>
        /// Reads the stored values into memory. Call once during content loading, before
        /// <see cref="Type.Data.Settings"/> or <see cref="Type.Data.Progress"/> read one.
        /// </summary>
        public void Load()
        {
            _StorageProvider.Load();
        }

        /// <summary>
        /// Reads one stored value
        /// </summary>
        /// <param name="key"> The key to read </param>
        /// <returns> The stored value, or null when nothing has been saved under that key </returns>
        public Object GetValue(String key)
        {
            return _StorageProvider.GetValue(key);
        }

        /// <summary>
        /// Writes one value and saves the store
        /// </summary>
        /// <param name="key"> The key to write </param>
        /// <param name="value"> The value to store </param>
        public void SetValue(String key, Object value)
        {
            _StorageProvider.SetValue(key, value);
        }
    }
}
