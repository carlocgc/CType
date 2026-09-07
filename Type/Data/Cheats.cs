using System;
using Type.Services;

namespace Type.Data
{
    /// <summary>
    /// The testing cheats, and whether this build has any.
    /// </summary>
    /// <remarks>
    /// **These used to be `const` fields edited by hand before a test run**, which is a bad place
    /// for them: the edit has to be made, remembered and undone, it shows up as a diff in an
    /// unrelated change, and the value that ships is whatever was left behind. Nothing catches
    /// that, because a hand edited constant needs no symbol to be defined.
    /// <para>
    /// They are settings now, changed from the options screen and kept in the save like any
    /// other. **Nothing here survives a build without `CTYPE_CHEATS`**: the properties become
    /// literals the compiler folds away, there is no way to set them, and the stored values are
    /// never read. A shipped build cannot be cheated by editing its save.
    /// </para>
    /// </remarks>
    public static class Cheats
    {
#if CTYPE_CHEATS
        /// <summary> Whether this build has cheats at all </summary>
        public const Boolean Available = true;

        /// <summary> Store key for the invincibility cheat </summary>
        private const String InvincibleKey = "CHEAT_INVINCIBLE";
        /// <summary> Store key for the starting level cheat </summary>
        private const String StartLevelKey = "CHEAT_START_LEVEL";
        /// <summary> Store key for the Omega unlock cheat </summary>
        private const String OmegaUnlockedKey = "CHEAT_OMEGA_UNLOCKED";
        /// <summary> Store key for the infinite bombs cheat </summary>
        private const String InfiniteBombsKey = "CHEAT_INFINITE_BOMBS";

        /// <summary> Whether the player takes damage </summary>
        public static Boolean Invincible { get; private set; }

        /// <summary> The level a new run begins on </summary>
        public static Int32 StartLevel { get; private set; } = 1;

        /// <summary>
        /// Whether the Omega ship's unlock follows the campaign, or is forced either way
        /// </summary>
        /// <remarks>
        /// Deliberately separate from <see cref="Progress.GameCompleted"/> rather than setting
        /// it, and three states rather than a switch. See <see cref="CheatOverride"/>.
        /// </remarks>
        public static CheatOverride OmegaUnlock { get; private set; }

        /// <summary> Whether firing a bomb uses one up </summary>
        public static Boolean InfiniteBombs { get; private set; }

        /// <summary>
        /// Reads the saved cheats. Call once during content loading, alongside
        /// <see cref="Settings.Load"/>.
        /// </summary>
        public static void Load()
        {
            Invincible = ReadFlag(InvincibleKey);
            StartLevel = ReadStartLevel();
            OmegaUnlock = ReadOverride(OmegaUnlockedKey);
            InfiniteBombs = ReadFlag(InfiniteBombsKey);
        }

        /// <summary>
        /// Turns invincibility on or off and saves it
        /// </summary>
        /// <param name="invincible"> Whether the player takes damage </param>
        public static void SetInvincible(Boolean invincible)
        {
            Invincible = invincible;
            StorageService.Instance.SetValue(InvincibleKey, Invincible ? 1 : 0);
        }

        /// <summary>
        /// Sets the level a new run begins on and saves it
        /// </summary>
        /// <param name="level"> The level, clamped to the ones that exist </param>
        public static void SetStartLevel(Int32 level)
        {
            StartLevel = Clamp(level);
            StorageService.Instance.SetValue(StartLevelKey, StartLevel);
        }

        /// <summary>
        /// Sets how the Omega unlock is decided and saves it
        /// </summary>
        /// <param name="state"> Whether to follow the campaign or force a state </param>
        public static void SetOmegaUnlock(CheatOverride state)
        {
            OmegaUnlock = state;
            StorageService.Instance.SetValue(OmegaUnlockedKey, OmegaUnlock.ToString());
        }

        /// <summary>
        /// Turns infinite bombs on or off and saves it
        /// </summary>
        /// <param name="infinite"> Whether firing a bomb uses one up </param>
        public static void SetInfiniteBombs(Boolean infinite)
        {
            InfiniteBombs = infinite;
            StorageService.Instance.SetValue(InfiniteBombsKey, InfiniteBombs ? 1 : 0);
        }

        /// <summary>
        /// Reads a stored flag, treating anything missing or unparseable as off
        /// </summary>
        private static Boolean ReadFlag(String key)
        {
            Object stored = StorageService.Instance.GetValue(key);
            if (stored == null) return false;

            try
            {
                return Convert.ToInt32(stored) != 0;
            }
            catch (Exception)
            {
                // A corrupt value must not stop the game starting, and must not turn a cheat on.
                return false;
            }
        }

        /// <summary>
        /// Reads a stored override, falling back to leaving the game's own answer alone
        /// </summary>
        /// <remarks>
        /// Stored by name rather than ordinal, the same as the display mode, so reordering
        /// <see cref="CheatOverride"/> cannot silently change what a saved cheat means.
        /// </remarks>
        private static CheatOverride ReadOverride(String key)
        {
            Object stored = StorageService.Instance.GetValue(key);
            if (stored == null) return CheatOverride.DEFAULT;

            return Enum.TryParse(stored.ToString(), true, out CheatOverride state)
                ? state
                : CheatOverride.DEFAULT;
        }

        /// <summary>
        /// Reads the stored starting level, falling back to the first one
        /// </summary>
        private static Int32 ReadStartLevel()
        {
            Object stored = StorageService.Instance.GetValue(StartLevelKey);
            if (stored == null) return 1;

            try
            {
                return Clamp(Convert.ToInt32(stored));
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// Restricts a level to the ones the game actually has
        /// </summary>
        private static Int32 Clamp(Int32 level)
        {
            if (level < 1) return 1;
            if (level > Constants.Global.MAX_LEVEL) return Constants.Global.MAX_LEVEL;
            return level;
        }
#else // #if CTYPE_CHEATS
        /// <summary> Whether this build has cheats at all </summary>
        public const Boolean Available = false;

        /// <summary> Whether the player takes damage. Never, in a build without cheats </summary>
        public static Boolean Invincible => false;

        /// <summary> The level a new run begins on. Always the first, in a build without cheats </summary>
        public static Int32 StartLevel => 1;

        /// <summary> How Omega's unlock is decided. Always by the campaign, in a build without cheats </summary>
        public static CheatOverride OmegaUnlock => CheatOverride.DEFAULT;

        /// <summary> Whether bombs are free. Never, in a build without cheats </summary>
        public static Boolean InfiniteBombs => false;

        /// <summary>
        /// Does nothing. There is nothing stored to read, and nothing that could read it.
        /// </summary>
        public static void Load()
        {
        }
#endif // #if CTYPE_CHEATS
    }
}
