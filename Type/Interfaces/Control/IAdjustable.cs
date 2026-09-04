using System;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// A focusable item that holds a value the player changes in place, such as a volume level,
    /// rather than one that is simply confirmed.
    /// </summary>
    /// <remarks>
    /// A navigator moves focus with up and down, and sends left and right to the focused item
    /// when it is adjustable. Items that are not adjustable keep the older behaviour where left
    /// and right also move focus, which is what a single row of choices such as ship select
    /// wants.
    /// </remarks>
    public interface IAdjustable : IFocusable
    {
        /// <summary>
        /// Changes the held value by one step
        /// </summary>
        /// <param name="direction"> -1 to decrease, 1 to increase </param>
        void Adjust(Int32 direction);
    }
}
