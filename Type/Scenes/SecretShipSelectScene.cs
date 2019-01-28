using AmosShared.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.UI;

namespace Type.Scenes
{
    public class SecretShipSelectScene : Scene
    {
        private readonly TextDisplay _Title;

        private readonly Sprite _Background;

        private Boolean _Active;

        public ShipSelectButton OmegaButton { get; }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                OmegaButton.Active = _Active;
            }
        }

        public SecretShipSelectScene()
        {
            _Title = new TextDisplay(Game.MainCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "SECRET SHIP",
                Position = new Vector2(0, 450),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;
            _Background = new Sprite(Game.UiCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Background/stars-2.png"))
            {
                Position = new Vector2(0, 0),
                Visible = true,
            };
            _Background.Offset = _Background.Size / 2;

            OmegaButton = new ShipSelectButton(3, new Vector2(658, 50), "Content/Graphics/Player/player_omega.png", "OMEGA", 4, 150, 150);
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Title.Dispose();
            _Background.Dispose();
            OmegaButton.Dispose();
        }
    }
}
