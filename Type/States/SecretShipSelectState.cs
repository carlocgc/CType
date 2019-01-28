using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Audio;
using AmosShared.State;
using Type.Interfaces.Control;
using Type.Scenes;

namespace Type.States
{
    public class SecretShipSelectState : State, IShipSelectListener
    {
        private readonly SecretShipSelectScene _Scene;

        private Boolean _IsComplete;

        private Int32 _Selection;

        public SecretShipSelectState()
        {
            _Scene = new SecretShipSelectScene();
        }

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _Scene.OmegaButton.RegisterListener(this);
            _Scene.Active = true;
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
            AudioManager.Instance.Dispose();
        }


    }
}
