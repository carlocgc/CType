using System;
using System.Collections.Generic;
using System.Text;
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
        /// <summary> Shown in place of an input name when an action has nothing bound </summary>
        public const String Unbound = "NONE";

        /// <summary>
        /// Separates the inputs when an action has more than one bound to the same device
        /// </summary>
        /// <remarks>
        /// Spaces rather than a comma: the bitmap font has no comma, and adding one means
        /// widening the atlas and appending to <c>Constants.Font.Map</c> in the same order. Not
        /// worth it for a separator, and the atlas ordering is a trap — see ROADMAP item S7.
        /// </remarks>
        private const String InputSeparator = "  ";

        /// <summary>
        /// The actions the player may rebind, in the order the controls screen lists them
        /// </summary>
        /// <remarks>
        /// CONFIRM and CANCEL are deliberately absent, and <see cref="IsReserved"/> refuses any
        /// captured input already bound to one of them. They are the only way off the controls
        /// screen, so a binding that made them ambiguous could leave a player unable to undo it.
        /// START and BACK are absent because nothing on desktop listens for them; they exist for
        /// the dormant Android build.
        /// </remarks>
        public static readonly ButtonData.Type[] Rebindable =
        {
            ButtonData.Type.FIRE,
            ButtonData.Type.NUKE,
            ButtonData.Type.PAUSE,
            ButtonData.Type.MENU_UP,
            ButtonData.Type.MENU_DOWN,
            ButtonData.Type.MENU_LEFT,
            ButtonData.Type.MENU_RIGHT,
        };

        /// <summary> Binding for each bound action </summary>
        private readonly Dictionary<ButtonData.Type, ActionBinding> _Bindings;

        /// <summary> Every binding in the set </summary>
        public IEnumerable<ActionBinding> All => _Bindings.Values;

        /// <summary>
        /// Incremented on every change to the mapping, so anything displaying a binding can tell
        /// that it has gone stale without comparing the bindings themselves
        /// </summary>
        public Int32 Revision { get; private set; }

        /// <summary> Returns the binding for an action, or null if the action is unbound </summary>
        public ActionBinding this[ButtonData.Type action] =>
            _Bindings.TryGetValue(action, out ActionBinding binding) ? binding : null;

        /// <summary>
        /// Returns a short label naming the input a player should press for an action, for use
        /// in on screen prompts
        /// </summary>
        /// <param name="action"> The action to describe </param>
        /// <param name="gamepad"> Whether to describe the gamepad input rather than the key </param>
        /// <returns> An upper case label, or an empty string if the action is unbound </returns>
        /// <remarks>
        /// The first binding is used, so the order they are declared in is the order of
        /// preference for prompts. Output is restricted to the characters the bitmap font can
        /// render: A to Z, 0 to 9 and space.
        /// </remarks>
        public String GetPromptLabel(ButtonData.Type action, Boolean gamepad)
        {
            ActionBinding binding = this[action];
            if (binding == null) return String.Empty;

            if (gamepad)
            {
                if (binding.PadButtons.Count == 0) return String.Empty;
                return DescribePad(binding.PadButtons[0]);
            }

            return binding.Keys.Count == 0 ? String.Empty : DescribeKey(binding.Keys[0]);
        }

        /// <summary>
        /// Lists every input bound to an action for one device, as the controls screen shows it
        /// </summary>
        /// <param name="action"> The action to describe </param>
        /// <param name="gamepad"> Whether to list the gamepad buttons rather than the keys </param>
        /// <returns> The input names separated by commas, or <see cref="Unbound"/> if there are none </returns>
        public String DescribeAll(ButtonData.Type action, Boolean gamepad)
        {
            ActionBinding binding = this[action];
            if (binding == null) return Unbound;

            StringBuilder result = new StringBuilder();

            if (gamepad)
            {
                foreach (GamepadButton button in binding.PadButtons) Append(result, DescribePad(button));
            }
            else
            {
                foreach (String key in binding.Keys) Append(result, DescribeKey(key));
            }

            return result.Length == 0 ? Unbound : result.ToString();
        }

        /// <summary>
        /// Adds one input name to a separated list
        /// </summary>
        private static void Append(StringBuilder result, String label)
        {
            if (label.Length == 0) return;
            if (result.Length > 0) result.Append(InputSeparator);
            result.Append(label);
        }

        /// <summary>
        /// Drops any character the bitmap font has no glyph for
        /// </summary>
        /// <param name="text"> The label to filter </param>
        /// <remarks>
        /// Applied to the input names this class does not spell out itself, which come from a
        /// platform's key enum and so are not a fixed set. <c>TextDisplay</c> looks every
        /// character up in <c>Constants.Font.Map</c> without checking, and throws on one that is
        /// missing, so an unrenderable name would take the game down rather than look wrong.
        /// </remarks>
        private static String Renderable(String text)
        {
            Dictionary<Char, String> font = Constants.Font.Map;
            StringBuilder result = new StringBuilder(text.Length);

            foreach (Char character in text)
            {
                if (font.ContainsKey(character)) result.Append(character);
            }

            return result.ToString();
        }

        /// <summary>
        /// Whether an input may not be rebound, because an action that has to stay unambiguous
        /// already uses it
        /// </summary>
        /// <param name="source"> The input the player pressed </param>
        /// <remarks>
        /// Confirming and cancelling are how a player leaves the controls screen. If a rebind
        /// could give either of those inputs a second meaning, a mistake made on that screen
        /// might not be undoable from it.
        /// </remarks>
        public Boolean IsReserved(InputSource source)
        {
            if (source == null) return true;

            return Holds(this[ButtonData.Type.CONFIRM], source)
                || Holds(this[ButtonData.Type.CANCEL], source);
        }

        /// <summary>
        /// Whether a binding already contains an input
        /// </summary>
        private static Boolean Holds(ActionBinding binding, InputSource source)
        {
            if (binding == null) return false;

            return source.IsGamepad
                ? binding.PadButtons.Contains(source.Button)
                : binding.Keys.Contains(source.Key);
        }

        /// <summary>
        /// Binds an action to one input, replacing whatever it had bound for that device
        /// </summary>
        /// <param name="action"> The action to rebind </param>
        /// <param name="source"> The input the player pressed </param>
        /// <returns> Whether the mapping changed </returns>
        /// <remarks>
        /// The action loses its other inputs for that device rather than keeping them as
        /// alternatives, so what the controls screen shows is the whole truth: an action bound
        /// to one key does not quietly still answer to another.
        /// <para>
        /// An input taken from another rebindable action is swapped rather than merely removed —
        /// that action inherits the input given up here — so a rebind can never leave a second
        /// action with nothing bound. Overlaps with actions that are not rebindable are left
        /// alone, <see cref="IsReserved"/> having already refused the ones that would matter.
        /// </para>
        /// </remarks>
        public Boolean Rebind(ButtonData.Type action, InputSource source)
        {
            ActionBinding target = this[action];
            if (target == null || source == null || IsReserved(source)) return false;

            if (source.IsGamepad) RebindPad(action, target, source.Button);
            else RebindKey(action, target, source.Key);

            Revision++;
            return true;
        }

        /// <summary>
        /// Gives an action one gamepad button, handing whatever it gives up to any rebindable
        /// action that button is taken from
        /// </summary>
        private void RebindPad(ButtonData.Type action, ActionBinding target, GamepadButton button)
        {
            GamepadButton surrendered = target.PadButtons.Count > 0 ? target.PadButtons[0] : GamepadButton.NONE;

            foreach (ButtonData.Type other in Rebindable)
            {
                if (other == action) continue;

                ActionBinding binding = this[other];
                if (binding == null || !binding.PadButtons.Remove(button)) continue;
                if (binding.PadButtons.Count == 0 && surrendered != GamepadButton.NONE) binding.PadButtons.Add(surrendered);
            }

            target.PadButtons.Clear();
            target.PadButtons.Add(button);
        }

        /// <summary>
        /// Gives an action one key, handing whatever it gives up to any rebindable action that
        /// key is taken from
        /// </summary>
        private void RebindKey(ButtonData.Type action, ActionBinding target, String key)
        {
            String surrendered = target.Keys.Count > 0 ? target.Keys[0] : null;

            foreach (ButtonData.Type other in Rebindable)
            {
                if (other == action) continue;

                ActionBinding binding = this[other];
                if (binding == null || !binding.Keys.Remove(key)) continue;
                if (binding.Keys.Count == 0 && surrendered != null) binding.Keys.Add(surrendered);
            }

            target.Keys.Clear();
            target.Keys.Add(key);
        }

        /// <summary>
        /// Replaces one action's inputs wholesale, for restoring a mapping that was saved
        /// </summary>
        /// <param name="action"> The action to set </param>
        /// <param name="padButtons"> Gamepad buttons to bind, may be empty </param>
        /// <param name="keys"> Key names to bind, may be empty </param>
        /// <returns> Whether the mapping changed </returns>
        /// <remarks>
        /// A restore that would leave the action with nothing bound at all is refused, so a save
        /// written by a build that named its inputs differently falls back to the defaults
        /// rather than to an action the player cannot perform. Conflicts are not resolved: the
        /// saved mapping is taken as it stands, having already been through
        /// <see cref="Rebind"/> when it was made.
        /// </remarks>
        public Boolean Restore(ButtonData.Type action, IList<GamepadButton> padButtons, IList<String> keys)
        {
            ActionBinding binding = this[action];
            if (binding == null || padButtons == null || keys == null) return false;
            if (padButtons.Count == 0 && keys.Count == 0) return false;

            binding.PadButtons.Clear();
            foreach (GamepadButton button in padButtons) binding.PadButtons.Add(button);

            binding.Keys.Clear();
            foreach (String key in keys) binding.Keys.Add(key);

            Revision++;
            return true;
        }

        /// <summary>
        /// Replaces every binding with a copy of the ones given, for adopting a saved or default
        /// mapping without swapping the instance the input provider holds
        /// </summary>
        /// <param name="bindings"> The mapping to adopt </param>
        public void CopyFrom(InputBindings bindings)
        {
            if (bindings == null) return;

            _Bindings.Clear();
            foreach (ActionBinding binding in bindings.All)
            {
                _Bindings[binding.Action] = new ActionBinding(binding.Action,
                    binding.PadButtons.ToArray(), binding.Keys.ToArray());
            }

            Revision++;
        }

        /// <summary>
        /// Names a gamepad button the way a player would refer to it
        /// </summary>
        public static String DescribePad(GamepadButton button)
        {
            switch (button)
            {
                case GamepadButton.LEFT_SHOULDER: return "LB";
                case GamepadButton.RIGHT_SHOULDER: return "RB";
                case GamepadButton.LEFT_TRIGGER: return "LT";
                case GamepadButton.RIGHT_TRIGGER: return "RT";
                case GamepadButton.LEFT_STICK: return "L STICK";
                case GamepadButton.RIGHT_STICK: return "R STICK";
                case GamepadButton.DPAD_UP: return "DPAD UP";
                case GamepadButton.DPAD_DOWN: return "DPAD DOWN";
                case GamepadButton.DPAD_LEFT: return "DPAD LEFT";
                case GamepadButton.DPAD_RIGHT: return "DPAD RIGHT";
                default: return Renderable(button.ToString());
            }
        }

        /// <summary>
        /// Names a key the way a player would refer to it, shortening the ones whose enum name
        /// reads badly on screen
        /// </summary>
        public static String DescribeKey(String key)
        {
            switch (key)
            {
                case "Escape": return "ESC";
                case "BackSpace": return "BACKSPACE";
                case "Enter": return "ENTER";
                default: return Renderable(key.ToUpperInvariant());
            }
        }

        /// <summary>
        /// Names an action the way the controls screen labels it
        /// </summary>
        /// <param name="action"> The action to label </param>
        /// <remarks>
        /// The four menu directions double as the ship's movement, which is what a player is
        /// looking for on a controls screen, so they are labelled as movement rather than by
        /// their enum name.
        /// </remarks>
        public static String DescribeAction(ButtonData.Type action)
        {
            switch (action)
            {
                case ButtonData.Type.MENU_UP: return "MOVE UP";
                case ButtonData.Type.MENU_DOWN: return "MOVE DOWN";
                case ButtonData.Type.MENU_LEFT: return "MOVE LEFT";
                case ButtonData.Type.MENU_RIGHT: return "MOVE RIGHT";
                default: return action.ToString();
            }
        }

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
        /// <para>
        /// PAUSE is its own action rather than a second meaning for START or CANCEL, and pad
        /// Start is deliberately not bound to CONFIRM. It was, and the result was that pausing
        /// built a menu which the same frame's CONFIRM immediately activated, resuming again,
        /// so pausing appeared to do nothing at all. CANCEL cannot double as pause either,
        /// because B is bound to NUKE while playing.
        /// </para>
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
                    new[] { GamepadButton.A },
                    new[] { "Enter", "Space" }),

                new ActionBinding(ButtonData.Type.PAUSE,
                    new[] { GamepadButton.START },
                    new[] { "Escape", "P" }),

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
            });
        }
    }
}
