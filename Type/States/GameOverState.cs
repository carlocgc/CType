using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.State;
using Type.Scenes;

namespace Type.States
{
    public class GameOverState : State
    {
        protected override void OnEnter()
        {
            GameOverScene.Instance.IsConfirmed = false;
            GameOverScene.Instance.Visible = true;
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
