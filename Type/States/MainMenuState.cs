using AmosShared.Audio;
using AmosShared.State;
using System;
using OpenTK;
using Type.Data;
using Type.Interfaces.Control;
using Type.Scenes;
using Type.Services;

namespace Type.States
{
    public class MainMenuState : State, IInputListener
    {
        private MainMenuScene _Scene;

        /// <summary> Background music </summary>
        private AudioPlayer _Music;

        public MainMenuState(AudioPlayer music = null)
        {
            _Music = music ?? new AudioPlayer("Content/Audio/mainMenuBgm.wav", true, AudioManager.Category.MUSIC, 1);
        }

        protected override void OnEnter()
        {
            _Scene = new MainMenuScene { Visible = true };
            _Scene.Show();

            InputService.Instance.RegisterListener(this);

            if (!GameStats.Instance.CanShowAds || !AdService.Instance.IsLoaded) return;
            AdService.Instance.OnAddClosed = () => { GameStats.Instance.CanShowAds = false; };
            AdService.Instance.ShowInterstitial();
        }

        public override Boolean IsComplete()
        {
            if (_Scene.IsComplete) ChangeState(new ShipSelectState(_Music));
            return _Scene.IsComplete;
        }

        protected override void OnExit()
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            InputService.Instance.DeregisterListener(this);
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
                case ButtonData.Type.START:
                    {
                        if (data.State != ButtonData.State.PRESSED) return;
                        _Scene.StartGame();
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
