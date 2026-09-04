using AmosShared.State;
using System;
using AmosShared.Audio;
using OpenTK;
using Type.Data;
using Type.Interfaces.Control;
using Type.Scenes;
using Type.Services;
using Type.UI.Navigation;

namespace Type.States
{
    public class ShipSelectState : State, IShipSelectListener, IBackButtonListener, IInputListener
    {
        private ShipSelectScene _Scene;

        private AudioPlayer _Music;

        private Int32 _Selection;

        private Boolean _IsComplete;

        private Boolean _FirstButtonHeld;

        private Boolean _SecondButtonHeld;

        private Boolean _ThirdButtonHeld;

        private Boolean _Returning;

        /// <summary> Moves focus between the craft and confirms a choice </summary>
        private MenuNavigator _Navigator;

        /// <summary> Whether the hidden craft has been requested this visit </summary>
        private Boolean _SecretRequested;

        /// <summary>
        /// Whether to open the secret menu. Touch still reaches it by holding all three cards
        /// at once; a pointerless input reaches it through the SECRET action.
        /// </summary>
        private Boolean _EnteringSecretMenu => _SecretRequested || (_FirstButtonHeld && _SecondButtonHeld && _ThirdButtonHeld);

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

            _Navigator = new MenuNavigator { OnCancel = () => _Scene.BackPressed() };
            _Navigator.Add(_Scene.AlphaButton);
            _Navigator.Add(_Scene.BetaButton);
            _Navigator.Add(_Scene.GammaButton);
            _Navigator.FocusFirst();

            InputService.Instance.RegisterListener(this);
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
            _Selection = id;
            OnSelection();
        }

        public void OnSelection()
        {
            _Scene.AlphaButton.DeregisterListener(this);
            _Scene.BetaButton.DeregisterListener(this);
            _Scene.GammaButton.DeregisterListener(this);
            _Scene.Active = false;
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
            InputService.Instance.DeregisterListener(this);
            _Navigator.Dispose();
            _Navigator = null;
            _Music = null;
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
                case ButtonData.Type.SECRET:
                    {
                        // Stands in for the touch gesture of holding all three cards at once,
                        // which a focus cursor cannot express.
                        if (data.State != ButtonData.State.PRESSED) return;
                        _SecretRequested = true;
                        OnSelection();
                        break;
                    }
                case ButtonData.Type.BACK:
                    {
                        if (data.State != ButtonData.State.PRESSED) return;
                        _Scene.BackPressed();
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
