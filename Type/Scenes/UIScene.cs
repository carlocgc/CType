using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Base;
using Type.Buttons;
using Type.Controllers;
using Type.UI;

namespace Type.Scenes
{
    public class UIScene : Scene
    {

        /// <summary> UI element that displays the current FPS </summary>
        private readonly FpsCounter _FrameCounter;
        /// <summary> Shows that the game is paused </summary>
        private readonly Sprite _PauseIndicator;
        /// <summary> Displays info about powerups in the pause menu </summary>
        private readonly PowerUpHelp _Help;
        /// <summary> Virtual analog stick </summary>
        private readonly VirtualAnalogStick _VirtualAnalogStick;
        /// <summary> Virtual fire button </summary>
        private readonly FireButton _FireButton;
        /// <summary> Virtual pause button </summary>
        private readonly PauseButton _PauseButton;
        /// <summary> Virtual resume button</summary>
        private readonly ResumeButton _ResumeButton;

        private Boolean _Active;

        /// <summary> Text printer that displays the score </summary>
        public TextDisplay ScoreDisplay { get; private set; }

        /// <summary> UI element that displays the amount oif lives remaining </summary>
        public LifeMeter LifeMeter { get; private set; }

        /// <summary> Object that shows the current level text </summary>
        public LevelDisplay LevelDisplay { get; private set; }

        /// <summary> Nuke button element of the UI, displays nuke count </summary>
        public NukeButton NukeButton { get; set; }

        /// <summary>
        /// Whether the UI is Active
        /// </summary>
        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                Visible = value;
                SetState(_Active);
            }
        }

        public UIScene(Int32 playertype)
        {
            ScoreDisplay = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "0",
                Position = new Vector2(-900, 460),
                Visible = true,
                Scale = new Vector2(2, 2),
            };
            AddDrawable(ScoreDisplay);

            _Help = new PowerUpHelp();
            _FrameCounter = new FpsCounter();
            LifeMeter = new LifeMeter(playertype);
            LevelDisplay = new LevelDisplay();

            NukeButton = new NukeButton();

#if __ANDROID__
            _FireButton = new FireButton();
            _PauseButton = new PauseButton();
            _ResumeButton = new ResumeButton();
            _VirtualAnalogStick = new VirtualAnalogStick(new Vector2(-620, -220), 110);

            InputManager.Instance.NukeButton = NukeButton;
            InputManager.Instance.FireButton = _FireButton;
            InputManager.Instance.PauseButton = _PauseButton;
            InputManager.Instance.ResumeButton = _ResumeButton;
            InputManager.Instance.VirtualAnalogStick = _VirtualAnalogStick;
#endif
        }

        /// <summary>
        /// Sets the state of the UI
        /// </summary>
        /// <param name="state"></param>
        private void SetState(Boolean state)
        {
#if __ANDROID__
            _FireButton.Active = state;
            _FireButton.Visible = state;
            _PauseButton.Active = state;
            _PauseButton.Visible = state;
            _VirtualAnalogStick.TouchEnabled = state;
            _VirtualAnalogStick.Visible = state;
            _VirtualAnalogStick.ListeningForMove = state;

            if (!state)
            {
                _ResumeButton.Active = false;
                _ResumeButton.Visible = false;
            }
#endif
            NukeButton.Visible = state;
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();

            InputManager.Instance.NukeButton = null;
            InputManager.Instance.FireButton = null;
            InputManager.Instance.PauseButton = null;
            InputManager.Instance.ResumeButton = null;
            InputManager.Instance.VirtualAnalogStick = null;

            NukeButton?.Dispose();
            _VirtualAnalogStick.Dispose();
            _PauseButton?.Dispose();
            _ResumeButton?.Dispose();
            _FireButton?.Dispose();

            _Help.Dispose();
            _FrameCounter.Dispose();
            LevelDisplay.Dispose();
            LifeMeter.Dispose();
            ScoreDisplay.Dispose();
        }
    }
}
