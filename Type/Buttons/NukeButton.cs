using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using Type.Base;
using Type.Data;
using Type.Interfaces.Control;

namespace Type.Buttons
{
    /// <summary>
    /// Button that can detonate a nuke, displays the current nuke count
    /// </summary>
    public class NukeButton : GameObject, IVirtualButton
    {
        /// <summary> Text showing how many nukes the player has </summary>
        private readonly TextDisplay _NukeCountDisplay;
        /// <summary> Button that will inform the listeners </summary>
        private readonly Button _Button;
        /// <summary> The number of nukes to display on the button </summary>
        private Int32 _NukeCount;
        /// <summary> WHether the button is visible </summary>
        private Boolean _Visible;

        private Boolean _Active;

        #region Implementation of IVirtualButton

        public ButtonData.Type ID => ButtonData.Type.NUKE;

        /// <summary> The current press state of the button </summary>
        public ButtonData.State State { get; set; }

        #endregion

        public Boolean Visible
        {
            get => _Visible;
            set
            {
                _Visible = value;
                if (_Visible == false)
                {
                    _Button.Visible = false;
                    _NukeCountDisplay.Visible = false;
                    _Button.TouchEnabled = false;
                    State = ButtonData.State.RELEASED;
                }
                else
                {
                    _Button.Visible = true;
                    NukeCount = _NukeCount;
                    State = ButtonData.State.RELEASED;
                }
            }
        }

        /// <summary> The number of nukes to display on the button </summary>
        public Int32 NukeCount
        {
            get => _NukeCount;
            set
            {
                _NukeCount = value;
                if (_NukeCount > 0)
                {
                    _Button.Sprite.Colour = new Vector4(1, 1, 1, 0.4f);
                    _Button.TouchEnabled = true;
                    _NukeCountDisplay.Visible = true;
                    State = ButtonData.State.RELEASED;
                }
                else
                {
                    _Button.Sprite.Colour = new Vector4(0.4f, 0.4f, 0.4f, 0.4f);
                    _Button.TouchEnabled = false;
                    _NukeCountDisplay.Visible = false;
                    State = ButtonData.State.RELEASED;
                }
                _NukeCountDisplay.Text = $"{_NukeCount}";
            }
        }


        public NukeButton()
        {
            Sprite buttonSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/nuke_button.png"))
            {
                Position = new Vector2(425, -450),
                Visible = true,
            };
            _Button = new Button(Int32.MaxValue, buttonSprite) { OnButtonPress = OnPressed, OnButtonRelease = OnReleased };

            _NukeCountDisplay = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{NukeCount}",
                Position = _Button.Position + _Button.Sprite.Size / 2,
                Scale = new Vector2(2, 2),
            };
            _NukeCountDisplay.Offset = new Vector2(_NukeCountDisplay.Size.X * _NukeCountDisplay.Scale.X, _NukeCountDisplay.Size.Y * _NukeCountDisplay.Scale.Y) / 2;
            NukeCount = 0;
        }

        /// <summary>
        /// Sets the button state to pressed
        /// </summary>
        /// <param name="obj"></param>
        private void OnPressed(Button obj)
        {
            State = ButtonData.State.PRESSED;
        }

        /// <summary>
        /// Sets the button state to released
        /// </summary>
        /// <param name="obj"></param>
        private void OnReleased(Button obj)
        {
            State = ButtonData.State.RELEASED;
        }

        #region Overrides of GameObject

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _Button.Dispose();
            _NukeCountDisplay.Dispose();
        }

        #endregion
    }
}
