using AmosShared.State;
using System;
using AmosShared.Audio;
using Type.Interfaces.Control;
using Type.Scenes;

namespace Type.States
{
    public class ShipSelectState : State, IShipSelectListener
    {
        private ShipSelectScene _Scene;

        private Int32 _Selection;

        private Boolean _IsComplete;

        private Boolean _FirstButtonHeld;

        private Boolean _SecondButtonHeld;

        private Boolean _ThirdButtonHeld;

        private Boolean _EnteringSecretMenu => _FirstButtonHeld && _SecondButtonHeld && _ThirdButtonHeld;

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
        public void OnButtonPressed(Int32 id)
        {
            switch (id)
            {
                case 0:
                    {
                        _FirstButtonHeld = true;
                        break;
                    }
                case 1:
                    {
                        _SecondButtonHeld = true;
                        break;
                    }
                case 2:
                    {
                        _ThirdButtonHeld = true;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("Ship select button does not exist");
            }
        }

        /// <inheritdoc />
        public void OnButtonReleased(Int32 id)
        {
            _Scene.AlphaButton.DeregisterListener(this);
            _Scene.BetaButton.DeregisterListener(this);
            _Scene.GammaButton.DeregisterListener(this);
            _Scene.Active = false;
            _Selection = id;
            _IsComplete = true;
        }

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
            if (_IsComplete && !_EnteringSecretMenu)
            {
                ChangeState(new PlayingState(_Selection));
                AudioManager.Instance.Dispose();
            }

            if (_IsComplete && _EnteringSecretMenu)
            {
                ChangeState(new SecretShipSelectState());
            }

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
