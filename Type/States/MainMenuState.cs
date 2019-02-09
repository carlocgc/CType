using AmosShared.State;
using System;
using AmosShared.Audio;
using AmosShared.Competitive;
using Type.Data;
using Type.Scenes;

namespace Type.States
{
    public class MainMenuState : State
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
            CompetitiveManager.Instance.Connect();
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
            _Music = null;
            _Scene.Dispose();
            _Scene = null;
        }
    }
}
