using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;

namespace Type.Scenes
{
    public class EngineSplashScene : Scene
    {
        /// <summary> The background </summary>
        private readonly Sprite _Background;
        /// <summary> Amos engine logo </summary>
        private readonly Sprite _Logo;
        /// <summary> Amos engien head graphic </summary>
        private readonly Sprite _Head;

        public EngineSplashScene()
        {
            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Engine/engine_background.png"))
            {
                Position = new Vector2(0, 0),
                Visible = true,
            };
            _Background.Offset = _Background.Size / 2;

            _Logo = new Sprite(Game.MainCanvas, Constants.ZOrders.ENGINE_LOGO, Texture.GetTexture("Content/Graphics/Engine/amos_logo_box.png"))
            {
                Position = new Vector2(0, -50),
                Visible = true,
            };
            _Logo.Offset = _Logo.Size / 2;
            _Head = new Sprite(Game.MainCanvas, Constants.ZOrders.ENGINE_HEAD, Texture.GetTexture("Content/Graphics/Engine/amos_logo_head.png"))
            {
                Position = new Vector2(0, -50),
                Visible = true,
            };
            _Head.Offset = new Vector2(_Head.Size.X / 2, _Head.Offset.Y);
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
            _Head.Dispose();
            _Logo.Dispose();
        }
    }
}
