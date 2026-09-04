using System;
using System.Collections.Generic;
using Type.Data;

namespace Type.Input
{
    /// <summary>
    /// The complete action to input mapping used by a platform input provider. Replaces the
    /// hardcoded polling chain the desktop provider used to carry, so that adding an input or
    /// rebinding an action is a data change rather than a code change.
    /// </summary>
    public sealed class InputBindings
    {
        /// <summary> Binding for each bound action </summary>
        private readonly Dictionary<ButtonData.Type, ActionBinding> _Bindings;

        /// <summary> Every binding in the set </summary>
        public IEnumerable<ActionBinding> All => _Bindings.Values;

        /// <summary> Returns the binding for an action, or null if the action is unbound </summary>
        public ActionBinding this[ButtonData.Type action] =>
            _Bindings.TryGetValue(action, out ActionBinding binding) ? binding : null;

        private InputBindings(IEnumerable<ActionBinding> bindings)
        {
            _Bindings = new Dictionary<ButtonData.Type, ActionBinding>();
            foreach (ActionBinding binding in bindings)
            {
                _Bindings[binding.Action] = binding;
            }
        }

        /// <summary>
        /// Creates the default bindings.
        /// </summary>
        /// <remarks>
        /// Two deliberate omissions, both because <see cref="ButtonData.Type"/> still mixes
        /// input identity with game meaning:
        /// the B button is not bound to BACK or CANCEL, because ship select currently uses
        /// NUKE to choose a craft and B would both select and cancel; and GAMMA_SELECT remains
        /// a button type rather than a menu action. Both are resolved by the menu navigation
        /// work, at which point CANCEL should also pick up B. See ROADMAP items I1 and I5.
        /// </remarks>
        public static InputBindings CreateDefaults()
        {
            return new InputBindings(new[]
            {
                new ActionBinding(ButtonData.Type.FIRE,
                    new[] { GamepadButton.A, GamepadButton.RIGHT_TRIGGER },
                    new[] { "Space", "Z" }),

                new ActionBinding(ButtonData.Type.NUKE,
                    new[] { GamepadButton.B, GamepadButton.LEFT_TRIGGER },
                    new[] { "F", "X" }),

                new ActionBinding(ButtonData.Type.START,
                    new[] { GamepadButton.START },
                    new[] { "Enter", "C" }),

                new ActionBinding(ButtonData.Type.BACK,
                    new[] { GamepadButton.BACK },
                    new[] { "Escape", "BackSpace" }),

                new ActionBinding(ButtonData.Type.CONFIRM,
                    new[] { GamepadButton.A, GamepadButton.START },
                    new[] { "Enter", "Space" }),

                new ActionBinding(ButtonData.Type.CANCEL,
                    new[] { GamepadButton.BACK },
                    new[] { "Escape", "BackSpace" }),

                new ActionBinding(ButtonData.Type.MENU_UP,
                    new[] { GamepadButton.DPAD_UP },
                    new[] { "Up", "W" }),

                new ActionBinding(ButtonData.Type.MENU_DOWN,
                    new[] { GamepadButton.DPAD_DOWN },
                    new[] { "Down", "S" }),

                new ActionBinding(ButtonData.Type.MENU_LEFT,
                    new[] { GamepadButton.DPAD_LEFT },
                    new[] { "Left", "A" }),

                new ActionBinding(ButtonData.Type.MENU_RIGHT,
                    new[] { GamepadButton.DPAD_RIGHT },
                    new[] { "Right", "D" }),

                new ActionBinding(ButtonData.Type.GAMMA_SELECT,
                    new[] { GamepadButton.Y },
                    new[] { "V" }),
            });
        }
    }
}
