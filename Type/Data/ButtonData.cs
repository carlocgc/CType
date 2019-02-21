namespace Type.Data
{
    /// <summary> Button static data </summary>
    public static class ButtonData
    {
        /// <summary> The press state of button </summary>
        public enum State
        {
            RELEASED,
            PRESSED,
            HELD,
        }

        /// <summary> The type of button </summary>
        public enum Type
        {
            FIRE,
            NUKE,
            PAUSE,
            RESUME,
            START,
            BACK,
            ALPHA_SELECT,
            BETA_SELECT,
            GAMMA_SELECT,
            OMEGA_SELECT
        }
    }
}
