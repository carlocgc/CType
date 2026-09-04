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
        /// These are input actions only. Members that carried game meaning — one per selectable
        /// craft, which is why ship select used to read as "A chooses Alpha" — were removed once
        /// menu navigation replaced face button selection.
        /// </remarks>
        public enum Type
        {
            FIRE,
            NUKE,
            START,
            BACK,

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

            /// <summary> Reveals the hidden craft on the ship select screen </summary>
            SECRET,
        }
    }
}
