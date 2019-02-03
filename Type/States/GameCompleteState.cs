using AmosShared.State;
using System;
using Type.Scenes;

namespace Type.States
{
    public class GameCompleteState : State
    {
        private GameCompleteScene _Scene;

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
