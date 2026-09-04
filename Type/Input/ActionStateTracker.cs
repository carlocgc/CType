using System;
using System.Collections.Generic;
using Type.Data;

namespace Type.Input
{
    /// <summary>
    /// Tracks the pressed state of each action between updates and turns a per update "is this
    /// action down" reading into edge triggered state changes.
    /// </summary>
    /// <remarks>
    /// The desktop provider previously reported PRESSED or RELEASED on every update for every
    /// button, so listeners saw a continuous stream of events and each had to guard against
    /// retriggering with its own bookkeeping. With this, PRESSED and RELEASED are each reported
    /// exactly once per press and HELD is reported while the action stays down.
    /// </remarks>
    public sealed class ActionStateTracker
    {
        /// <summary> Whether each action was down on the previous update </summary>
        private readonly Dictionary<ButtonData.Type, Boolean> _WasDown = new Dictionary<ButtonData.Type, Boolean>();

        /// <summary>
        /// Records the current reading for an action and returns the state to report, if any
        /// </summary>
        /// <param name="action"> The action being polled </param>
        /// <param name="isDown"> Whether any input bound to the action is currently down </param>
        /// <param name="state"> The state to report to listeners </param>
        /// <returns> True if the action changed state or is being held, false if there is nothing to report </returns>
        public Boolean TryGetState(ButtonData.Type action, Boolean isDown, out ButtonData.State state)
        {
            _WasDown.TryGetValue(action, out Boolean wasDown);
            _WasDown[action] = isDown;

            if (isDown)
            {
                state = wasDown ? ButtonData.State.HELD : ButtonData.State.PRESSED;
                return true;
            }

            state = ButtonData.State.RELEASED;
            return wasDown;
        }

        /// <summary>
        /// Forgets all tracked state. The next update reports PRESSED for anything still down.
        /// </summary>
        public void Reset()
        {
            _WasDown.Clear();
        }
    }
}
