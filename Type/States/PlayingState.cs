using AmosShared.State;
using System;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Controllers;
using Type.Data;
using Type.Factories;
using Type.Interfaces.Control;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Scenes;
using Type.UI;

namespace Type.States
{
    /// <summary>
    /// Game play state
    /// </summary>
    public class PlayingState : State, IPlayerListener, IEnemyListener
    {
        /// <summary> Max level of the game </summary>
        private readonly Int32 _MaxLevel = 9;

        /// <summary> Scene for game objects </summary>
        private GameScene _GameScene;
        /// <summary> Scene for UI objects </summary>
        private UIScene _UIScene;
        /// <summary> The player </summary>
        private IPlayer _Player;
        /// <summary> The current level </summary>
        private Int32 _CurrentLevel;
        /// <summary> Whether the game is over </summary>
        private Boolean _GameOver;
        /// <summary> Whether the game is complete </summary>
        private Boolean _GameComplete;
        /// <summary> Displays the current level as text on the screen </summary>
        private LevelDisplay _LevelDisplay;
        /// <summary> Text to display the score </summary>
        private TextDisplay _ScoreDisplay;
        /// <summary> Displays the players current lives </summary>
        private LifeMeter _LifeMeter;

        protected override void OnEnter()
        {
            _CurrentLevel = 1;

            EnemyFactory.Instance.ParentState = this;

            _GameScene = new GameScene();
            _GameScene.Visible = true;

            _Player = _GameScene.Player;
            _Player.RegisterListener(this);

            _UIScene = new UIScene();
            _UIScene.RegisterListener(_Player);
            _UIScene.AnalogStick.RegisterListener(_Player);
            _ScoreDisplay = _UIScene.ScoreDisplay;
            _LifeMeter = _UIScene.LifeMeter;
            _LevelDisplay = _UIScene.LevelDisplay;
            _UIScene.Active = true;

            _GameScene.StartBackgroundScroll();

            GameStats.Instance.Score = 0;
            GameStats.Instance.GameStart();

            _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                EnemyFactory.Instance.Start(LevelLoader.GetWaveData(_CurrentLevel));
                CollisionController.Instance.IsActive = true;
            });
        }

        public override Boolean IsComplete()
        {
            if (_GameOver) ChangeState(new GameOverState());
            else if (_GameComplete) ChangeState(new GameCompleteState());

            Boolean gameEnded = _GameOver || _GameComplete;
            return gameEnded;
        }

        #region Player

        /// <inheritdoc />
        public void OnPlayerHit(IPlayer player)
        {

        }

        /// <inheritdoc />
        public void OnPlayerDeath(IPlayer player)
        {
            PlayerDeath();
        }

        #endregion

        #region Enemy

        /// <inheritdoc />
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            UpdateScore(enemy.Points);
            if (!EnemyFactory.Instance.Creating && CollisionController.Instance.Enemies == 0) LevelComplete();
        }

        /// <inheritdoc />
        public void OnEnemyOffscreen(IEnemy enemy)
        {
            enemy.Dispose();
            if (!EnemyFactory.Instance.Creating && CollisionController.Instance.Enemies == 0) LevelComplete();
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
            if (_CurrentLevel >= _MaxLevel) GameCompleted();
            else
            {
                _CurrentLevel++;
                _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    EnemyFactory.Instance.Start(LevelLoader.GetWaveData(_CurrentLevel));
                });
            }
        }

        /// <summary>
        /// Called when the player dies checks if game is over
        /// </summary>
        private void PlayerDeath()
        {
            CollisionController.Instance.ClearObjects();

            _LifeMeter.LoseLife();

            if (_LifeMeter.PlayerLives > 0)
            {
                _Player.Spawn();
                EnemyFactory.Instance.RestartWave();
            }
            else
            {
                GameOver();
            }
        }

        /// <summary>
        /// Ends the playing state and set the next state to be <see cref="GameCompleteState"/>
        /// </summary>
        private void GameCompleted()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.ClearObjects();
            EnemyFactory.Instance.Stop();
            _UIScene.Active = false;
            _GameComplete = true;
        }

        /// <summary>
        /// Ends the game and sets the next state to Game over state
        /// </summary>
        private void GameOver()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.ClearObjects();
            EnemyFactory.Instance.Stop();
            _UIScene.Active = false;
            _GameOver = true;
        }

        #endregion

        protected override void OnExit()
        {
            EnemyFactory.Instance.ParentState = null;
            CollisionController.Instance.IsActive = false;
            _GameScene.Dispose();
            _UIScene.Dispose();
            Dispose();
        }
    }
}
