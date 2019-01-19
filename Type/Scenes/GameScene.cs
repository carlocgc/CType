using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using Type.Controllers;
using Type.Data;
using Type.Objects.Enemies;
using Type.Objects.Player;
using Type.Objects.World;
using Type.States;
using Type.UI;

namespace Type.Scenes
{
    public class GameScene : Scene
    {
        /// <summary> Scrolling background </summary>
        private readonly ScrollingBackground _BackgroundFar;
        /// <summary> Scrolling background </summary>
        private readonly ScrollingBackground _BackgroundNear;
        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _PlanetsNear;
        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _PlanetsFar;
        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _Clusters;
        /// <summary> Max level of the game </summary>
        private readonly Int32 _MaxLevel;
        /// <summary> The players ship </summary>
        private readonly Player _Player;
        /// <summary> Loads wave data from text files </summary>
        private readonly LevelLoader _LevelLoader;
        /// <summary> Fire button </summary>
        private readonly Button _FireButton;
        /// <summary> TODO Test button that adds probes </summary>
        private readonly Button _ProbeButton;
        /// <summary> TODO Test button that adds shield </summary>
        private readonly Button _ShieldButton;
        /// <summary> Floating analog stick </summary>
        private readonly AnalogStick _Stick;
        /// <summary> Text printer that displays the score </summary>
        private readonly TextDisplay _ScoreDisplay;
        /// <summary> THe word score displayed top left of screen </summary>
        private readonly TextDisplay _ScoreText;
        /// <summary> UI element that displays the amount oif lives remaining </summary>
        private readonly LifeMeter _LifeMeter;
        /// <summary> Object that shows the current level text </summary>
        private readonly LevelDisplay _LevelDisplay;
        /// <summary> UI element that displays the current FPS </summary>
        private readonly FpsCounter _Fps;
        /// <summary> The enemy factory </summary>
        private readonly EnemyFactory _EnemySpawner;

        /// <summary> The current level </summary>
        private Int32 CurrentLevel { get; set; }

        /// <summary> The players current score</summary>
        public Int32 CurrentScore { get; private set; }
        /// <summary> Whether the player has ran out of lives, ends the playing state </summary>
        public Boolean IsGameOver { get; private set; }
        /// <summary> Whether the game has been completed </summary>
        public Boolean IsGameComplete { get; private set; }

        public GameScene()
        {
            CurrentLevel = 1;
            _MaxLevel = 8;

            _BackgroundFar = new ScrollingBackground(100, "Content/Graphics/stars-1.png");
            _BackgroundNear = new ScrollingBackground(200, "Content/Graphics/stars-2.png");
            _Clusters = new ScrollingObject(100, 200, "Content/Graphics/cluster-", 7, 20, 40, Constants.ZOrders.CLUSTERS);
            _PlanetsFar = new ScrollingObject(200, 250, "Content/Graphics/planet-far-", 9, 10, 20, Constants.ZOrders.PLANETS_FAR);
            _PlanetsNear = new ScrollingObject(250, 350, "Content/Graphics/planet-near-", 9, 10, 30, Constants.ZOrders.PLANETS_NEAR);

            _LifeMeter = new LifeMeter();
            _Fps = new FpsCounter();
            _LevelDisplay = new LevelDisplay();

            _Player = new Player(OnPlayerDeath);
            _LevelLoader = new LevelLoader();
            _EnemySpawner = new EnemyFactory(this);

            _ScoreText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "SCORE: ",
                Position = new Vector2(-900, 460),
                Visible = true,
                Scale = new Vector2(3, 3),
                Colour = new Vector4(1, 0, 0, 1)
            };
            AddDrawable(_ScoreText);

            _ScoreDisplay = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = CurrentScore.ToString(),
                Position = new Vector2(-650, 460),
                Visible = true,
                Scale = new Vector2(3, 3),
            };
            AddDrawable(_ScoreDisplay);

