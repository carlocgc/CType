using AmosShared.State;
using System;
using AmosShared.Audio;
using Type.Interfaces.Control;
using Type.Scenes;

namespace Type.States
{
    public class ShipSelectState : State, IShipSelectListener, IBackButtonListener
    {
        private ShipSelectScene _Scene;

        private AudioPlayer _Music;

        private Int32 _Selection;

        private Boolean _IsComplete;

        private Boolean _FirstButtonHeld;

        private Boolean _SecondButtonHeld;

        private Boolean _ThirdButtonHeld;

        private Boolean _Returning;

        private Boolean _EnteringSecretMenu => _FirstButtonHeld && _SecondButtonHeld && _ThirdButtonHeld;

        public ShipSelectState(AudioPlayer music)
        {
            _Music = music;
        }

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _Scene = new ShipSelectScene();
            _Scene.AlphaButton.RegisterListener(this);
            _Scene.BetaButton.RegisterListener(this);
            _Scene.GammaButton.RegisterListener(this);
            _Scene.RegisterListener(this);
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

        #region Implementation of IBackButtonListener

        /// <summary> Invoked when the back button is pressed </summary>
        public void OnBackPressed()
        {
            _Scene.AlphaButton.DeregisterListener(this);
            _Scene.BetaButton.DeregisterListener(this);
            _Scene.GammaButton.DeregisterListener(this);
            _Scene.Active = false;
            _Returning = true;
            _IsComplete = true;
        }

        #endregion

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
            if (_IsComplete && !_EnteringSecretMenu && !_Returning)
            {
                _Music.Stop();
                ChangeState(new PlayingState(_Selection));
            }

            if (_IsComplete && _EnteringSecretMenu)
            {
                ChangeState(new SecretShipSelectState(_Music));
            }

            if (_IsComplete && _Returning)
            {
                ChangeState(new MainMenuState(_Music));
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
            _Music = null;
            _Scene.Dispose();
            _Scene = null;
        }
    }
}
