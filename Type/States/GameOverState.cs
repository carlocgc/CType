using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.State;
using Type.Scenes;

namespace Type.States
{
    public class GameOverState : State
    {
        private readonly Int32 _Score;

        private GameOverScene _Scene;

        public GameOverState(Int32 score)
        {
            _Score = score;
        }

        protected override void OnEnter()
        {
            _Scene = new GameOverScene { Visible = true };
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
            Dispose();
        }
    }
}
