using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Interfaces.Control;

namespace Type.UI.Navigation
{
    /// <summary>
    /// A labelled setting on an options screen, shown as "MASTER VOLUME        100", where left
    /// and right change the value in place.
    /// </summary>
    public sealed class OptionRow : IAdjustable
    {
        /// <summary> Tint applied while the row does not have focus </summary>
        private static readonly Vector4 UnfocusedTint = new Vector4(0.55f, 0.55f, 0.55f, 1);
        /// <summary> Tint applied while the row has focus </summary>
        private static readonly Vector4 FocusedTint = new Vector4(1, 1, 1, 1);

        /// <summary> The setting name </summary>
        private readonly TextDisplay _Label;
        /// <summary> The current value </summary>
        private readonly TextDisplay _Value;
        /// <summary> Produces the text for the current value </summary>
        private readonly Func<String> _ReadValue;
        /// <summary> Applies a change of one step to the setting </summary>
        private readonly Action<Int32> _OnAdjust;

        /// <inheritdoc />
        public Boolean CanFocus => true;

        /// <summary>
        /// Creates a row for one setting
        /// </summary>
        /// <param name="label"> The setting name, in upper case </param>
        /// <param name="position"> Where to place the row </param>
        /// <param name="readValue"> Produces the text for the current value </param>
        /// <param name="onAdjust"> Applies a change of one step, given -1 or 1 </param>
        public OptionRow(String label, Vector2 position, Func<String> readValue, Action<Int32> onAdjust)
        {
            _ReadValue = readValue;
            _OnAdjust = onAdjust;

            _Label = CreateText(label, position);
            _Value = CreateText(String.Empty, position + new Vector2(700, 0));

            Refresh();
        }

        /// <summary>
        /// Creates one piece of row text
        /// </summary>
        private static TextDisplay CreateText(String text, Vector2 position)
        {
            return new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = text,
                Position = position,
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = UnfocusedTint,
            };
        }

        /// <summary>
        /// Re-reads the setting and updates the displayed value
        /// </summary>
        private void Refresh()
        {
            _Value.Text = _ReadValue();
        }

        #region Implementation of IAdjustable

        /// <inheritdoc />
        public void SetFocused(Boolean focused)
        {
            Vector4 tint = focused ? FocusedTint : UnfocusedTint;
            _Label.Colour = tint;
            _Value.Colour = tint;
        }

        /// <inheritdoc />
        public void Adjust(Int32 direction)
        {
            _OnAdjust(direction);
            Refresh();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Confirming a value row does nothing. The row is changed with left and right; there is
        /// no separate edit mode to enter.
        /// </remarks>
        public void Activate()
        {
        }

        #endregion

        /// <summary>
        /// Disposes the row's text
        /// </summary>
        public void Dispose()
        {
            _Label.Dispose();
            _Value.Dispose();
        }
    }
}
