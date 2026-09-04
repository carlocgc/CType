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

        /// <summary>
        /// The type of button, used as the action identifier throughout the input system
        /// </summary>
        /// <remarks>
        /// The *_SELECT members mix game meaning into input identity, which is why ship select
        /// reads as "A chooses Alpha" rather than as a cursor. They are kept until menu
        /// navigation replaces face button selection; see ROADMAP item I5.
        /// </remarks>
        public enum Type
        {
            FIRE,
            NUKE,
            START,
            BACK,
            ALPHA_SELECT,
            BETA_SELECT,
            GAMMA_SELECT,
            OMEGA_SELECT,

            /// <summary> Accept the focused menu item </summary>
            CONFIRM,
            /// <summary> Dismiss the current menu or go back </summary>
            CANCEL,
            /// <summary> Move menu focus up </summary>
            MENU_UP,
            /// <summary> Move menu focus down </summary>
            MENU_DOWN,
            /// <summary> Move menu focus left </summary>
            MENU_LEFT,
            /// <summary> Move menu focus right </summary>
            MENU_RIGHT,
        }
    }
}
