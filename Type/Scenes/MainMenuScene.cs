using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
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
        /// <summary> Button that starts the game </summary>
        private Button _StartButton;
        /// <summary> The title text </summary>
        private TextDisplay _TitleText;
        /// <summary> Text that promts game start </summary>
        private TextDisplay _StartText;

        private AudioPlayer _BackgroundMusic;

        /// <summary> Whether the  player has pressed space and started the game </summary>
        public Boolean IsComplete { get; set; }

        private MainMenuScene()
        {
            _TitleText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "C:TYPE",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1470, -Renderer.Instance.TargetDimensions.Y / 2 + 500),
                Visible = true,
                Scale = new Vector2(11, 11),
                Colour = new Vector4(0, 1, 0, 1)
            };
            AddDrawable(_TitleText);
            _StartText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "TOUCH TO START",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1270, -Renderer.Instance.TargetDimensions.Y / 2 + 350),
                Visible = true,
                Scale = new Vector2(3, 3),
                Colour = new Vector4(1, 0, 0, 1)
            };
            AddDrawable(_StartText);

            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/MainMenuBG.png"))
            {
                Position = new Vector2(-960, -540),
            };
            _StartButton = new Button(Constants.ZOrders.UI, _Background);
            _StartButton.OnButtonPress += OnButtonPress;

            _BackgroundMusic = new AudioPlayer("Content/Audio/bgm-1.wav", true, AudioManager.Category.MUSIC, 1);
        }

        public void Show()
        {
            _StartButton.TouchEnabled = true;
            _StartButton.Visible = true;
            Visible = true;
        }

        private void OnButtonPress(Button button)
        {
            Visible = false;
            _StartButton.TouchEnabled = false;
            _StartButton.Visible = false;
            IsComplete = true;
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _StartButton.Dispose();
        }
    }
}
