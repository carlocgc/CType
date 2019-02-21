namespace Type.Data
{
    /// <summary> Virtual button static data </summary>
    public static class ButtonData
    {
        /// <summary> The press state of the virtual button </summary>
        public enum State
        {
            RELEASED,
            PRESSED,
            HELD,
        }

        /// <summary> The type of the button </summary>
        public enum Type
        {
            FIRE,
            NUKE,
            PAUSE,
            RESUME
        }
    }
}
