using AmosShared.Audio;
using AmosShared.Base;
using System;
using Type.Services;

namespace Type.Data
{
    /// <summary>
    /// Player settings that survive between sessions, held by <see cref="StorageService"/>
    /// alongside the high score.
    /// </summary>
    /// <remarks>
    /// Volumes are stored as whole percentages rather than the engine's 0 to 1 floats. The store
    /// round trips values as text, so a float would have to be parsed back with a guess at its
    /// original type; an integer percentage is also what the options screen shows, so there is
    /// one representation rather than two.
    /// </remarks>
    public static class Settings
    {
        /// <summary> Store key for the master volume percentage </summary>
        private const String MasterVolumeKey = "MASTER_VOLUME";
        /// <summary> Store key for the music volume percentage </summary>
        private const String MusicVolumeKey = "MUSIC_VOLUME";
        /// <summary> Store key for the effect volume percentage </summary>
        private const String EffectVolumeKey = "EFFECT_VOLUME";
        /// <summary> Store key for the display mode </summary>
        private const String DisplayModeKey = "DISPLAY_MODE";
        /// <summary> Store key for the rumble intensity percentage </summary>
        private const String RumbleIntensityKey = "RUMBLE_INTENSITY";

        /// <summary> Volume percentage used when nothing has been saved </summary>
        private const Int32 DefaultVolume = 100;

        /// <summary> Master volume, 0 to 100 </summary>
        public static Int32 MasterVolume { get; private set; } = DefaultVolume;

        /// <summary> Music volume, 0 to 100 </summary>
        public static Int32 MusicVolume { get; private set; } = DefaultVolume;

        /// <summary> Effect volume, 0 to 100 </summary>
        public static Int32 EffectVolume { get; private set; } = DefaultVolume;

        /// <summary> How the game window fills the display </summary>
        public static DisplayMode DisplayMode { get; private set; } = DisplayMode.WINDOWED;

        /// <summary>
        /// How hard the controller rumbles, 0 to 100, where 0 turns it off entirely
        /// </summary>
        /// <remarks>
        /// A percentage like the volumes rather than an on and off switch. Rumble is one of the
        /// first things a player turns down rather than off, and the row costs nothing extra.
        /// </remarks>
        public static Int32 RumbleIntensity { get; private set; } = DefaultVolume;

        /// <summary>
        /// Reads the saved settings and applies them. Call once during content loading, before
        /// anything plays.
        /// </summary>
        public static void Load()
        {
            MasterVolume = ReadPercentage(MasterVolumeKey);
            MusicVolume = ReadPercentage(MusicVolumeKey);
            EffectVolume = ReadPercentage(EffectVolumeKey);
            RumbleIntensity = ReadPercentage(RumbleIntensityKey);
            DisplayMode = ReadDisplayMode();

            Apply();
        }

        /// <summary>
        /// Sets the master volume and saves it
        /// </summary>
        /// <param name="percentage"> The new value, clamped to 0 to 100 </param>
        public static void SetMasterVolume(Int32 percentage)
        {
            MasterVolume = Clamp(percentage);
            StorageService.Instance.SetValue(MasterVolumeKey, MasterVolume);
            Apply();
        }

        /// <summary>
        /// Sets the music volume and saves it
        /// </summary>
        /// <param name="percentage"> The new value, clamped to 0 to 100 </param>
        public static void SetMusicVolume(Int32 percentage)
        {
            MusicVolume = Clamp(percentage);
            StorageService.Instance.SetValue(MusicVolumeKey, MusicVolume);
            Apply();
        }

        /// <summary>
        /// Sets the effect volume and saves it
        /// </summary>
        /// <param name="percentage"> The new value, clamped to 0 to 100 </param>
        public static void SetEffectVolume(Int32 percentage)
        {
            EffectVolume = Clamp(percentage);
            StorageService.Instance.SetValue(EffectVolumeKey, EffectVolume);
            Apply();
        }

        /// <summary>
        /// Sets the rumble intensity and saves it
        /// </summary>
        /// <param name="percentage"> The new value, clamped to 0 to 100 </param>
        /// <remarks>
        /// Nothing to apply: <see cref="Services.InputService"/> reads this each time it is asked
        /// to rumble, so there is no copy of it held anywhere to keep in step.
        /// </remarks>
        public static void SetRumbleIntensity(Int32 percentage)
        {
            RumbleIntensity = Clamp(percentage);
            StorageService.Instance.SetValue(RumbleIntensityKey, RumbleIntensity);
        }

        /// <summary>
        /// Sets the display mode and saves it
        /// </summary>
        /// <param name="mode"> The mode to apply </param>
        public static void SetDisplayMode(DisplayMode mode)
        {
            DisplayMode = mode;
            StorageService.Instance.SetValue(DisplayModeKey, mode.ToString());
            DisplayService.Instance.SetMode(mode);
        }

        /// <summary>
        /// Reads the stored display mode, falling back to a window if it is missing or names a
        /// mode this build no longer has
        /// </summary>
        private static DisplayMode ReadDisplayMode()
        {
            Object stored = StorageService.Instance.GetValue(DisplayModeKey);
            if (stored == null) return DisplayMode.WINDOWED;

            // Stored by name rather than ordinal, so reordering the enum cannot silently change
            // what a saved setting means.
            return Enum.TryParse(stored.ToString(), true, out DisplayMode mode) ? mode : DisplayMode.WINDOWED;
        }

        /// <summary>
        /// Pushes the current values onto the audio manager
        /// </summary>
        private static void Apply()
        {
            AudioManager.Instance.MasterVolume = MasterVolume / 100f;
            AudioManager.Instance.MusicVolume = MusicVolume / 100f;
            AudioManager.Instance.EffectVolume = EffectVolume / 100f;
            DisplayService.Instance.SetMode(DisplayMode);
        }

        /// <summary>
        /// Reads a stored percentage, falling back to the default when it is missing or was
        /// written by an older build in a form that no longer parses
        /// </summary>
        private static Int32 ReadPercentage(String key)
        {
            Object stored = StorageService.Instance.GetValue(key);
            if (stored == null) return DefaultVolume;

            try
            {
                return Clamp(Convert.ToInt32(stored));
            }
            catch (Exception)
            {
                // A corrupt or unexpected value must not stop the game starting.
                return DefaultVolume;
            }
        }

        /// <summary>
        /// Restricts a percentage to the range the interface allows
        /// </summary>
        private static Int32 Clamp(Int32 percentage)
        {
            if (percentage < 0) return 0;
            return percentage > 100 ? 100 : percentage;
        }
    }
}
