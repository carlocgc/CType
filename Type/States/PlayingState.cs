using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.State;
using Type.Scenes;

namespace Type.States
{
    public class PlayingState : State
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
            if (_Scene.IsGameOver) ChangeState(new GameOverState());
            return _Scene.IsGameOver;
        }

        protected override void OnExit()
        {
            _Scene.Dispose();
            Dispose();
        }
    }
}
