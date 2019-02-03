using AmosShared.State;
using System;
using Type.Data;
using Type.Scenes;

namespace Type.States
{
    public class GameOverState : State
    {
        private GameOverScene _Scene;

        protected override void OnEnter()
        {
            _Scene = new GameOverScene { Visible = true };
            _Scene.Start();
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
