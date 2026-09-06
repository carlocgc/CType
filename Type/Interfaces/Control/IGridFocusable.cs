using System;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// An adjustable item whose focus also has a horizontal position, so that a navigator moving
    /// up or down between two of them can keep the column instead of returning to the start of
    /// the row.
    /// </summary>
    /// <remarks>
    /// What makes a list of these read as a grid. Without it, moving down from the third cell of
    /// one row lands on the first cell of the next, which is right for a list of values and
    /// wrong for a table of them.
    /// <para>
    /// A navigator carries the column across items that are not grids too — stepping down onto a
    /// plain entry and back up returns to the column that was left, rather than to the start.
    /// </para>
    /// </remarks>
    public interface IGridFocusable : IAdjustable
    {
        /// <summary> How many columns the item has </summary>
        Int32 ColumnCount { get; }

        /// <summary>
        /// Which column currently holds focus. Setting it past either end clamps rather than
        /// wrapping, so a row with fewer columns than the one above does not move the cursor
        /// somewhere the player did not ask for.
        /// </summary>
        Int32 Column { get; set; }
    }
}
