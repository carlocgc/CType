using System;
using System.Collections.Generic;
using System.Text;
using Type.Input;
using Type.Services;

namespace Type.Data
{
    /// <summary>
    /// The player's input bindings, held by <see cref="StorageService"/> alongside the volumes
    /// and the high score.
    /// </summary>
    /// <remarks>
    /// Kept apart from <see cref="Settings"/> because a binding is not a scalar: it is a list of
    /// inputs per device, and reading one back has to survive a build that has since renamed or
    /// removed an input. The load, clamp and save shape is the same, and the same rule applies —
    /// a key is only written once the player has changed something, so an untouched action is
    /// simply absent and falls back to the default.
    /// </remarks>
    public static class ControlSettings
    {
        /// <summary> Prefix of the store key holding one action's inputs </summary>
        private const String KeyPrefix = "BIND_";

        /// <summary> Separates the keyboard half of a stored binding from the gamepad half </summary>
        private const Char DeviceSeparator = ';';

        /// <summary> Separates the inputs within one half of a stored binding </summary>
        private const Char InputSeparator = ',';

        /// <summary>
        /// Reads the saved bindings and applies them. Call once during content loading, before
        /// anything is listening for input.
        /// </summary>
        public static void Load()
        {
            InputBindings bindings = InputService.Instance.Bindings;
            if (bindings == null) return;

            foreach (ButtonData.Type action in InputBindings.Rebindable)
            {
                Object stored = StorageService.Instance.GetValue(KeyPrefix + action);
                if (stored == null) continue;

                Read(stored.ToString(), out List<GamepadButton> padButtons, out List<String> keys);
                bindings.Restore(action, padButtons, keys);
            }

            InputService.Instance.ReloadBindings();
        }

        /// <summary>
        /// Binds an action to one input and saves the result
        /// </summary>
        /// <param name="action"> The action to rebind </param>
        /// <param name="source"> The input the player pressed </param>
        /// <returns> Whether the mapping changed; false if the input is not allowed to be bound </returns>
        public static Boolean Rebind(ButtonData.Type action, InputSource source)
        {
            InputBindings bindings = InputService.Instance.Bindings;
            if (bindings == null || !bindings.Rebind(action, source)) return false;

            // Every rebindable action is written, not just this one. A rebind can move an input
            // off another action, so saving only the action the player chose would store a
            // mapping that does not match the one now in play.
            Save(bindings);
            InputService.Instance.ReloadBindings();
            return true;
        }

        /// <summary>
        /// Puts every binding back to the shipped default and saves that
        /// </summary>
        public static void ResetToDefaults()
        {
            InputBindings bindings = InputService.Instance.Bindings;
            if (bindings == null) return;

            bindings.CopyFrom(InputBindings.CreateDefaults());

            // Written out rather than removed, because the store has no way to delete a key. An
            // absent key means "use the default", and these are the defaults, so the two agree.
            Save(bindings);
            InputService.Instance.ReloadBindings();
        }

        /// <summary>
        /// Writes every rebindable action's inputs to the store
        /// </summary>
        private static void Save(InputBindings bindings)
        {
            foreach (ButtonData.Type action in InputBindings.Rebindable)
            {
                StorageService.Instance.SetValue(KeyPrefix + action, Write(bindings[action]));
            }
        }

        /// <summary>
        /// Renders one binding as "Space,Z;A,RIGHT_TRIGGER"
        /// </summary>
        /// <param name="binding"> The binding to render, may be null </param>
        /// <remarks>
        /// Enum members and key names are written by name rather than by ordinal, so reordering
        /// either enum cannot silently change what a saved binding means.
        /// </remarks>
        private static String Write(ActionBinding binding)
        {
            if (binding == null) return String.Empty;

            StringBuilder result = new StringBuilder();

            for (Int32 index = 0; index < binding.Keys.Count; index++)
            {
                if (index > 0) result.Append(InputSeparator);
                result.Append(binding.Keys[index]);
            }

            result.Append(DeviceSeparator);

            for (Int32 index = 0; index < binding.PadButtons.Count; index++)
            {
                if (index > 0) result.Append(InputSeparator);
                result.Append(binding.PadButtons[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Reads back what <see cref="Write"/> produced
        /// </summary>
        /// <param name="stored"> The stored text </param>
        /// <param name="padButtons"> Receives the gamepad buttons named </param>
        /// <param name="keys"> Receives the key names </param>
        /// <remarks>
        /// A name this build does not recognise is dropped rather than failing the whole read.
        /// Key names are not checked here at all: which strings name a key is a platform
        /// question, and the input provider already skips the ones it cannot resolve.
        /// </remarks>
        private static void Read(String stored, out List<GamepadButton> padButtons, out List<String> keys)
        {
            padButtons = new List<GamepadButton>();
            keys = new List<String>();

            String[] halves = stored.Split(DeviceSeparator);

            foreach (String key in halves[0].Split(InputSeparator))
            {
                if (key.Length > 0 && !keys.Contains(key)) keys.Add(key);
            }

            if (halves.Length < 2) return;

            foreach (String name in halves[1].Split(InputSeparator))
            {
                if (!Enum.TryParse(name, true, out GamepadButton button)) continue;
                if (button != GamepadButton.NONE && !padButtons.Contains(button)) padButtons.Add(button);
            }
        }
    }
}
