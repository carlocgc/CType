using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using AmosShared.State;
using OpenTK;
using System;
using System.Linq;
using Type.Controllers;
using Type.Data;
using Type.Factories;
using Type.Interfaces.Control;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using Type.Scenes;
using Type.UI;

namespace Type.States
{
    /// <summary>
    /// Game play state
    /// </summary>
    public class PlayingState : State, IPlayerListener, IEnemyListener, IEnemyFactoryListener, IPowerupListener, INukeButtonListener, IPowerupFactoryListener, IUpdatable
    {
        /// <summary> Max level of the game </summary>
        private readonly Int32 _MaxLevel = 20;
        /// <summary> Maximum amount of nukes the player can hold </summary>
        private readonly Int32 _MaxNukes = 3;
        /// <summary> THe type of player craft </summary>
        private readonly Int32 _PlayerType;

        /// <summary> Factory that will create enemies </summary>
        private EnemyFactory _EnemyFactory;
        /// <summary> Factory that creates power ups </summary>
        private PowerupFactory _PowerupFactory;
        /// <summary> Scene for game objects </summary>
        private GameScene _GameScene;
        /// <summary> Scene for UI objects </summary>
        private UIScene _UIScene;
        /// <summary> Displays the current level as text on the screen </summary>
        private LevelDisplay _LevelDisplay;
        /// <summary> Text to display the score </summary>
        private TextDisplay _ScoreDisplay;
        /// <summary> Displays the players current lives </summary>
        private LifeMeter _LifeMeter;
        /// <summary> The player </summary>
        private IPlayer _Player;
        /// <summary> Whether the game is over </summary>
        private Boolean _GameOver;
        /// <summary> Whether the game is complete </summary>
        private Boolean _GameComplete;
        /// <summary> Whether the level can be completed </summary>
        private Boolean _LevelCanEnd;
        /// <summary> The current level </summary>
        private Int32 _CurrentLevel;
        /// <summary> Total enemies in this level </summary>
        private Int32 _EnemiesInLevel;
        /// <summary> Total enemies destroyed this level </summary>
        private Int32 _EnemiesDestroyedThisLevel;
        /// <summary> amount of nukes the player has </summary>
        private Int32 _CurrentNukes;

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        public PlayingState(Int32 type)
        {
            _PlayerType = type;
        }

        protected override void OnEnter()
        {
            _CurrentLevel = 1;

            _EnemyFactory = new EnemyFactory();
            _EnemyFactory.RegisterListener(this);
            _EnemyFactory.ParentState = this;

            _PowerupFactory = new PowerupFactory();
            _PowerupFactory.RegisterListener(this);

            _GameScene = new GameScene(_PlayerType) { Visible = true };

            _Player = _GameScene.Player;
            _Player.RegisterListener(this);
            CollisionController.Instance.RegisterPlayer(_Player);

            _UIScene = new UIScene(_PlayerType);
            _UIScene.RegisterListener(_Player);
            _UIScene.AnalogStick.RegisterListener(_Player);
            _UIScene.NukeButton.RegisterListener(this);
            _ScoreDisplay = _UIScene.ScoreDisplay;
            _LifeMeter = _UIScene.LifeMeter;
            _LevelDisplay = _UIScene.LevelDisplay;
            _UIScene.Active = true;

            _GameScene.StartBackgroundScroll();

            _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                CollisionController.Instance.IsActive = true;
            });

            _Player.Spawn();
            GameStats.Instance.GameStart();

