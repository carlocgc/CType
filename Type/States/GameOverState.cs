using AmosShared.State;
using System;
using Type.Scenes;

namespace Type.States
{
    public class GameOverState : State
    {
        private GameOverScene _Scene;

        public GameOverState()
        {
        }

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
            _Scene.Visible = false;
            _Scene.IsComplete = false;
            _Scene.Dispose();
            Dispose();
        }
    }
}
