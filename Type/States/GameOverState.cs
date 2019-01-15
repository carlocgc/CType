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

        public GameOverState(Int32 score)
        {
            _Score = score;
        }

        protected override void OnEnter()
        {
            GameOverScene.Instance.Show(_Score);
        }

        public override Boolean IsComplete()
        {
            if (GameOverScene.Instance.IsConfirmed) ChangeState(new MainMenuState());
            return GameOverScene.Instance.IsConfirmed;
        }

        protected override void OnExit()
        {
            GameOverScene.Instance.Visible = false;
            Dispose();
        }
    }
}
