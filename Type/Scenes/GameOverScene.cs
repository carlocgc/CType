﻿using AmosShared.Audio;
using AmosShared.Competitive;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using Type.Ads;
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
        /// <summary> Button that will show the obtained achievements </summary>
        private readonly Button _AchievementsButton;
        /// <summary> Button that will show the leaderboards </summary>
        private readonly Button _LeaderboardButton;
        /// <summary> Displays the current game data via text displays </summary>
        private readonly StatsDisplay _StatsDisplay;
        /// <summary> Text on the confirm button </summary>
        private readonly TextDisplay _ConfirmText;
        /// <summary> Background music for the scene </summary>
        private readonly AudioPlayer _Music;

        /// <summary> Whether the confirm button has been pressed </summary>
        public Boolean IsComplete { get; private set; }

        public GameOverScene()
        {
            _GameOverText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "GAME OVER",
                Position = new Vector2(0, 300),
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

            Sprite achievementsButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/trophy-red.png"))
            {
                Position = new Vector2(-900, -500)
            };
            _AchievementsButton = new Button(Constants.ZOrders.UI, achievementsButton);
            _AchievementsButton.OnButtonPress += AchievementsButtonOnPress;

            Sprite leaderboardButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/leaderboard-red.png"))
            {
                Position = new Vector2(-900, -380)
            };
            _LeaderboardButton = new Button(Constants.ZOrders.UI, leaderboardButton);
            _LeaderboardButton.OnButtonPress += LeaderboardButtonOnPress;

            Sprite confirmButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/gameovercontinue.png"))
            {
                Position = new Vector2(-200, -450),
            };
            _ConfirmButton = new Button(Constants.ZOrders.UI, confirmButton);
            _ConfirmButton.OnButtonPress += ConfirmPress;

            _ConfirmText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = @"CONFIRM",
                Position = new Vector2(0, confirmButton.Position.Y + confirmButton.Height / 2),
                Scale = new Vector2(2.5f, 2.5f),
            };
            _ConfirmText.Offset = new Vector2(_ConfirmText.Size.X * _ConfirmText.Scale.X, _ConfirmText.Size.Y * _ConfirmText.Scale.Y) / 2;
            AddDrawable(_ConfirmText);

            _Music = new AudioPlayer("Content/Audio/gameOverBgm.wav", true, AudioManager.Category.MUSIC, 1);

            _StatsDisplay = new StatsDisplay();
        }

        public void Start()
        {
            _ScoreText.Text = $"SCORE {GameStats.Instance.Score}";
            _ScoreText.Offset = new Vector2(_ScoreText.Size.X * _ScoreText.Scale.X, _ScoreText.Size.Y * _ScoreText.Scale.Y) / 2;

            _ConfirmButton.TouchEnabled = true;
            _ConfirmButton.Visible = true;
            _AchievementsButton.TouchEnabled = true;
            _AchievementsButton.Visible = true;
            _LeaderboardButton.TouchEnabled = true;
            _LeaderboardButton.Visible = true;
            _ConfirmText.Visible = true;
            _Background.Visible = true;
        }

        private void ConfirmPress(Button button)
        {
            if (AdService.Instance.IsLoaded)
            {
                AdService.Instance.OnAddClosed = () => IsComplete = true;
                AdService.Instance.ShowInterstitial();
            }
            else
            {
                IsComplete = true;
            }
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

        /// <summary> Disposes of the scene </summary>
        public override void Dispose()
        {
            base.Dispose();
            _Music.Stop();
            _GameOverText.Dispose();
            _ScoreText.Dispose();
            _ConfirmButton.Dispose();
            _ConfirmText.Dispose();
            _AchievementsButton.Dispose();
            _LeaderboardButton.Dispose();
            _Background.Dispose();
            _StatsDisplay.Dispose();
        }
    }
}
