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
        /// <summary> Shown in place of an input name when a slot has nothing bound </summary>
        public const String Unbound = "NONE";

        /// <summary> How many inputs of each device an action can be bound to </summary>
        /// <remarks>
        /// Two, because the defaults ship two of each — Space and Z both fire — and the controls
        /// screen has to be able to express what the game ships with. It shows one cell per slot
        /// per device, so this is also how wide that screen is.
        /// </remarks>
        public const Int32 Slots = 2;

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
        /// Names the input in one slot of one device, as the controls screen shows it
        /// </summary>
        /// <param name="action"> The action to describe </param>
        /// <param name="gamepad"> Whether to read the gamepad buttons rather than the keys </param>
        /// <param name="slot"> Which of the action's inputs for that device, from zero </param>
        /// <returns> The input's name, or <see cref="Unbound"/> if the slot is empty </returns>
        public String DescribeSlot(ButtonData.Type action, Boolean gamepad, Int32 slot)
        {
            ActionBinding binding = this[action];
            if (binding == null || slot < 0) return Unbound;

            if (gamepad)
            {
                return slot < binding.PadButtons.Count ? DescribePad(binding.PadButtons[slot]) : Unbound;
            }

            return slot < binding.Keys.Count ? DescribeKey(binding.Keys[slot]) : Unbound;
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
        /// Whether an action may not take an input, because confirming or cancelling already
        /// uses it and the two would be live at the same time
        /// </summary>
        /// <param name="action"> The action being rebound </param>
        /// <param name="source"> The input the player pressed </param>
        /// <remarks>
        /// **This applies to PAUSE and the movement directions only.** It used to apply to every
        /// action, which was wrong in a way the defaults themselves prove: A is bound to FIRE
        /// *and* CONFIRM, B to NUKE *and* CANCEL, so the rule forbade reproducing what the game
        /// ships with. Anything the player rebound off A could never be put back.
        /// <para>
        /// The collision is only real where both actions are listened for at once. FIRE and NUKE
        /// are suppressed while paused and no menu listens for them, so they can share a face
        /// button with a menu action and do. PAUSE stays live while paused — it is what unpauses
        /// — and the four directions are live alongside CONFIRM and CANCEL on every menu, so for
        /// those the overlap would make the pause menu ambiguous or leave a mistake on this very
        /// screen impossible to undo.
        /// </para>
        /// </remarks>
        public Boolean IsReserved(ButtonData.Type action, InputSource source)
        {
            if (source == null) return true;
            if (!SharesAScreenWithMenuActions(action)) return false;

            return Holds(this[ButtonData.Type.CONFIRM], source)
                || Holds(this[ButtonData.Type.CANCEL], source);
        }

        /// <summary>
        /// Whether an action is ever dispatched at the same time as confirm and cancel
        /// </summary>
        private static Boolean SharesAScreenWithMenuActions(ButtonData.Type action)
        {
            return action == ButtonData.Type.PAUSE
                || action == ButtonData.Type.MENU_UP
                || action == ButtonData.Type.MENU_DOWN
                || action == ButtonData.Type.MENU_LEFT
                || action == ButtonData.Type.MENU_RIGHT;
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
        /// Binds one slot of one device to an input, leaving the action's other slots alone
        /// </summary>
        /// <param name="action"> The action to rebind </param>
        /// <param name="gamepad"> Whether the slot holds a gamepad button rather than a key </param>
        /// <param name="slot"> Which of the action's inputs for that device, from zero </param>
        /// <param name="source"> The input the player pressed, which must match the device </param>
        /// <returns> Whether the mapping changed </returns>
        /// <remarks>
        /// Per slot rather than per device. Replacing the whole device meant that touching a
        /// binding collapsed it to one input, so the two-of-each the game ships with could never
        /// be got back except by resetting everything.
        /// <para>
        /// Two rules keep an input from appearing twice. Taking one the action already holds in
        /// its other slot swaps the two rather than duplicating it. Taking one from another
        /// rebindable action removes it there, and if that leaves that action nothing on this
        /// device it inherits the input given up here, so a rebind cannot silently unbind
        /// something else. Overlaps with actions that are not rebindable are left alone, since
        /// the defaults rely on them: A is FIRE and CONFIRM both.
        /// </para>
        /// </remarks>
        public Boolean Rebind(ButtonData.Type action, Boolean gamepad, Int32 slot, InputSource source)
        {
            ActionBinding target = this[action];
            if (target == null || source == null || slot < 0) return false;
            if (source.IsGamepad != gamepad || IsReserved(action, source)) return false;

            Boolean changed = gamepad
                ? RebindPad(action, target, slot, source.Button)
                : RebindKey(action, target, slot, source.Key);

            if (changed) Revision++;
            return changed;
        }

        /// <summary>
        /// Puts a gamepad button in one of an action's slots
        /// </summary>
        private Boolean RebindPad(ButtonData.Type action, ActionBinding target, Int32 slot, GamepadButton button)
        {
            GamepadButton surrendered = slot < target.PadButtons.Count ? target.PadButtons[slot] : GamepadButton.NONE;
            if (surrendered == button) return false;

            Int32 held = target.PadButtons.IndexOf(button);
            if (held >= 0)
            {
                if (surrendered != GamepadButton.NONE) target.PadButtons[held] = surrendered;
                else target.PadButtons.RemoveAt(held);

                SetSlot(target.PadButtons, slot, button);
                return true;
            }

            foreach (ButtonData.Type other in Rebindable)
            {
                if (other == action) continue;

                ActionBinding binding = this[other];
                if (binding == null || !binding.PadButtons.Remove(button)) continue;
                if (binding.PadButtons.Count == 0 && surrendered != GamepadButton.NONE) binding.PadButtons.Add(surrendered);
            }

            SetSlot(target.PadButtons, slot, button);
            return true;
        }

        /// <summary>
        /// Puts a key in one of an action's slots
        /// </summary>
        private Boolean RebindKey(ButtonData.Type action, ActionBinding target, Int32 slot, String key)
        {
            String surrendered = slot < target.Keys.Count ? target.Keys[slot] : null;
            if (surrendered == key) return false;

            Int32 held = target.Keys.IndexOf(key);
            if (held >= 0)
            {
                if (surrendered != null) target.Keys[held] = surrendered;
                else target.Keys.RemoveAt(held);

                SetSlot(target.Keys, slot, key);
                return true;
            }

            foreach (ButtonData.Type other in Rebindable)
            {
                if (other == action) continue;

                ActionBinding binding = this[other];
                if (binding == null || !binding.Keys.Remove(key)) continue;
                if (binding.Keys.Count == 0 && surrendered != null) binding.Keys.Add(surrendered);
            }

            SetSlot(target.Keys, slot, key);
            return true;
        }

        /// <summary>
        /// Writes a value into a slot, appending when the slot is past the end
        /// </summary>
        /// <remarks>
        /// The lists are kept without gaps, so filling the second slot of an action that has
        /// only one input appends. Filling the second slot of an action that has none puts the
        /// input in the first, which is what the screen then shows.
        /// </remarks>
        private static void SetSlot<T>(List<T> list, Int32 slot, T value)
        {
            if (slot < list.Count) list[slot] = value;
            else list.Add(value);
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
        /// <remarks>
        /// **Each action's inputs are replaced in place, and the set of actions is not
        /// restructured.** This runs from inside an input dispatch — RESET DEFAULTS is a menu
        /// item, and the provider activates it from within a loop over these bindings. Clearing
        /// and refilling the dictionary invalidated that loop's enumerator, so resetting threw
        /// `InvalidOperationException` on the next action it went to dispatch.
        /// <para>
        /// An action the current mapping does not have is still added, which does restructure.
        /// That cannot happen while both sides come from <see cref="CreateDefaults"/>, which is
        /// the only way this is called; it is here so a future action is not silently dropped.
        /// </para>
        /// </remarks>
        public void CopyFrom(InputBindings bindings)
        {
            if (bindings == null) return;

            foreach (ActionBinding source in bindings.All)
            {
                ActionBinding target = this[source.Action];

                if (target == null)
                {
                    _Bindings[source.Action] = new ActionBinding(source.Action,
                        source.PadButtons.ToArray(), source.Keys.ToArray());
                    continue;
                }

                target.PadButtons.Clear();
                foreach (GamepadButton button in source.PadButtons) target.PadButtons.Add(button);

                target.Keys.Clear();
                foreach (String key in source.Keys) target.Keys.Add(key);
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
