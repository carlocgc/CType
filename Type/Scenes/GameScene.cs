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

        /// <summary> Up directional button </summary>
        private readonly Button _UpButton;
        /// <summary> Down directional button </summary>
        private readonly Button _DownButton;
        /// <summary> Right directional button </summary>
        private readonly Button _RightButton;
        /// <summary> Left directional button </summary>
        private readonly Button _LeftButton;
        /// <summary> Fire button </summary>
        private readonly Button _FireButton;

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

        /// <summary> The players current score</summary>
        public Int32 CurrentScore { get; private set; }

        /// <summary> Whether the player has ran out of lives, ends the playing state </summary>
        public Boolean IsGameOver;
        /// <summary> The enemy factory </summary>
        private EnemyFactory _EnemySpawner;

        /// <summary> The current level </summary>
        public Int32 CurrentLevel { get; private set; }

        public GameScene()
        {
            _MaxLevel = 8;

            _BackgroundFar = new ScrollingBackground(100, "Content/Graphics/stars-1.png");
            _BackgroundNear = new ScrollingBackground(200, "Content/Graphics/stars-2.png");
            _Clusters = new ScrollingObject(100, 200, "Content/Graphics/cluster-", 7, 20, 40, Constants.ZOrders.CLUSTERS);
            _PlanetsFar = new ScrollingObject(200, 250, "Content/Graphics/planet-far-", 9, 10, 20, Constants.ZOrders.PLANETS_FAR);
            _PlanetsNear = new ScrollingObject(250, 350, "Content/Graphics/planet-near-", 9, 10, 30, Constants.ZOrders.PLANETS_NEAR);

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

            _LifeMeter = new LifeMeter();
            _Fps = new FpsCounter();
            _LevelDisplay = new LevelDisplay();
            CurrentLevel = 1;

            _Player = new Player(OnPlayerDeath);
            _LevelLoader = new LevelLoader();
            _EnemySpawner = new EnemyFactory(this);

            Sprite upButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/up.png"))
            {
                Position = new Vector2(-712, -220),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _UpButton = new Button(Int32.MaxValue, upButton) { OnButtonPress = UpButtonPress, OnButtonRelease = UpButtonRelease };

            Sprite downButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/down.png"))
            {
                Position = new Vector2(-712, -480),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _DownButton = new Button(Int32.MaxValue, downButton) { OnButtonPress = DownButtonPress, OnButtonRelease = DownButtonRelease };

            Sprite leftButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/left.png"))
            {
                Position = new Vector2(-845, -350),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _LeftButton = new Button(Int32.MaxValue, leftButton) { OnButtonPress = LeftButtonPress, OnButtonRelease = LeftButtonRelease };

            Sprite rightButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/right.png"))
            {
                Position = new Vector2(-585, -350),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _RightButton = new Button(Int32.MaxValue, rightButton) { OnButtonPress = RightButtonPress, OnButtonRelease = RightButtonRelease };

            Sprite fireButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/fire.png"))
            {
                Position = new Vector2(625, -450),
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            _FireButton = new Button(Int32.MaxValue, fireButton) { OnButtonPress = FireButtonPress, OnButtonRelease = FireButtonRelease };
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartGame()
        {
            CurrentScore = 0;

            _BackgroundNear.Start();
            _BackgroundFar.Start();
            _PlanetsFar.Start();
            _PlanetsNear.Start();
            _Clusters.Start();

            _LevelDisplay.ShowLevel(CurrentLevel, TimeSpan.FromSeconds(0.5), () =>
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

        public void LevelComplete()
        {
            CurrentLevel++;
            if (CurrentLevel >= _MaxLevel)
            {
                // TODO Game Complete
                IsGameOver = true;
                SetButtonsEnabled(false);
                SetButtonsVisible(false);
            }
            else
            {
                _LevelDisplay.ShowLevel(CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    _EnemySpawner.SetLevelData(_LevelLoader.GetWaveData(CurrentLevel));
                });
            }
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
                new AudioPlayer("Content/Audio/gameOver.wav", false, AudioManager.Category.EFFECT, 100);
            }
            else
            {
                _EnemySpawner.StartWave();
            }
        }

        #region Input

        private void SetButtonsEnabled(Boolean state)
        {
            _UpButton.TouchEnabled = state;
            _DownButton.TouchEnabled = state;
            _LeftButton.TouchEnabled = state;
            _RightButton.TouchEnabled = state;
            _FireButton.TouchEnabled = state;
        }

        private void SetButtonsVisible(Boolean state)
        {
            _UpButton.Visible = state;
            _DownButton.Visible = state;
            _LeftButton.Visible = state;
            _RightButton.Visible = state;
            _FireButton.Visible = state;
        }

        private void RightButtonPress(Button obj)
        {
            _Player.MoveRight = true;
        }

        private void RightButtonRelease(Button obj)
        {
            _Player.MoveRight = false;
        }

        private void LeftButtonPress(Button obj)
        {
            _Player.MoveLeft = true;
        }

        private void LeftButtonRelease(Button obj)
        {
            _Player.MoveLeft = false;
        }

        private void DownButtonPress(Button obj)
        {
            _Player.MoveDown = true;
        }

        private void DownButtonRelease(Button obj)
        {
            _Player.MoveDown = false;
        }

        private void UpButtonRelease(Button button)
        {
            _Player.MoveUp = false;
        }

        private void UpButtonPress(Button button)
        {
            _Player.MoveUp = true;
        }

        private void FireButtonRelease(Button obj)
        {
            _Player.Shoot = false;
        }

        private void FireButtonPress(Button obj)
        {
            _Player.Shoot = true;
        }

        #endregion

        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        public override void Dispose()
        {
            base.Dispose();

            _UpButton.OnButtonPress = null;
            _UpButton.OnButtonRelease = null;
            _DownButton.OnButtonPress = null;
            _DownButton.OnButtonRelease = null;
            _LeftButton.OnButtonPress = null;
            _LeftButton.OnButtonRelease = null;
            _RightButton.OnButtonPress = null;
            _RightButton.OnButtonRelease = null;
            _FireButton.OnButtonPress = null;
            _FireButton.OnButtonRelease = null;

            _Fps.Dispose();
            _Player.Dispose();
            _BackgroundNear.Dispose();
            _BackgroundFar.Dispose();
            _PlanetsNear.Dispose();
            _PlanetsFar.Dispose();
            _Clusters.Dispose();
            _EnemySpawner.Dispose();
        }
    }
}
