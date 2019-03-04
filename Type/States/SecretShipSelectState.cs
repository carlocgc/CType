using AmosShared.Audio;
using AmosShared.State;
using System;
using Type.Controllers;
using Type.Interfaces.Control;
using Type.Scenes;

namespace Type.States
{
    public class SecretShipSelectState : State, IShipSelectListener, IBackButtonListener
    {
        private SecretShipSelectScene _Scene;

        private AudioPlayer _Music;

        private Boolean _Selected;

        private Boolean _Returning;

        private Boolean _IsComplete;

        private Int32 _Selection;

        public SecretShipSelectState(AudioPlayer music)
        {
            _Music = music;
        }

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _Scene = new SecretShipSelectScene();
            _Scene.OmegaButton.RegisterListener(this);
            _Scene.Active = true;
            _Scene.RegisterListener(this);
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
            _Selected = true;
            _IsComplete = true;
        }

        #region Implementation of IBackButtonListener

        /// <summary> Invoked when the back button is pressed </summary>
        public void OnBackPressed()
        {
            _Scene.OmegaButton.DeregisterListener(this);
            _Returning = true;
            _IsComplete = true;
        }

        #endregion

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
            if (_Selected && _IsComplete)
            {
                _Music.Stop();
                ChangeState(new PlayingState(_Selection));
            }

            if (_Returning && _IsComplete)
            {
                ChangeState(new ShipSelectState(_Music));
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
