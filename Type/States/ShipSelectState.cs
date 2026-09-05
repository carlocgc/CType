using AmosShared.State;
using System;
using AmosShared.Audio;
using OpenTK;
using Type.Controllers;
using Type.Data;
using Type.Interfaces.Control;
using Type.Scenes;
using Type.Services;
using Type.UI.Navigation;

namespace Type.States
{
    public class ShipSelectState : State, IShipSelectListener, IBackButtonListener, IInputListener
    {
        /// <summary> Identifier of the hidden craft, which is also an achievement </summary>
        private const Int32 OmegaId = 3;

        private ShipSelectScene _Scene;

        private AudioPlayer _Music;

        private Int32 _Selection;

        private Boolean _IsComplete;

        private Boolean _Returning;

        /// <summary> Moves focus between the craft and confirms a choice </summary>
        private MenuNavigator _Navigator;

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
            _Scene.OmegaButton.RegisterListener(this);
            _Scene.RegisterListener(this);
            _Scene.Active = true;

            _Navigator = new MenuNavigator { OnCancel = () => _Scene.BackPressed() };
            _Navigator.Add(_Scene.AlphaButton);
            _Navigator.Add(_Scene.BetaButton);
            _Navigator.Add(_Scene.GammaButton);
            _Navigator.Add(_Scene.OmegaButton);
            _Navigator.FocusFirst();

            InputService.Instance.RegisterListener(this);
        }

        /// <inheritdoc />
        public void OnButtonReleased(Int32 id)
        {
            _Selection = id;

            // Only reachable once the craft has been unlocked, so reaching it is the achievement.
            if (id == OmegaId) AchievementController.Instance.PrototypeFound();

            StopListening();
            _IsComplete = true;
        }

        /// <summary>
        /// Stops listening to the craft cards and shuts the screen down, so that nothing can be
        /// chosen while the state is on its way out
        /// </summary>
        private void StopListening()
        {
            _Scene.AlphaButton.DeregisterListener(this);
            _Scene.BetaButton.DeregisterListener(this);
            _Scene.GammaButton.DeregisterListener(this);
            _Scene.OmegaButton.DeregisterListener(this);
            _Scene.Active = false;
        }

        #region Implementation of IBackButtonListener

        /// <summary> Invoked when the back button is pressed </summary>
        public void OnBackPressed()
        {
            StopListening();
            _Returning = true;
            _IsComplete = true;
        }

        #endregion

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
            if (_IsComplete && !_Returning)
            {
                _Music.Stop();
                ChangeState(new PlayingState(_Selection));
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
            _Navigator?.Dispose();
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
