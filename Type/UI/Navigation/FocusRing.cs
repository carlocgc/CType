using System;
using System.Collections.Generic;
using Type.Interfaces.Control;

namespace Type.UI.Navigation
{
    /// <summary>
    /// An ordered set of <see cref="IFocusable"/> items with exactly one of them focused.
    /// Moving wraps at both ends and skips items that cannot currently take focus.
    /// </summary>
    /// <remarks>
    /// Deliberately free of any engine dependency so the focus rules can be exercised without
    /// standing up input or update managers. <see cref="MenuNavigator"/> owns one of these and
    /// supplies the input plumbing.
    /// </remarks>
    public sealed class FocusRing
    {
        /// <summary> Items focus can move between, in navigation order </summary>
        private readonly List<IFocusable> _Items = new List<IFocusable>();

        /// <summary> Index of the focused item, negative when nothing is focused </summary>
        public Int32 Index { get; private set; } = -1;

        /// <summary> Number of items in the ring </summary>
        public Int32 Count => _Items.Count;

        /// <summary> The focused item, or null when nothing is focused </summary>
        public IFocusable Focused => Index >= 0 && Index < _Items.Count ? _Items[Index] : null;

        /// <summary>
        /// Adds an item to the end of the navigation order, ignoring duplicates
        /// </summary>
        /// <param name="item"> The item focus can move to </param>
        /// <remarks>
        /// The item is put into the unfocused state as it joins. Without this an item that has
        /// never been moved to keeps whatever appearance it was constructed with, so a freshly
        /// built menu shows every item looking focused until the cursor has visited each one.
        /// </remarks>
        public void Add(IFocusable item)
        {
            if (item == null || _Items.Contains(item)) return;
            _Items.Add(item);
            item.SetFocused(false);
        }

        /// <summary>
        /// Focuses the first item that can take focus. Does nothing if none can.
        /// </summary>
        public void FocusFirst()
        {
            for (Int32 i = 0; i < _Items.Count; i++)
            {
                if (!_Items[i].CanFocus) continue;
                SetIndex(i);
                return;
            }
        }

        /// <summary>
        /// Moves focus by the given number of places, skipping items that cannot take focus
        /// and wrapping at both ends
        /// </summary>
        /// <param name="step"> How far to move, negative to move backwards </param>
        public void Move(Int32 step)
        {
            if (_Items.Count == 0 || step == 0) return;

            // With nothing focused yet, start so that the first step lands on the near end for
            // the direction travelled: forward onto the first item, backward onto the last.
            Int32 candidate = Index >= 0
                ? Index
                : step > 0 ? _Items.Count - 1 : 0;

            // At most one full lap, so a ring with nothing focusable cannot spin forever.
            for (Int32 i = 0; i < _Items.Count; i++)
            {
                candidate = ((candidate + step) % _Items.Count + _Items.Count) % _Items.Count;
                if (!_Items[candidate].CanFocus) continue;
                SetIndex(candidate);
                return;
            }
        }

        /// <summary>
        /// Focuses the item at the given index, unfocusing whatever held it before
        /// </summary>
        private void SetIndex(Int32 index)
        {
            if (Index == index) return;

            Focused?.SetFocused(false);
            Index = index;
            Focused?.SetFocused(true);
        }

        /// <summary>
        /// Unfocuses the current item and empties the ring
        /// </summary>
        public void Clear()
        {
            Focused?.SetFocused(false);
            Index = -1;
            _Items.Clear();
        }
    }
}
