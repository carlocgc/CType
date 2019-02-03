using AmosShared.State;
using System;
using Type.Controllers;
using Type.Data;
using Type.Scenes;

namespace Type.States
{
    public class GameCompleteState : State
    {
        private GameCompleteScene _Scene;

        private readonly Int32 _PlayerShipId;

        public GameCompleteState(Int32 playerShipId)
        {
            _PlayerShipId = playerShipId;
        }

        protected override void OnEnter()
        {
            _Scene = new GameCompleteScene { Visible = true };
            _Scene.Start();
            AchievementController.Instance.GameComplete(_PlayerShipId);
        }

        public override Boolean IsComplete()
        {
            if (_Scene.IsComplete) ChangeState(new MainMenuState());
            return _Scene.IsComplete;
        }

        protected override void OnExit()
        {
            GameStats.Instance.Clear();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Scene.Dispose();
            _Scene = null;
        }
    }
}
