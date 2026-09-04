using System;
using System.Collections.Generic;
using Type.Data;

namespace Type.Input
{
    /// <summary>
    /// The set of physical inputs bound to a single game action. An action fires when any one
    /// of its bound inputs is active, so an action can be driven by a pad and a key at once.
    /// </summary>
    public sealed class ActionBinding
    {
        /// <summary> The action these inputs trigger </summary>
        public ButtonData.Type Action { get; }

        /// <summary> Gamepad buttons bound to the action </summary>
        public List<GamepadButton> PadButtons { get; }

        /// <summary>
        /// Names of the keyboard keys bound to the action. Held as strings so that shared code
        /// does not need to know any platform's key enum; the platform input provider resolves
        /// them once when the bindings are applied.
        /// </summary>
        public List<String> Keys { get; }

        /// <summary>
        /// Creates a binding for an action
        /// </summary>
        /// <param name="action"> The action the inputs trigger </param>
        /// <param name="padButtons"> Gamepad buttons bound to the action </param>
        /// <param name="keys"> Names of keyboard keys bound to the action </param>
        public ActionBinding(ButtonData.Type action, GamepadButton[] padButtons, String[] keys)
        {
            Action = action;
            PadButtons = new List<GamepadButton>(padButtons);
            Keys = new List<String>(keys);
        }
    }
}
