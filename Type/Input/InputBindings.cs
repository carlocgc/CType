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
        /// B is bound to both NUKE and CANCEL. That is safe because no menu binds NUKE any
        /// more: ship select uses a focus cursor rather than one face button per craft, so the
        /// two actions can never both be listened for on the same screen.
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
                    new[] { GamepadButton.B, GamepadButton.BACK },
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

                // The secret craft was reached on touch by holding all three ship cards at
                // once, a gesture with no cursor equivalent. Left stick click and V stand in
                // for it: both are deliberate enough not to be hit by accident, which is the
                // property the original gesture had.
                new ActionBinding(ButtonData.Type.SECRET,
                    new[] { GamepadButton.LEFT_STICK },
                    new[] { "V" }),
            });
        }
    }
}
