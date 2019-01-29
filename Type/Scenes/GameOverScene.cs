using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using Android.Gms.Ads;
using OpenTK;
using System;
using Type.Ads;
using Type.Android;
using Type.Data;
using Type.UI;

namespace Type.Scenes
{
    /// <summary>
    /// The Game over scene
    /// </summary>
    public class GameOverScene : Scene
    {
        /// <summary> Game over title text </summary>
        private readonly TextDisplay _GameOverText;
        /// <summary> Text that prompts user to restart </summary>
        private readonly TextDisplay _ScoreText;
        /// <summary> Sprite for the background </summary>
        private readonly Sprite _Background;
        /// <summary> Confirm button that ends the game over state </summary>
        private readonly Button _ConfirmButton;
        /// <summary> Displays the current game data via text displays </summary>
        private readonly StatsDisplay _StatsDisplay;

        private readonly AudioPlayer _Music;

        /// <summary> Whether the confirm button has been pressed </summary>
        public Boolean IsComplete { get; private set; }

        public GameOverScene()
        {
            _GameOverText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "GAME OVER",
                Position = new Vector2(0, 200),
                Visible = true,
                Scale = new Vector2(7, 7),
                Colour = new Vector4(1, 0, 0, 1)
            };
            _GameOverText.Offset = new Vector2(_GameOverText.Size.X * _GameOverText.Scale.X, _GameOverText.Size.Y * _GameOverText.Scale.Y) / 2;
            AddDrawable(_GameOverText);
            _ScoreText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Position = new Vector2(0, 0),
                Visible = true,
                Scale = new Vector2(4, 4),
                TextAlignment = TextDisplay.Alignment.CENTER,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _ScoreText.Offset = new Vector2(_ScoreText.Size.X * _ScoreText.Scale.X, _ScoreText.Size.Y * _ScoreText.Scale.Y) / 2;
            AddDrawable(_ScoreText);
            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Background/GameCompleteBG.png"))
            {
                Position = new Vector2(-960, -540),
                Colour = new Vector4(0.7f, 0.7f, 0.7f, 1)
            };
            AddDrawable(_Background);

            Sprite confirmButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/gameovercontinue.png"))
            {
                Position = new Vector2(-200, -450),
            };
            _ConfirmButton = new Button(Constants.ZOrders.UI, confirmButton);
            _ConfirmButton.OnButtonPress += OnButtonPress;

            _Music = new AudioPlayer("Content/Audio/gameOverBgm.wav", true, AudioManager.Category.MUSIC, 1);

            _StatsDisplay = new StatsDisplay();
        }

        private void OnButtonPress(Button button)
        {
            _Music.Stop();
            _ConfirmButton.TouchEnabled = false;

            AdService.Instance.ShowInterstitial(onClosed: () => IsComplete = true);
        }

        public void Start()
        {
            _ScoreText.Text = $"SCORE {GameStats.Instance.Score}";
            _ScoreText.Offset = new Vector2(_ScoreText.Size.X * _ScoreText.Scale.X, _ScoreText.Size.Y * _ScoreText.Scale.Y) / 2;

            _ConfirmButton.TouchEnabled = true;
            _ConfirmButton.Visible = true;

            _Background.Visible = true;

            new AudioPlayer("Content/Audio/gameOver.wav", false, AudioManager.Category.EFFECT, 1);
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        /// <summary> Disposes of the scene </summary>
        public override void Dispose()
        {
            base.Dispose();
            _ConfirmButton.Dispose();
            _Background.Dispose();
            _StatsDisplay.Dispose();
            //AudioManager.Instance.Dispose();
        }
    }
}
