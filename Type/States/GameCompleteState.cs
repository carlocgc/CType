using AmosShared.State;
using System;
using Type.Scenes;

namespace Type.States
{
    public class GameCompleteState : State
    {
        private readonly Int32 _Score;

        private GameCompleteScene _Scene;

        public GameCompleteState(Int32 score)
        {
            _Score = score;
        }

        protected override void OnEnter()
        {
            _Scene = new GameCompleteScene { Visible = true };
            _Scene.Start(_Score);
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
