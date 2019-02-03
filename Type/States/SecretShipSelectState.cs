using AmosShared.Audio;
using AmosShared.State;
using System;
using Type.Controllers;
using Type.Interfaces.Control;
using Type.Scenes;

namespace Type.States
{
    public class SecretShipSelectState : State, IShipSelectListener
    {
        private SecretShipSelectScene _Scene;

        private Boolean _IsComplete;

        private Int32 _Selection;

        public SecretShipSelectState()
        {

        }

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _Scene = new SecretShipSelectScene();
            _Scene.OmegaButton.RegisterListener(this);
            _Scene.Active = true;
            AchievementController.Instance.PrototypeFound();
        }

        /// <inheritdoc />
        public void OnButtonPressed(Int32 id)
        {

        }

        /// <inheritdoc />
        public void OnButtonReleased(Int32 id)
        {
            _Scene.OmegaButton.DeregisterListener(this);
            _Selection = id;
            _IsComplete = true;
        }

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
            ChangeState(new PlayingState(_Selection));
            return _IsComplete;
        }

        /// <inheritdoc />
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
