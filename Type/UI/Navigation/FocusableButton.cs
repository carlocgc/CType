using AmosShared.Touch;
using OpenTK;
using System;
using Type.Interfaces.Control;

namespace Type.UI.Navigation
{
    /// <summary>
    /// Makes an engine <see cref="Button"/> focusable, so an existing touch button can be
    /// driven by a gamepad or the keyboard without changing how it behaves for a pointer.
    /// </summary>
    public sealed class FocusableButton : IFocusable
    {
        /// <summary> Tint applied while the button does not have focus </summary>
        private static readonly Vector4 Unfocused = new Vector4(0.55f, 0.55f, 0.55f, 1);
        /// <summary> Tint applied while the button has focus </summary>
        private static readonly Vector4 Focused = new Vector4(1, 1, 1, 1);

        /// <summary> The button being wrapped </summary>
        private readonly Button _Button;
        /// <summary> Invoked when the button is confirmed </summary>
        private readonly Action _OnActivate;

        /// <inheritdoc />
        public Boolean CanFocus => _Button.Visible;

        /// <summary>
        /// Wraps a button so it can take focus
        /// </summary>
        /// <param name="button"> The button to wrap </param>
        /// <param name="onActivate"> Invoked when the button is confirmed </param>
        public FocusableButton(Button button, Action onActivate)
        {
            _Button = button;
            _OnActivate = onActivate;
            _Button.Sprite.Colour = Unfocused;
        }

        /// <inheritdoc />
        public void SetFocused(Boolean focused)
        {
            _Button.Sprite.Colour = focused ? Focused : Unfocused;
        }

        /// <inheritdoc />
        public void Activate()
        {
            _OnActivate?.Invoke();
        }
    }
}
