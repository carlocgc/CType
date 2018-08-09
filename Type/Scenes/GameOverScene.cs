using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
#if DESKTOP
using OpenTK.Input;
#endif

namespace Type.Scenes
{
    /// <summary>
    /// The Game over scene 
    /// </summary>
    public class GameOverScene : Scene
    {
        /// <summary> The instance of the Game Over scene </summary>
        private static GameOverScene _Instance;
        /// <summary> The instance of the Game Over scene </summary>
        public static GameOverScene Instance => _Instance ?? (_Instance = new GameOverScene());

        /// <summary> The bacground sprite </summary>
        private Sprite _Background;
        /// <summary> Text that prompts user to restart </summary>
        private TextDisplay _RestartText;

        /// <summary> Whether the game over state has been confirmed and can exit </summary>
        public Boolean IsConfirmed;

        private TextDisplay _GameOverText;

        private GameOverScene()
        {
            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/background.png"))
            {
                Offset = new Vector2(960, 540),
                Visible = true,
                Colour = new Vector4(0.5f, 0.5f, 0.5f, 1)
            };
            AddDrawable(_Background);
            _GameOverText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, "Content/Graphics/KenPixel/", Constants.Font.Map)
            {
                Text = "GAME OVER",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1700, -Renderer.Instance.TargetDimensions.Y / 2 + 500),
                Visible = true,
                Scale = new Vector2(11, 11),
                Colour = new Vector4(1, 0, 0, 1)
            };
            AddDrawable(_GameOverText);
            _RestartText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, "Content/Graphics/KenPixel/", Constants.Font.Map)
            {
                Text = "PRESS ENTER TO GO TO MENU",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1550, -Renderer.Instance.TargetDimensions.Y / 2 + 350),
                Visible = true,
                Scale = new Vector2(3, 3),
                Colour = new Vector4(1, 1, 1, 1)
            };
            AddDrawable(_RestartText);
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {
#if DESKTOP
            if (Keyboard.GetState().IsKeyDown(Key.Enter) && !IsConfirmed)
            {
                IsConfirmed = true;
            }
#endif
        }
    }
}
