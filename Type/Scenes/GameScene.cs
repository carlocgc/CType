using System;
using System.ComponentModel;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
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
        private Int32 _Score;
        /// <summary> Whether the player has ran out of lives, ends the playing state </summary>
        public Boolean IsGameOver;
        /// <summary> The enemy factory </summary>
        public EnemyFactory EnemySpawner;

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
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1900, -Renderer.Instance.TargetDimensions.Y / 2 + 1000),
                Visible = true,
                Scale = new Vector2(3, 3),
                Colour = new Vector4(1, 0, 0, 1)
            };
            AddDrawable(_ScoreText);

            _ScoreDisplay = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = _Score.ToString(),
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1650, -Renderer.Instance.TargetDimensions.Y / 2 + 1000),
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
            EnemySpawner = new EnemyFactory(this);
        }

        /// <summary>
        /// Called when the player dies checks if game is over
        /// </summary>
        private void OnPlayerDeath()
        {
            EnemySpawner.Reset();
            _LifeMeter.LoseLife();

            if (_LifeMeter.PlayerLives <= 0 && !IsGameOver)
            {
                IsGameOver = true;
            }
            else
            {
                EnemySpawner.StartWave();
            }
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartGame()
        {
            _Score = 0;

            _BackgroundNear.Start();
            _BackgroundFar.Start();
            _PlanetsFar.Start();
            _PlanetsNear.Start();
            _Clusters.Start();

            _LevelDisplay.ShowLevel(CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                EnemySpawner.SetLevelData(_LevelLoader.GetWaveData(CurrentLevel));
                CollisionController.Instance.IsActive = true;
            });
        }

        public void LevelComplete()
        {
            CurrentLevel++;
            if (CurrentLevel >= _MaxLevel)
            {
                // TODO Game Complete
            }
            else
            {
                _LevelDisplay.ShowLevel(CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    EnemySpawner.SetLevelData(_LevelLoader.GetWaveData(CurrentLevel));
                });
            }
        }

        /// <summary>
        /// Adds the value to the players current score
        /// </summary>
        /// <param name="amount"></param>
        public void UpdateScore(Int32 amount)
        {
            _Score += amount;
            _ScoreDisplay.Text = _Score.ToString();
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            _Fps.Dispose();
            _Player.Dispose();
            _BackgroundNear.Dispose();
            _BackgroundFar.Dispose();
            _PlanetsNear.Dispose();
            _PlanetsFar.Dispose();
            _Clusters.Dispose();
            EnemySpawner.Dispose();
        }
    }
}
