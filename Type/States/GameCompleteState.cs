using AmosShared.State;
using System;
using Type.Data;
using Type.Scenes;

namespace Type.States
{
    public class GameCompleteState : State
    {
        private GameCompleteScene _Scene;

        public GameCompleteState()
        {
        }

        protected override void OnEnter()
        {
            _Scene = new GameCompleteScene { Visible = true };
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
        }
    }
}
