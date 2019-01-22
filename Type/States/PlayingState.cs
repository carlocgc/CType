using AmosShared.State;
using System;
using Type.Interfaces.Communication;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Scenes;

namespace Type.States
{
    public class PlayingState : State, IEnemyFactoryListener, IPlayerListener, IEnemyListener
    {
        private GameScene _Scene;



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

        }

        /// <inheritdoc />
        public void OnWaveCreated()
        {

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

        }

        #endregion

        #region Enemy Events

        /// <inheritdoc />
        public void OnEnemyDestroyed(IEnemy enemy)
        {

        }

        /// <inheritdoc />
        public void OnEnemyOffscreen(IEnemy enemy)
        {

        }

        #endregion

        protected override void OnExit()
        {
            _Scene.Dispose();
            Dispose();
        }

    }
}
