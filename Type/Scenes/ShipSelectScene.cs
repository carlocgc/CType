using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using AmosShared.Audio;
using Type.UI;

namespace Type.Scenes
{
    public class ShipSelectScene : Scene
    {
        private readonly TextDisplay _Title;

        private readonly Sprite _Background;

        private Boolean _Active;

        public ShipSelectButton AlphaButton { get; }

        public ShipSelectButton BetaButton { get; }

        public ShipSelectButton GammaButton { get; }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                AlphaButton.Active = _Active;
                BetaButton.Active = _Active;
                GammaButton.Active = _Active;
            }
        }

        public ShipSelectScene()
        {
            _Title = new TextDisplay(Game.MainCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "SHIP SELECT",
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

            AlphaButton = new ShipSelectButton(0, new Vector2(45, 50), "Content/Graphics/Player/player-alpha.png", "ALPHA", 1, 100, 100);
            BetaButton = new ShipSelectButton(1, new Vector2(658, 50), "Content/Graphics/Player/player-beta.png", "BETA", 2, 80, 80);
            GammaButton = new ShipSelectButton(2, new Vector2(1270, 50), "Content/Graphics/Player/player-gamma.png", "GAMMA", 3, 60, 60);
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
            AlphaButton.Dispose();
            BetaButton.Dispose();
            GammaButton.Dispose();
            _Background.Dispose();
            AudioManager.Instance.Dispose();
        }
    }
}
