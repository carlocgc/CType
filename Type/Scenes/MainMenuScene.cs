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
        /// <summary> Button that starts the game </summary>
        private Button _StartButton;
        /// <summary> The title text </summary>
        private TextDisplay _TitleText;
        /// <summary> Text that promts game start </summary>
        private TextDisplay _StartText;
        /// <summary> background music </summary>
        private AudioPlayer _BackgroundMusic;

        /// <summary> Whether the  player has pressed space and started the game </summary>
        public Boolean IsComplete { get; set; }

        public MainMenuScene()
        {
            _TitleText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "C:TYPE",
                Position = new Vector2(0, 100),
                Visible = true,
                Scale = new Vector2(10, 10),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _TitleText.Offset = new Vector2(_TitleText.Size.X * _TitleText.Scale.X, _TitleText.Size.Y * _TitleText.Scale.Y) / 2;
            AddDrawable(_TitleText);
            _StartText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "TOUCH TO START",
                Position = new Vector2(0, 0),
                Visible = true,
                Scale = new Vector2(1, 1),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _StartText.Offset = new Vector2(_StartText.Size.X * _StartText.Scale.X, _StartText.Size.Y * _StartText.Scale.Y) / 2;
            AddDrawable(_StartText);

            Sprite startButton = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/MainMenuBG-2.png"))
            {
                Position = new Vector2(-960, -540),
            };
            _StartButton = new Button(Constants.ZOrders.UI, startButton);
            _StartButton.OnButtonPress += OnButtonPress;

            _BackgroundMusic = new AudioPlayer("Content/Audio/bgm-1.wav", true, AudioManager.Category.MUSIC, 1);
        }

        public void Show()
        {
            _StartButton.TouchEnabled = true;
            _StartButton.Visible = true;
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
            AudioManager.Instance.Dispose();
        }
    }
}
