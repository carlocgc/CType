using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;
using System;
using Type.Data;
using Type.Services;

namespace Type.UI
{
    /// <summary>
    /// A line of text telling the player which input performs an action, for example
    /// "A  SELECT" on a gamepad or "SPACE  SELECT" on a keyboard.
    /// </summary>
    /// <remarks>
    /// The label follows whichever device is driving input and updates when a pad is plugged
    /// in or unplugged mid-menu, so the prompt never names a device the player is not holding.
    /// Text rather than glyph sprites: the bitmap font is already loaded everywhere, whereas
    /// glyphs would mean new art registered in both platform projects.
    /// </remarks>
    public sealed class InputPrompt : IUpdatable
    {
        /// <summary> The action being described </summary>
        private readonly ButtonData.Type _Action;
        /// <summary> What the action does, shown after the input name </summary>
        private readonly String _Caption;
        /// <summary> The text on screen </summary>
        private readonly TextDisplay _Display;

        /// <summary> Whether the label currently names a gamepad input </summary>
        private Boolean _ShowingGamepad;
        /// <summary> Whether a label has been produced yet </summary>
        private Boolean _Initialised;

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        /// <summary> Whether the prompt is shown </summary>
        public Boolean Visible
        {
            get => _Display.Visible;
            set => _Display.Visible = value;
        }

        /// <summary>
        /// Creates a prompt for an action
        /// </summary>
        /// <param name="action"> The action to describe </param>
        /// <param name="caption"> What the action does, in upper case </param>
        /// <param name="position"> Where to place the prompt </param>
        /// <param name="scale"> Text scale </param>
        public InputPrompt(ButtonData.Type action, String caption, Vector2 position, Single scale = 1.5f)
        {
            _Action = action;
            _Caption = caption;

            _Display = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = String.Empty,
                Position = position,
                Visible = true,
                Scale = new Vector2(scale, scale),
                Colour = new Vector4(0.8f, 0.8f, 0.8f, 1),
            };

            Refresh();
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Rebuilds the label for the device currently driving input
        /// </summary>
        private void Refresh()
        {
            Boolean gamepad = InputService.Instance.GamepadActive;
            String label = InputService.Instance.Bindings?.GetPromptLabel(_Action, gamepad) ?? String.Empty;

            // An unbound action would otherwise show a caption with nothing to press.
            _Display.Text = label.Length == 0 ? String.Empty : $"{label}  {_Caption}";

            _ShowingGamepad = gamepad;
            _Initialised = true;
        }

        #region Implementation of IUpdatable

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            if (_Initialised && InputService.Instance.GamepadActive == _ShowingGamepad) return;
            Refresh();
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            UpdateManager.Instance.RemoveUpdatable(this);
            _Display.Dispose();
        }
    }
}
