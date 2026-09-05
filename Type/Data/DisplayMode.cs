namespace Type.Data
{
    /// <summary>
    /// How the game window occupies the display
    /// </summary>
    public enum DisplayMode
    {
        /// <summary> A bordered window smaller than the desktop </summary>
        WINDOWED,

        /// <summary> Borderless and filling the desktop, so alt tab stays instant </summary>
        BORDERLESS,

        /// <summary> Exclusive fullscreen </summary>
        FULLSCREEN,
    }
}
