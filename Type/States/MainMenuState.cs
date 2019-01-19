using AmosShared.State;
using System;
using Type.Data;
using Type.Scenes;

namespace Type.States
{
    public class MainMenuState : State
    {
        private MainMenuScene _Scene;

        protected override void OnEnter()
        {
            GameStats.Instance.Clear();
            _Scene = new MainMenuScene {Visible = true};
            _Scene.Show();            
        }

        public override Boolean IsComplete()
        {
            if (_Scene.IsComplete) ChangeState(new PlayingState());
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
