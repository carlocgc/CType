using AmosShared.Audio;
using AmosShared.State;
using System;
using Type.Scenes;
using Type.UI.Navigation;

namespace Type.States
{
    /// <summary>
    /// Shows the options screen and returns to the main menu when the player backs out
    /// </summary>
    public class OptionsState : State
    {
        /// <summary> The scene being shown </summary>
        private OptionsScene _Scene;

        /// <summary> Menu music, carried through so it keeps playing across the screen </summary>
        private AudioPlayer _Music;

        /// <summary> Moves focus between the settings and adjusts the focused one </summary>
        private MenuNavigator _Navigator;

        /// <summary> Whether the player has asked to leave </summary>
        private Boolean _IsComplete;

        /// <summary>
        /// Creates the state
        /// </summary>
        /// <param name="music"> Menu music to keep playing and hand back on exit </param>
        public OptionsState(AudioPlayer music)
        {
            _Music = music;
        }

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _Scene = new OptionsScene { Visible = true };

            _Navigator = new MenuNavigator { OnCancel = () => _IsComplete = true };
            foreach (OptionRow row in _Scene.Rows) _Navigator.Add(row);
            _Navigator.FocusFirst();
        }

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
            if (_IsComplete) ChangeState(new MainMenuState(_Music));
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
            _Navigator?.Dispose();
            _Navigator = null;
            _Music = null;
            _Scene.Dispose();
            _Scene = null;
        }
    }
}
