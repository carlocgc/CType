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

        /// <summary> The controls screen when it is open, null when it is not </summary>
        private ControlsScene _ControlsScene;

        /// <summary> Menu music, carried through so it keeps playing across the screen </summary>
        private AudioPlayer _Music;

        /// <summary> Moves focus between the settings and adjusts the focused one </summary>
        private MenuNavigator _Navigator;

        /// <summary> Moves focus between the bindings while the controls screen is open </summary>
        private MenuNavigator _ControlsNavigator;

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
            _Scene = new OptionsScene(ShowControls) { Visible = true };

            FocusOptions();
        }

        /// <summary>
        /// Gives the settings the focus cursor
        /// </summary>
        private void FocusOptions()
        {
            _Navigator = new MenuNavigator { OnCancel = () => _IsComplete = true };
            foreach (OptionRow row in _Scene.Rows) _Navigator.Add(row);
            _Navigator.Add(_Scene.ControlsItem);
            _Navigator.FocusFirst();
        }

        /// <summary>
        /// Opens the controls screen over the settings, which stay on screen behind it
        /// </summary>
        /// <remarks>
        /// Built as an overlay so the menu art is not drawn twice, and the settings themselves
        /// are left in place: the controls screen covers the part of the screen they occupy.
        /// </remarks>
        private void ShowControls()
        {
            if (_ControlsScene != null) return;

            _Navigator?.Dispose();
            _Navigator = null;

            _Scene.SetSettingsVisible(false);
            _ControlsScene = new ControlsScene(overlay: true) { Visible = true };

            _ControlsNavigator = new MenuNavigator { OnCancel = CloseControls };
            foreach (BindingRow row in _ControlsScene.Rows) _ControlsNavigator.Add(row);
            _ControlsNavigator.Add(_ControlsScene.ResetItem);
            _ControlsNavigator.FocusFirst();
        }

        /// <summary>
        /// Closes the controls screen and returns focus to the settings
        /// </summary>
        private void CloseControls()
        {
            if (_ControlsScene == null) return;

            _ControlsNavigator?.Dispose();
            _ControlsNavigator = null;
            _ControlsScene.Dispose();
            _ControlsScene = null;

            _Scene.SetSettingsVisible(true);
            FocusOptions();
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
            _ControlsNavigator?.Dispose();
            _ControlsNavigator = null;
            _ControlsScene?.Dispose();
            _ControlsScene = null;
            _Navigator?.Dispose();
            _Navigator = null;
            _Music = null;
            _Scene.Dispose();
            _Scene = null;
        }
    }
}
