using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using AmosShared.Competitive;
using Type.Ads;
using Type.Data;
using Type.UI;

namespace Type.Scenes
{
    public class GameCompleteScene : Scene
    {
        /// <summary> Button used to return to main menu </summary>
        private readonly Button _ConfirmButton;
        /// <summary> Button that will show the obtained achievements </summary>
        private readonly Button _AchievementsButton;
        /// <summary> Button that will show the leaderboards </summary>
        private readonly Button _LeaderboardButton;
        /// <summary> Sprite for the background </summary>
        private readonly Sprite _Background;
        /// <summary> Text displaying the word congratulations </summary>
        private readonly TextDisplay _CongratsText;
        /// <summary> Text that prompts user to restart </summary>
        private readonly TextDisplay _ScoreText;
        /// <summary> Displays the current game data via text displays </summary>
        private readonly StatsDisplay _StatsDisplay;
        /// <summary> Text on the confirm button </summary>
        private readonly TextDisplay _ConfirmText;
        /// <summary> Text to display when a new highscore is achieved </summary>
        private TextDisplay _HighScoreText;
        /// <summary> Background music for the scene </summary>
        private readonly AudioPlayer _Music;

        /// <summary> Whether the complete can end </summary>
        public Boolean IsComplete { get; private set; }

        public GameCompleteScene()
        {
            _CongratsText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "CONGRATULATIONS",
                Position = new Vector2(0, 300),
                Visible = true,
                Scale = new Vector2(5, 5),
                Colour = new Vector4(0, 0, 1, 1)
            };
            _CongratsText.Offset = new Vector2(_CongratsText.Size.X * _CongratsText.Scale.X, _CongratsText.Size.Y * _CongratsText.Scale.Y) / 2;
            AddDrawable(_CongratsText);
            _HighScoreText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "NEW HIGHSCORE",
                Position = new Vector2(0, 100),
                Scale = new Vector2(4, 4),
                Colour = new Vector4(255, 255, 0, 1)
            };
            _HighScoreText.Offset = new Vector2(_HighScoreText.Size.X * _HighScoreText.Scale.X, _HighScoreText.Size.Y * _HighScoreText.Scale.Y) / 2;
            AddDrawable(_HighScoreText);
            _ScoreText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Position = new Vector2(0, 0),
                Visible = true,
                Scale = new Vector2(4, 4),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _ScoreText.Offset = new Vector2(_ScoreText.Size.X * _ScoreText.Scale.X, _ScoreText.Size.Y * _ScoreText.Scale.Y) / 2;
            AddDrawable(_ScoreText);
            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Background/GameOverBG.png"))
            {
                Position = new Vector2(-960, -540),
                Colour = new Vector4(0.7f, 0.7f, 0.7f, 1)
            };
            AddDrawable(_Background);

            Sprite achievementsButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/trophy.png"))
            {
                Position = new Vector2(-900, 350),
            };
            _AchievementsButton = new Button(Constants.ZOrders.UI, achievementsButton);
            _AchievementsButton.OnButtonPress += AchievementsButtonOnPress;

            Sprite leaderboardButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/leaderboard.png"))
            {
                Position = new Vector2(770, 350),
            };
            _LeaderboardButton = new Button(Constants.ZOrders.UI, leaderboardButton);
            _LeaderboardButton.OnButtonPress += LeaderboardButtonOnPress;

            Sprite confirmButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/completecontinue.png"))
            {
                Position = new Vector2(-200, -450),
            };
            _ConfirmButton = new Button(Constants.ZOrders.UI, confirmButton);
            _ConfirmButton.OnButtonPress += OnButtonPress;

            _ConfirmText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = @"CONFIRM",
                Position = new Vector2(0, confirmButton.Position.Y + confirmButton.Height / 2),
                Scale = new Vector2(2.5f, 2.5f),
            };
            _ConfirmText.Offset = new Vector2(_ConfirmText.Size.X * _ConfirmText.Scale.X, _ConfirmText.Size.Y * _ConfirmText.Scale.Y) / 2;
            AddDrawable(_ConfirmText);

            _Music = new AudioPlayer("Content/Audio/gameCompleteBgm.wav", true, AudioManager.Category.MUSIC, 1);

            _StatsDisplay = new StatsDisplay();
        }

        private void OnButtonPress(Button obj)
        {
            IsComplete = true;
        }

        private void AchievementsButtonOnPress(Button button)
        {
            CompetitiveManager.Instance.ViewAchievements();
        }

        private void LeaderboardButtonOnPress(Button button)
        {
            CompetitiveManager.Instance.ViewLeaderboards();
        }

        /// <summary> Updates the scene </summary>
        /// <param name="timeSinceUpdate"></param>
        public override void Update(TimeSpan timeSinceUpdate)
        {
        }

        public void Start()
        {
            _ScoreText.Text = $"SCORE {GameStats.Instance.Score}";
            _ScoreText.Offset = new Vector2(_ScoreText.Size.X * _ScoreText.Scale.X, _ScoreText.Size.Y * _ScoreText.Scale.Y) / 2;

            _Background.Visible = true;
            _ConfirmButton.TouchEnabled = true;
            _ConfirmButton.Visible = true;
            _AchievementsButton.TouchEnabled = true;
            _AchievementsButton.Visible = true;
            _LeaderboardButton.TouchEnabled = true;
            _LeaderboardButton.Visible = true;
            _ConfirmText.Visible = true;

            _HighScoreText.Visible = GameStats.Instance.IsNewHighScore;
        }

        /// <summary> Disposes of the scene </summary>
        public override void Dispose()
        {
            base.Dispose();
            _Music.Stop();
            _Background.Dispose();
            _ConfirmText.Dispose();
            _CongratsText.Dispose();
            _ScoreText.Dispose();
            _AchievementsButton.Dispose();
            _LeaderboardButton.Dispose();
            _ConfirmButton.Dispose();
            _StatsDisplay.Dispose();
            _HighScoreText.Dispose();
        }
    }
}
