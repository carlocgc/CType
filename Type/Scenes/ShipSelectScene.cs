using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.UI;

namespace Type.Scenes
{
    public class ShipSelectScene : Scene
    {
        private Boolean _Active;
        public ShipSelectButton AlphaButton { get; }

        public ShipSelectButton BetaButton { get; }

        public ShipSelectButton GammaButton { get; }

        private readonly Sprite _Background;

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                _Background.Visible = _Active;
                AlphaButton.Active = _Active;
                BetaButton.Active = _Active;
                GammaButton.Active = _Active;
            }
        }

        public ShipSelectScene()
        {
            _Background = new Sprite(Game.UiCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Background/stars-2.png"))
            {
                Position = new Vector2(0, 0),
            };
            _Background.Offset = _Background.Size / 2;
            AlphaButton = new ShipSelectButton(0, new Vector2(65, 115), "Content/Graphics/Player/player-alpha.png", "ALPHA", 1, 100, 100);
            BetaButton = new ShipSelectButton(1, new Vector2(680, 115), "Content/Graphics/Player/player-beta.png", "BETA", 2, 80, 80);
            GammaButton = new ShipSelectButton(2, new Vector2(1290, 115), "Content/Graphics/Player/player-gamma.png", "GAMMA", 3, 60, 60);
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Background.Dispose();
            AlphaButton.Dispose();
            BetaButton.Dispose();
            GammaButton.Dispose();
        }
    }
}
