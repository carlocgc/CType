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
    public class PlayingState : State, IPlayerListener, IEnemyListener, IUIListener
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

        protected override void OnEnter()
        {
            _CurrentLevel = 1;

            CollisionController.Instance.ClearObjects();

            EnemyFactory.Instance.Reset();
            EnemyFactory.Instance.ParentState = this;

            _GameScene = new GameScene();
            _GameScene.Visible = true;

            _Player = _GameScene.Player;

            _UIScene = new UIScene();
            _UIScene.Visible = true;
            _UIScene.RegisterListener(_Player);

            _ScoreDisplay = _UIScene


            GameStats.Instance.Score = 0;
            GameStats.Instance.GameStart();

            _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                EnemyFactory.Instance.SetLevelData(LevelLoader.GetWaveData(_CurrentLevel));
                EnemyFactory.Instance.Start();
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

        }

        #endregion

        #region Enemy

        /// <inheritdoc />
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            GameStats.Instance.Score += enemy.Points;
        }

        /// <inheritdoc />
        public void OnEnemyOffscreen(IEnemy enemy)
        {
            enemy.Dispose();
        }

        #endregion

        #region Game_Logic

        /// <summary>
        /// Adds the value to the players current score
        /// </summary>
        /// <param name="amount"></param>
        public void UpdateScore(Int32 amount)
        {
            GameStats.Instance.Score += CurrentScore;
            _ScoreDisplay.Text = CurrentScore.ToString();
        }

        /// <summary>
        /// Sets the next level data and displays the current level, ends the game if complete
        /// </summary>
        public void LevelComplete()
        {
            if (_CurrentLevel >= _MaxLevel)
            {
                GameCompleted();
            }
            else
            {
                _CurrentLevel++;
                _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    EnemyFactory.Instance.SetLevelData(LevelLoader.GetWaveData(_CurrentLevel));
                });
            }
        }

        /// <summary>
        /// Called when the player dies checks if game is over
        /// </summary>
        public void OnPlayerDeath()
        {
            CollisionController.Instance.ClearObjects();
            EnemyFactory.Instance.Reset();

            _LifeMeter.LoseLife();

            if (_LifeMeter.PlayerLives > 0 || IsGameOver) return;

            GameStats.Instance.GameEnd();
            SetButtonsEnabled(false);
            SetButtonsVisible(false);
            IsGameOver = true;
        }

        /// <summary>
        /// Ends the playing state and set the next state to be <see cref="GameCompleteState"/>
        /// </summary>
        private void GameCompleted()
        {
            GameStats.Instance.GameEnd();
            EnemyFactory.Instance.Reset();
            IsGameComplete = true;
            SetButtonsEnabled(false);
            SetButtonsVisible(false);
        }

        #endregion

        #region Button_Presses

        /// <inheritdoc />
        public void UpdateAnalogData(Vector2 direction, Single strength)
        {

        }

        /// <inheritdoc />
        public void FireButtonPressed()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FireButtonReleased()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ShieldButtonPressed()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ProbeButtonPressed()
        {
            throw new NotImplementedException();
        }


        #endregion

        protected override void OnExit()
        {
            _GameScene.Dispose();
            _UIScene.Dispose();
            Dispose();
        }


    }
}
