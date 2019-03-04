using AmosShared.State;
using System;
using OpenTK;
using Type.Data;
using Type.Interfaces.Control;
using Type.Scenes;
using Type.Services;

namespace Type.States
{
    public class GameOverState : State, IInputListener
    {
        private GameOverScene _Scene;

        protected override void OnEnter()
        {
            _Scene = new GameOverScene { Visible = true };
            _Scene.Start();
            InputService.Instance.RegisterListener(this);
        }

        public override Boolean IsComplete()
        {
            if (_Scene.IsComplete) ChangeState(new MainMenuState());
            return _Scene.IsComplete;
        }

        protected override void OnExit()
        {
            GameStats.Instance.Clear();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            InputService.Instance.DeregisterListener(this);
            _Scene.Dispose();
            _Scene = null;
        }

        #region Implementation of IInputListener

        /// <summary> Informs the listener of input data </summary>
        /// <param name="data"> Data packet from the <see cref="InputManager"/> </param>
        public void UpdateInputData(ButtonEventData data)
        {
            switch (data.ID)
            {
                case ButtonData.Type.START:
                    {
                        if (data.State != ButtonData.State.PRESSED) return;
                        _Scene.IsComplete = true;
                        break;
                    }
            }
        }

        /// <summary>
        /// Informs the listener of directional input data
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> The distance the stick is pushed </param>
        public void UpdateDirectionData(Vector2 direction, Single strength)
        {

        }

        #endregion
    }
}
