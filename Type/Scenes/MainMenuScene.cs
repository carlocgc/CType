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
    /// The main menu scene
    /// </summary>
    public class MainMenuScene : Scene
    {
        /// <summary> The instance of the Main Menu </summary>
        private static MainMenuScene _Instance;
        /// <summary> The instance of the Main Menu </summary>
        public static MainMenuScene Instance => _Instance ?? (_Instance = new MainMenuScene());

        /// <summary> The background graphic </summary>
        private Sprite _Background;

        /// <summary> The title text </summary>
        private TextDisplay _TitleText;
        /// <summary> Text that promts game start </summary>
        private TextDisplay _StartText;
        /// <summary> Whether the  player has pressed space and started the game </summary>
        public Boolean IsGameStarted;
        /// <summary> Whether space has been depressed </summary>
        private Boolean IsKeyboardReset;

        private MainMenuScene()
        {
            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/background.png"))
            {
                Offset = new Vector2(960, 540),
                Visible = true,
                Colour = new Vector4(0.6f, 0.6f, 1, 1)
            };
            AddDrawable(_Background);
            _TitleText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, "Content/Graphics/KenPixel/", Constants.Font.Map)
            {
                Text = "D:TYPE",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1450, -Renderer.Instance.TargetDimensions.Y / 2 + 500),
                Visible = true,
                Scale = new Vector2(11, 11),
                Colour = new Vector4(1, 1, 1, 1)
            };
            AddDrawable(_TitleText);
            _StartText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, "Content/Graphics/KenPixel/", Constants.Font.Map)
            {
                Text = "PRESS SPACE TO START",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1400, -Renderer.Instance.TargetDimensions.Y / 2 + 350),
                Visible = true,
                Scale = new Vector2(3, 3),
                Colour = new Vector4(1, 1, 1, 1)
            };
            AddDrawable(_StartText);
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {
#if DESKTOP
            

            if (Keyboard.GetState().IsKeyUp(Key.Space)) IsKeyboardReset = true;

            if (IsKeyboardReset)
            {
                if (Keyboard.GetState().IsKeyDown(Key.Space) && !IsGameStarted)
                {
                    IsGameStarted = true;
                }
            }
#endif
        }
    }
}
