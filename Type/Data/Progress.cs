using System;
using Type.Services;

namespace Type.Data
{
    /// <summary>
    /// Player progression that survives between sessions, held by <see cref="StorageService"/>
    /// alongside the high score and the settings.
    /// </summary>
    /// <remarks>
    /// Kept apart from <see cref="GameStats"/>, which only lasts as long as a run, and from
    /// <see cref="Settings"/>, which holds what the player chose rather than what they earned.
    /// </remarks>
    public static class Progress
    {
        /// <summary> Store key for whether the campaign has ever been finished </summary>
        private const String GameCompletedKey = "GAME_COMPLETED";

        /// <summary> Whether the player has finished the campaign at least once </summary>
        public static Boolean GameCompleted { get; private set; }

        /// <summary>
        /// Reads the saved progression. Call once during content loading, before any state that
        /// reacts to it can be entered.
        /// </summary>
        public static void Load()
        {
            GameCompleted = ReadFlag(GameCompletedKey);
        }

        /// <summary>
        /// Records that the campaign has been finished and saves it
        /// </summary>
        public static void SetGameCompleted()
        {
            if (GameCompleted) return;

            GameCompleted = true;
            StorageService.Instance.SetValue(GameCompletedKey, true);
        }

        /// <summary>
        /// Reads a stored flag, treating anything missing or unreadable as not yet earned
        /// </summary>
        /// <param name="key"> The store key to read </param>
        private static Boolean ReadFlag(String key)
        {
            Object stored = StorageService.Instance.GetValue(key);
            if (stored == null) return false;

            try
            {
                return Convert.ToBoolean(stored);
            }
            catch (Exception)
            {
                // A corrupt or unexpected value must not stop the game starting.
                return false;
            }
        }
    }
}
