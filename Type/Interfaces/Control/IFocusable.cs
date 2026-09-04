using System;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// An item that a <see cref="Type.UI.Navigation.MenuNavigator"/> can move focus to and
    /// activate, so that a menu is usable without a pointing device.
    /// </summary>
    public interface IFocusable
    {
        /// <summary> Whether the item can currently take focus; hidden or disabled items cannot </summary>
        Boolean CanFocus { get; }

        /// <summary> Called when the item gains or loses focus, so it can show that state </summary>
        /// <param name="focused"> Whether the item now has focus </param>
        void SetFocused(Boolean focused);

        /// <summary> Called when the focused item is confirmed </summary>
        void Activate();
    }
}
