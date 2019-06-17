using AmosShared.Audio;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Linq;
using AmosShared.Base;
using AmosShared.State;
using Type.Controllers;
using Type.Data;
using Type.Factories;
using Type.Interfaces.Control;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using Type.Scenes;
using Type.Services;
using Type.UI;

namespace Type.States
{
    /// <summary>
    /// Game play state
    /// </summary>
    public class PlayingState : State, IPlayerListener, IEnemyListener, IEnemyFactoryListener, IPowerupListener, IPowerupFactoryListener, IUpdatable, IInputListener
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
        /// <summary> Whether the game is paused </summary>
        private Boolean _Paused;
        /// <summary> Whether the level has started </summary>
        private Boolean _LevelStarted;
        /// <summary> Whether the game is over </summary>
        private Boolean _GameOver;
        /// <summary> Whether the game is complete </summary>
        private Boolean _GameComplete;
        /// <summary> Whether the nuke button was pressed last update, used to prevent multiple triggers of nukes </summary>
        private Boolean _NukePressed;
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
            _ScoreDisplay = _UIScene.ScoreDisplay;
            _LifeMeter = _UIScene.LifeMeter;
            _LevelDisplay = _UIScene.LevelDisplay;
            _UIScene.ShowOnScreenControls(true);
            _UIScene.Visible = true;

            _GameScene.StartBackgroundScroll();

            _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                CollisionController.Instance.IsActive = true;
            });

            _Player.Spawn();
            GameStats.Instance.GameStart();
            InputService.Instance.RegisterListener(this);
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>If true then this state is considered complete and control will be passed over to <see cref="State.NextState"/></summary>
        /// <returns></returns>
        public override Boolean IsComplete()
        {
            if (_GameOver) ChangeState(new GameOverState());
            else if (_GameComplete) ChangeState(new GameCompleteState(_PlayerType));

            Boolean gameEnded = _GameOver || _GameComplete;
            return gameEnded;
        }

        #region Player

        /// <summary>
        /// Invoked when a life is added
        /// </summary>
        /// <param name="player"></param>
        /// <param name="points"></param>
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
            InputService.Instance.Vibrate(0, true, TimeSpan.FromMilliseconds(200));

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
        public void OnNukeAdded(Int32 points)
        {
            if (_CurrentNukes >= _MaxNukes)
            {
                UpdateScore(points);
                new AudioPlayer("Content/Audio/points_instead.wav", false, AudioManager.Category.EFFECT, 1);
                return;
            }
            _CurrentNukes++;
            _UIScene.NukeDisplay.NukeCount = _CurrentNukes;
            new AudioPlayer("Content/Audio/nuke_pickup.wav", false, AudioManager.Category.EFFECT, 1);
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
            _LevelStarted = true;
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

            AchievementController.Instance.LevelCompleted(_CurrentLevel);

            if (_CurrentLevel >= _MaxLevel) GameCompleted();
            else
            {
                _CurrentLevel++;
                _EnemiesDestroyedThisLevel = 0;
                _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                    _LevelStarted = true;
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
            _UIScene.ShowOnScreenControls(false);
            _UIScene.Visible = false;
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
            _UIScene.ShowOnScreenControls(false);
            _UIScene.Visible = false;
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
            if (!_LevelStarted) return;
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


        #region Implementation of IInputListener

        /// <summary>
        /// Update data from the analog stic
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> The distance the stick is pushed </param>
        public void UpdateDirectionData(Vector2 direction, Single strength)
        {
        }

        /// <summary> Informs the listener of input events </summary>
        /// <param name="data"> Data packet from the <see cref="InputService"/> </param>
        public void UpdateInputData(ButtonEventData data)
        {
            switch (data.ID)
            {
                case ButtonData.Type.NUKE:
                    {
                        if (data.State == ButtonData.State.RELEASED) _NukePressed = false;

                        if (data.State != ButtonData.State.PRESSED || _CurrentNukes <= 0) return;

                        if (_NukePressed) return;

                        _CurrentNukes--;
                        _UIScene.NukeDisplay.NukeCount = _CurrentNukes;
                        CollisionController.Instance.ClearProjectiles();

                        foreach (IEnemy enemy in _GameScene.Enemies.Where(e => e.CanBeRoadKilled))
                        {
                            if (!enemy.IsDisposed && !enemy.IsDestroyed) enemy.Destroy();
                        }

                        _GameScene.ShowNukeEffect();
                        new AudioPlayer("Content/Audio/nuke.wav", false, AudioManager.Category.EFFECT, 1);
                        InputService.Instance.Vibrate(0, true, TimeSpan.FromMilliseconds(500));

                        _NukePressed = true;

                        break;
                    }
                case ButtonData.Type.START:
                    {
                        if (data.State != ButtonData.State.PRESSED) return;
                        if (!_Paused)
                        {
                            _Paused = true;
                            Game.GameTime.Multiplier = 0;
                            _UIScene.SetPaused(true);
                            InputService.Instance.SetPaused(true);
                            return;
                        }
                        if (_Paused)
                        {
                            _Paused = false;
                            Game.GameTime.Multiplier = 1;
                            _UIScene.SetPaused(false);
                            InputService.Instance.SetPaused(false);
                        }
                        break;
                    }
            }
        }

        #endregion

        /// <inheritdoc />
        public override void Dispose()
        {
            if (IsDisposed) return;
            base.Dispose();

            InputService.Instance.DeregisterListener(this);
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

    }
}
