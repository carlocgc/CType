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
            _Scene = new MainMenuScene { Visible = true };
            _Scene.Show();
        }

        public override Boolean IsComplete()
        {
            if (_Scene.IsComplete) ChangeState(new ShipSelectState());
            return _Scene.IsComplete;
        }

        protected override void OnExit()
        {
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
