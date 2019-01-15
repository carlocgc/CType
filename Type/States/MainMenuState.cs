using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.State;
using Type.Scenes;

namespace Type.States
{
    public class MainMenuState : State
    {
        protected override void OnEnter()
        {
            MainMenuScene.Instance.Show();
        }

        public override Boolean IsComplete()
        {
            if (MainMenuScene.Instance.IsComplete) ChangeState(new PlayingState());
            return MainMenuScene.Instance.IsComplete;
        }

        protected override void OnExit()
        {
            MainMenuScene.Instance.Visible = false;
            MainMenuScene.Instance.IsComplete = false;
            Dispose();
        }
    }
}
