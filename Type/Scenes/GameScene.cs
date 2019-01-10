using System;
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
        private ScrollingBackground _Background;
        /// <summary> The players ship </summary>
        private Player _Player;
        /// <summary> The players current score</summary>
        private Int32 _Score;
        /// <summary> Max level of the game </summary>
        private Int32 _MaxLevel = 4;
        /// <summary> Text printer that displays the score </summary>
        private TextDisplay _ScoreDisplay;
        /// <summary> THe word score displayed top left of screen </summary>
        private TextDisplay _ScoreText;
        /// <summary> UI element that displays the amount oif lives remaining </summary>
        private LifeMeter _LifeMeter;
        /// <summary> UI element that displays the current FPS </summary>
        private FpsCounter _FPS;
        /// <summary> Object that shows the current level text </summary>
        private LevelDisplay _LevelDisplay;
        /// <summary> Whether the player has ran out of lives, ends the playing state </summary>
        public Boolean IsGameOver;
        /// <summary> Loads wave data from text files </summary>
        private LevelLoader _LevelLoader;
        /// <summary> The enemy factory </summary>
        public EnemyFactory EnemySpawner;

        /// <summary> The current level </summary>
        public Int32 CurrentLevel { get; private set; }

        public GameScene()
        {
            _Background = new ScrollingBackground();

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
            _FPS = new FpsCounter();
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

            _Background.Start();

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
            _Player.Dispose();
            _Background.Dispose();
            EnemySpawner.Dispose();
        }
    }
}