            UpdateManager.Instance.AddUpdatable(this);
        }

        public override Boolean IsComplete()
        {
            if (_GameOver) ChangeState(new GameOverState());
            else if (_GameComplete) ChangeState(new GameCompleteState(_PlayerType));

            Boolean gameEnded = _GameOver || _GameComplete;
            return gameEnded;
        }

        #region Player

        /// <inheritdoc />
        public void OnLifeAdded(IPlayer player, Int32 points)
        {
            if (_LifeMeter.PlayerLives == 5)
            {
                UpdateScore(points);
                new AudioPlayer("Content/Audio/points_instead.wav", false, AudioManager.Category.EFFECT, 1);
                return;
            }
            _LifeMeter.AddLife();
        }

        /// <inheritdoc />
        public void OnPointPickup(Int32 value)
        {
            UpdateScore(value);
        }

        /// <inheritdoc />
        public void OnPlayerHit(IPlayer player)
        {
        }

        /// <inheritdoc />
        public void OnPlayerDeath(IPlayer player, Int32 probeCount, Vector2 position)
        {
            _LifeMeter.LoseLife();
            _GameScene.RemovePowerUps();

            if (_LifeMeter.PlayerLives > 0)
            {
                _Player.Spawn();
                if (probeCount > 0)
                {
                    _PowerupFactory.Create(1, position, _CurrentLevel);
                }
            }
            else
            {
                GameOver();
            }
        }

        /// <summary>
        /// Invoked when a nuke is collected by the player
        /// </summary>
        public void OnNukeAdded()
        {
            if (_CurrentNukes >= _MaxNukes) return;
            _CurrentNukes++;
            _UIScene.NukeButton.NukeCount = _CurrentNukes;
        }

        #endregion

        #region Enemy

        /// <summary>
        /// Invoked when the factory has started a new level
        /// </summary>
        /// <param name="levelTotal"></param>
        public void OnLevelStarted(Int32 levelTotal)
        {
            _EnemiesDestroyedThisLevel = 0;
            _EnemiesInLevel = levelTotal;
        }

        /// <inheritdoc />
        public void EnemyCreated(IEnemy enemy)
        {
            _GameScene.Enemies.Add(enemy);
        }

        /// <summary>
        /// Invoked when the factory has finished spawning the levels enemies
        /// </summary>
        public void OnLevelFinishedSpawning()
        {
            _LevelCanEnd = true;
        }

        /// <inheritdoc />
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            _EnemiesDestroyedThisLevel++;
            UpdateScore(enemy.Points);
            _PowerupFactory.Create(0, enemy.Position, _CurrentLevel);
        }

        /// <inheritdoc />
        public void OnEnemyOffscreen(IEnemy enemy)
        {
            _EnemiesDestroyedThisLevel++;
            enemy.Dispose();
        }

        #endregion

        #region  Powerups

        /// <inheritdoc />
        public void OnPowerupCreated(IPowerup powerup)
        {
            _GameScene.Powerups.Add(powerup);
            CollisionController.Instance.RegisterPowerup(powerup);
        }

        /// <inheritdoc />
        public void OnPowerupApplied(IPowerup powerup)
        {
            _GameScene.Powerups.Remove(powerup);
        }

        /// <inheritdoc />
        public void OnPowerupExpired(IPowerup powerup)
        {
            _GameScene.Powerups.Remove(powerup);
        }

        #endregion

        #region Game_Logic

        /// <summary>
        /// Adds the value to the players current score
        /// </summary>
        /// <param name="amount"></param>
        private void UpdateScore(Int32 amount)
        {
            GameStats.Instance.Score += amount;
            _ScoreDisplay.Text = GameStats.Instance.Score.ToString();
        }

        /// <summary>
        /// Sets the next level data and displays the current level, ends the game if complete
        /// </summary>
        private void LevelComplete()
        {
            if (_GameOver) return;

            _LevelCanEnd = false;

            AchievementController.Instance.LevelCompleted(_CurrentLevel);

            if (_CurrentLevel >= _MaxLevel) GameCompleted();
            else
            {
                _CurrentLevel++;
                _EnemiesDestroyedThisLevel = 0;
                _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                });
            }
        }

        /// <summary>
        /// Ends the playing state and set the next state to be <see cref="GameCompleteState"/>
        /// </summary>
        private void GameCompleted()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();
            _EnemyFactory.Stop();
            _UIScene.Active = false;
            _GameComplete = true;
        }

        /// <summary>
        /// Ends the game and sets the next state to Game over state
        /// </summary>
        private void GameOver()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();
            _LevelDisplay.Dispose();
            _EnemyFactory.Dispose();
            _UIScene.Active = false;
            _GameOver = true;
        }

        #endregion

        protected override void OnExit()
        {

        }

        /// <summary> Updates the state </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeSinceUpdate)
        {
            base.Update(timeSinceUpdate);

            if (!_LevelCanEnd) return;

            if (_EnemiesDestroyedThisLevel >= _EnemiesInLevel)
            {
                LevelComplete();
            }
        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }


        /// <inheritdoc />
        public override void Dispose()
        {
            if (IsDisposed) return;
            base.Dispose();

            UpdateManager.Instance.RemoveUpdatable(this);
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();

            _LevelDisplay = null;
            _ScoreDisplay = null;
            _LifeMeter = null;
            _Player = null;

            _PowerupFactory.Dispose();
            _PowerupFactory = null;
            _EnemyFactory.Dispose();
            _EnemyFactory = null;
            _GameScene.Dispose();
            _GameScene = null;
            _UIScene.Dispose();
            _UIScene = null;
        }

        #region Implementation of INukeButtonListener

        /// <summary> Invoked when the nuke button is pressed </summary>
        public void OnNukeButtonPressed()
        {
            if (_CurrentNukes <= 0) return;
            _CurrentNukes--;
            _UIScene.NukeButton.NukeCount = _CurrentNukes;
            CollisionController.Instance.ClearProjectiles();
            foreach (IEnemy enemy in _GameScene.Enemies.Where(e => e.CanBeRoadKilled))
            {
                enemy.Destroy();
            }
            new AudioPlayer("Content/Audio/nuke.wav", false, AudioManager.Category.EFFECT, 1);
        }

        #endregion
    }
}
