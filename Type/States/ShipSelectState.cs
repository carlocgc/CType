using AmosShared.State;
using System;
using Type.Interfaces.Control;
using Type.Scenes;

namespace Type.States
{
    public class ShipSelectState : State, IShipSelectListener
    {
        private ShipSelectScene _Scene;

        private Int32 _Selection;

        private Boolean _IsComplete;

        public ShipSelectState()
        {

        }

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _Scene = new ShipSelectScene();
            _Scene.AlphaButton.RegisterListener(this);
            _Scene.BetaButton.RegisterListener(this);
            _Scene.GammaButton.RegisterListener(this);
            _Scene.Active = true;
        }

        /// <inheritdoc />
        public void OnShipSelected(Int32 id)
        {
            _Scene.Active = false;
            _Selection = id;
            _IsComplete = true;
        }

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
            if (_IsComplete) ChangeState(new PlayingState(_Selection));
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
        }

    }
}
