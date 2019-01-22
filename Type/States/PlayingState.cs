using AmosShared.State;
using System;
using Type.Controllers;
using Type.Factories;
using Type.Interfaces.Communication;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Scenes;

namespace Type.States
{
    /// <summary>
    /// Game play state
    /// </summary>
    public class PlayingState : State, IEnemyFactoryListener, IPlayerListener, IEnemyListener
    {
        /// <summary> Main scene of the game </summary>
        private GameScene _Scene;

        /// <summary> Total enemies killed or offscreen this wave</summary>
        private Int32 _EnemiesKilledThisWave;

        /// <summary> Whether all the enemies in the wave have been destroyed or left the screen </summary>
        private Boolean LevelComplete => _EnemiesKilledThisWave == EnemyFactory.Instance.WaveCount;

        protected override void OnEnter()
        {
            _Scene = new GameScene();
            _Scene.Visible = true;
            _Scene.StartGame();
        }

        public override Boolean IsComplete()
        {
            if (_Scene.IsGameOver) ChangeState(new GameOverState(_Scene.CurrentScore));
            else if (_Scene.IsGameComplete) ChangeState(new GameCompleteState(_Scene.CurrentScore));

            Boolean gameEnded = _Scene.IsGameOver || _Scene.IsGameComplete;
            return gameEnded;
        }

        #region Factory Events

        /// <inheritdoc />
        public void OnFactoryStarted()
        {
            CollisionController.Instance.IsActive = true;
        }

        /// <inheritdoc />
        public void OnWaveCreated()
        {
            EnemyFactory.Instance.Stop();
        }

        /// <inheritdoc />
        public void OnAllWavesCreated()
        {

        }

        #endregion

        #region Player Events

        /// <inheritdoc />
        public void OnPlayerHit(IPlayer player)
        {
        }

        /// <inheritdoc />
        public void OnPlayerDeath(IPlayer player)
        {
            _Scene.OnPlayerDeath();
        }

        #endregion

        #region Enemy Events

        /// <inheritdoc />
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            _EnemiesKilledThisWave++;
            _Scene.UpdateScore(enemy.Points);
        }

        /// <inheritdoc />
        public void OnEnemyOffscreen(IEnemy enemy)
        {
            _EnemiesKilledThisWave++;
            enemy.Dispose();
        }

        #endregion

        protected override void OnExit()
        {
            _Scene.Dispose();
            Dispose();
        }

    }
}
