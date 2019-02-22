using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Buttons;
using Type.Data;
using Type.Services;
using Type.UI;

namespace Type.Scenes
{
    public class UIScene : Scene
    {
        /// <summary> UI element that displays the current FPS </summary>
        private readonly FpsCounter _FrameCounter;
        /// <summary> Displays info about powerups in the pause menu </summary>
        private readonly PowerUpHelp _Help;
        /// <summary> Virtual analog stick </summary>
        private readonly VirtualAnalogStick _VirtualAnalogStick;
        /// <summary> Virtual fire button </summary>
        private readonly VirtualButton _FireButton;
        /// <summary> Virtual nuke button </summary>
        private readonly VirtualButton _NukeButton;
        /// <summary> Virtual pause button </summary>
        private readonly VirtualButton _PauseButton;
        /// <summary> Virtual resume button</summary>
        private readonly VirtualButton _ResumeButton;
        /// <summary> Whether the buttons in the scene are active </summary>
        private Boolean _Active;

        /// <summary> Shows that the game is paused </summary>
        public Sprite PauseIndicator { get; }

        /// <summary> Text printer that displays the score </summary>
        public TextDisplay ScoreDisplay { get; }

        /// <summary> UI element that displays the amount oif lives remaining </summary>
        public LifeMeter LifeMeter { get; }

        /// <summary> Object that shows the current level text </summary>
        public LevelDisplay LevelDisplay { get; }

        /// <summary> Nuke button element of the UI, displays nuke count </summary>
        public NukeDisplay NukeDisplay { get; }

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
            NukeDisplay = new NukeDisplay();
            PauseIndicator = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Buttons/paused.png"))
            {
                Position = new Vector2(0, 0),
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            PauseIndicator.Offset = PauseIndicator.Size / 2;

#if __ANDROID__
            Sprite fireButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/fire.png"))
            {
                Position = new Vector2(625, -450),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _FireButton = new VirtualButton(fireButton.ZOrder, fireButton, ButtonData.Type.FIRE);
            InputService.Instance.RegisterButton(_FireButton);

            Sprite nukeButton = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/nuke_button.png"))
            {
                Position = new Vector2(425, -450),
                Visible = true,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _NukeButton = new VirtualButton(Int32.MaxValue, nukeButton, ButtonData.Type.NUKE);
            InputService.Instance.RegisterButton(_NukeButton);

            Sprite pauseButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/pausebutton.png"))
            {
                Position = new Vector2(770, 350),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _PauseButton = new VirtualButton(Int32.MaxValue, pauseButton, ButtonData.Type.PAUSE);
            InputService.Instance.RegisterButton(_PauseButton);

            Sprite resumeButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/playbutton.png"))
            {
                Position = new Vector2(770, 350),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _ResumeButton = new VirtualButton(Int32.MaxValue, resumeButton, ButtonData.Type.RESUME);
            InputService.Instance.RegisterButton(_ResumeButton);

            _VirtualAnalogStick = new VirtualAnalogStick(new Vector2(-620, -220), 110);
            InputService.Instance.VirtualAnalogStick = _VirtualAnalogStick;
#endif
        }

        /// <summary>
        /// Sets the state of the UI
        /// </summary>
        /// <param name="state"></param>
        public void ShowOnScreenControls(Boolean state)
        {
#if __ANDROID__
            _FireButton.TouchEnabled = state;
            _FireButton.Visible = state;
            _NukeButton.TouchEnabled = state;
            _NukeButton.Visible = state;
            _PauseButton.TouchEnabled = state;
            _PauseButton.Visible = state;

            _VirtualAnalogStick.TouchEnabled = state;
            _VirtualAnalogStick.Visible = state;
            _VirtualAnalogStick.ListeningForMove = state;

            if (state) return;
            _ResumeButton.TouchEnabled = false;
            _ResumeButton.Visible = false;
#endif
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Help.Dispose();
            _FrameCounter.Dispose();
            NukeDisplay.Dispose();
            LevelDisplay.Dispose();
            LifeMeter.Dispose();
            ScoreDisplay.Dispose();
            PauseIndicator.Dispose();
#if __ANDROID__
            _VirtualAnalogStick.Dispose();
            _FireButton.Dispose();
            _NukeButton.Dispose();
            _PauseButton.Dispose();
            _ResumeButton.Dispose();
#endif
        }
    }
}
