using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Interfaces.Control;

namespace Type.UI.Navigation
{
    /// <summary>
    /// A line of text that runs an action when confirmed, for menus whose entries are commands
    /// rather than values.
    /// </summary>
    /// <remarks>
    /// The counterpart to <see cref="OptionRow"/>, which holds a value changed with left and
    /// right. This is not adjustable, so a navigator lets left and right move focus past it.
    /// </remarks>
    public sealed class MenuTextItem : IFocusable
    {
        /// <summary> Tint applied while the item does not have focus </summary>
        private static readonly Vector4 UnfocusedTint = new Vector4(0.55f, 0.55f, 0.55f, 1);
        /// <summary> Tint applied while the item has focus </summary>
        private static readonly Vector4 FocusedTint = new Vector4(1, 1, 1, 1);

        /// <summary> The text on screen </summary>
        private readonly TextDisplay _Display;
        /// <summary> Invoked when the item is confirmed </summary>
        private readonly Action _OnActivate;

        /// <inheritdoc />
        public Boolean CanFocus => _Display.Visible;

        /// <summary>
        /// Creates a menu entry
        /// </summary>
        /// <param name="label"> The entry text, in upper case </param>
        /// <param name="position"> Where to place the entry </param>
        /// <param name="onActivate"> Invoked when the entry is confirmed </param>
        /// <param name="scale"> Text scale </param>
        public MenuTextItem(String label, Vector2 position, Action onActivate, Single scale = 2.5f)
        {
            _OnActivate = onActivate;

            _Display = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = label,
                Position = position,
                Visible = true,
                Scale = new Vector2(scale, scale),
                Colour = UnfocusedTint,
            };
        }

        /// <summary>
        /// Hides or shows the entry. A hidden entry cannot take focus.
        /// </summary>
        /// <param name="visible"> Whether the entry is shown </param>
        public void SetVisible(Boolean visible)
        {
            _Display.Visible = visible;
        }

        #region Implementation of IFocusable

        /// <inheritdoc />
        public void SetFocused(Boolean focused)
        {
            _Display.Colour = focused ? FocusedTint : UnfocusedTint;
        }

        /// <inheritdoc />
        public void Activate()
        {
            _OnActivate?.Invoke();
        }

        #endregion

        /// <summary>
        /// Disposes the entry's text
        /// </summary>
        public void Dispose()
        {
            _Display.Dispose();
        }
    }
}
