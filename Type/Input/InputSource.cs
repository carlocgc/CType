using System;

namespace Type.Input
{
    /// <summary>
    /// One physical input, either a gamepad button or a keyboard key. Produced by a platform
    /// input provider while capturing for the rebinding screen, so that shared code can be
    /// handed "what the player just pressed" without knowing any platform's key enum.
    /// </summary>
    public sealed class InputSource
    {
        /// <summary> The gamepad button, or <see cref="GamepadButton.NONE"/> for a key </summary>
        public GamepadButton Button { get; }

        /// <summary> The key name, or null for a gamepad button </summary>
        public String Key { get; }

        /// <summary> Whether this names a gamepad button rather than a key </summary>
        public Boolean IsGamepad => Button != GamepadButton.NONE;

        /// <summary> How the input should be named on screen </summary>
        public String Label => IsGamepad ? InputBindings.DescribePad(Button) : InputBindings.DescribeKey(Key);

        private InputSource(GamepadButton button, String key)
        {
            Button = button;
            Key = key;
        }

        /// <summary>
        /// Names a gamepad button
        /// </summary>
        /// <param name="button"> The button pressed </param>
        public static InputSource FromPad(GamepadButton button)
        {
            return new InputSource(button, null);
        }

        /// <summary>
        /// Names a keyboard key
        /// </summary>
        /// <param name="key"> The key's platform independent name, as stored in a binding </param>
        public static InputSource FromKey(String key)
        {
            return new InputSource(GamepadButton.NONE, key);
        }
    }
}
