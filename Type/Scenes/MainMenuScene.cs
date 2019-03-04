using AmosShared.Competitive;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;

namespace Type.Scenes
{
    /// <summary>
    /// The main menu scene
    /// </summary>
    public class MainMenuScene : Scene
    {
        /// <summary> Button that starts the game </summary>
        private readonly Button _StartButton;
        /// <summary> Button that will show the obtained achievements </summary>
        private readonly Button _AchievementsButton;
        /// <summary> Button that will show the leaderboards </summary>
        private readonly Button _LeaderboardButton;
        /// <summary> Sprite for the background </summary>
        private readonly Sprite _Background;
        /// <summary> The title text </summary>
        private readonly TextDisplay _TitleText;
        /// <summary> Text that promts game start </summary>
        private readonly TextDisplay _StartText;

        /// <summary> Whether the  player has pressed space and started the game </summary>
        public Boolean IsComplete { get; private set; }

        public MainMenuScene()
        {
            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Background/MainMenuBG-2.png"))
            {
                Position = new Vector2(-960, -540),
                Visible = true,
            };
            _TitleText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "C:TYPE",
                Position = new Vector2(0, 0),
                Visible = true,
                Scale = new Vector2(5, 5),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _TitleText.Offset = new Vector2(_TitleText.Size.X * _TitleText.Scale.X, _TitleText.Size.Y * _TitleText.Scale.Y) / 2;
            AddDrawable(_TitleText);

            Sprite achievementsButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/trophy.png"))
            {
                Position = new Vector2(-900, 375),
            };
            _AchievementsButton = new Button(Constants.ZOrders.UI, achievementsButton);
            _AchievementsButton.OnButtonPress += AchievementsButtonOnPress;

            Sprite leaderboardButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/leaderboard.png"))
            {
                Position = new Vector2(770, 375),
            };
            _LeaderboardButton = new Button(Constants.ZOrders.UI, leaderboardButton);
            _LeaderboardButton.OnButtonPress += LeaderboardButtonOnPress;

            Sprite startButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/completecontinue.png"))
            {
                Position = new Vector2(-200, -450),
            };
            _StartButton = new Button(Constants.ZOrders.UI, startButton);
            _StartButton.OnButtonPress += StartButtonPress;

            _StartText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = @"START",
                Position = new Vector2(0, startButton.Position.Y + startButton.Height / 2),
                Visible = true,
                Scale = new Vector2(2.5f, 2.5f),
            };
            _StartText.Offset = new Vector2(_StartText.Size.X * _StartText.Scale.X, _StartText.Size.Y * _StartText.Scale.Y) / 2;
            AddDrawable(_StartText);
        }

        public void Show()
        {
            _StartButton.TouchEnabled = true;
            _StartButton.Visible = true;
            _AchievementsButton.TouchEnabled = true;
            _AchievementsButton.Visible = true;
            _LeaderboardButton.TouchEnabled = true;
            _LeaderboardButton.Visible = true;
        }

        private void StartButtonPress(Button button)
        {
            StartGame();
        }

        public void StartGame()
        {
            Visible = false;
            _StartButton.TouchEnabled = false;
            _StartButton.Visible = false;
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

        public override void Update(TimeSpan timeSinceUpdate)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _TitleText.Dispose();
            _StartText.Dispose();
            _AchievementsButton.Dispose();
            _LeaderboardButton.Dispose();
            _Background.Dispose();
            _StartButton.Dispose();
        }
    }
}