            Sprite fireButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/fire.png"))
            {
                Position = new Vector2(625, -450),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _FireButton = new Button(Int32.MaxValue, fireButton) { OnButtonPress = FireButtonPress, OnButtonRelease = FireButtonRelease };

            Sprite probeButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/probe-button.png"))
            {
                Position = new Vector2(450, -450),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _ProbeButton = new Button(Int32.MaxValue, probeButton) { OnButtonPress = ProbeButtonOnPress };

            Sprite shieldButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/shield_button.png"))
            {
                Position = new Vector2(320, -450),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _ShieldButton = new Button(Int32.MaxValue, shieldButton) { OnButtonPress = ShieldButtonPress };

            _Stick = new AnalogStick(new Vector2(-620, -220), 110);
            _Stick.RegisterListener(_Player);

            new AudioPlayer("Content/Audio/bgm-1.wav", true, AudioManager.Category.MUSIC, 1);
        }

        /// <summary>
        /// Starts a new game, starts the background moving and activates the buttons
        /// </summary>
        public void StartGame()
        {
            CurrentScore = 0;

            _BackgroundNear.Start();
            _BackgroundFar.Start();
            _PlanetsFar.Start();
            _PlanetsNear.Start();
            _Clusters.Start();

            _LevelDisplay.ShowLevel(CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                _EnemySpawner.SetLevelData(_LevelLoader.GetWaveData(CurrentLevel));
                CollisionController.Instance.IsActive = true;
            });

            SetButtonsEnabled(true);
            SetButtonsVisible(true);
        }

        /// <summary>
        /// Adds the value to the players current score
        /// </summary>
        /// <param name="amount"></param>
        public void UpdateScore(Int32 amount)
        {
            CurrentScore += amount;
            _ScoreDisplay.Text = CurrentScore.ToString();
            if (CurrentScore % 1000 == 0) _LifeMeter.AddLife();
        }

        /// <summary>
        /// Sets the next level data and displays the current level, ends the game if complete
        /// </summary>
        public void LevelComplete()
        {
            if (CurrentLevel >= _MaxLevel)
            {
                GameCompleted();
            }
            else
            {
                CurrentLevel++;
                _LevelDisplay.ShowLevel(CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    _EnemySpawner.SetLevelData(_LevelLoader.GetWaveData(CurrentLevel));
                });
            }
        }

        /// <summary>
        /// Ends the playing state and set the next state to be <see cref="GameCompleteState"/>
        /// </summary>
        private void GameCompleted()
        {
            _EnemySpawner.Reset();
            IsGameComplete = true;
            SetButtonsEnabled(false);
            SetButtonsVisible(false);
        }

        /// <summary>
        /// Called when the player dies checks if game is over
        /// </summary>
        private void OnPlayerDeath()
        {
            _EnemySpawner.Reset();
            _LifeMeter.LoseLife();

            if (_LifeMeter.PlayerLives <= 0 && !IsGameOver)
            {
                IsGameOver = true;
                SetButtonsEnabled(false);
                SetButtonsVisible(false);
            }
            else
            {

                _EnemySpawner.StartWave();
            }
        }

        #region Input

        private void SetButtonsEnabled(Boolean state)
        {
            _FireButton.TouchEnabled = state;
            _ProbeButton.TouchEnabled = state;
            _ShieldButton.TouchEnabled = state;
            _Stick.TouchEnabled = state;
        }

        private void SetButtonsVisible(Boolean state)
        {
            _FireButton.Visible = state;
            _ProbeButton.Visible = state;
            _ShieldButton.Visible = state;
            _Stick.Visible = state;
            _Stick.ListeningForMove = state;
        }

        private void FireButtonRelease(Button obj)
        {
            _Player.Shoot = false;
        }

        private void FireButtonPress(Button obj)
        {
            _Player.Shoot = true;
        }

        private void ProbeButtonOnPress(Button obj)
        {
            _Player.CreateProbes(1, 0);
        }

        private void ShieldButtonPress(Button obj)
        {
            _Player.AddShield();
        }

        #endregion

        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        public override void Dispose()
        {
            base.Dispose();

            _Player.Dispose();
            _BackgroundNear.Dispose();
            _BackgroundFar.Dispose();
            _PlanetsNear.Dispose();
            _PlanetsFar.Dispose();
            _Clusters.Dispose();
            _EnemySpawner.Dispose();

            _Stick.Dispose();
            _FireButton.Dispose();
            _ProbeButton.Dispose();
            _ShieldButton.Dispose();

            _Fps.Dispose();
            _LifeMeter.Dispose();
            _ScoreText.Dispose();
            _ScoreDisplay.Dispose();

            AudioManager.Instance.Dispose();
        }
    }
}
