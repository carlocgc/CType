using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using System.Collections.Generic;
using AmosShared.Audio;
using Type.Base;
using Type.Interfaces;
using Type.Interfaces.Control;
using Type.UI;

namespace Type.Scenes
{
    public class UIScene : Scene, INotifier<IUIListener>
    {
        private Boolean _Active;

        /// <summary> Fire button </summary>
        private readonly Button _FireButton;
        /// <summary> Pause button </summary>
        private readonly Button _PauseButton;
        /// <summary> Resume button, used to unpause the game </summary>
        private readonly Button _ResumeButton;
        /// <summary> UI element that displays the current FPS </summary>
        private readonly FpsCounter _FrameCounter;
        /// <summary> Shows that the game is paused </summary>
        private readonly Sprite _PauseIndicator;
        /// <summary> Callback to flash the paused indicator while paused </summary>
        private TimedCallback _IndicatorCallback;
        /// <summary> Displays info about powerups in the pause menu </summary>
        private PowerUpHelp _Help;

        /// <summary> Floating analog stick </summary>
        public AnalogStick AnalogStick { get; private set; }
        /// <summary> Text printer that displays the score </summary>
        public TextDisplay ScoreDisplay { get; private set; }
        /// <summary> UI element that displays the amount oif lives remaining </summary>
        public LifeMeter LifeMeter { get; private set; }
        /// <summary> Object that shows the current level text </summary>
        public LevelDisplay LevelDisplay { get; private set; }
        /// <summary> Nuke button element of the UI, displays nuke count and informs listeners a nuke should be detonated </summary>
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

            Sprite fireButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/fire.png"))
            {
                Position = new Vector2(625, -450),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _FireButton = new Button(Int32.MaxValue, fireButton) { OnButtonPress = FireButtonPress, OnButtonRelease = FireButtonRelease };

            Sprite pauseButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/pausebutton.png"))
            {
                Position = new Vector2(770, 350),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _PauseButton = new Button(Int32.MaxValue, pauseButton) { OnButtonPress = PauseButtonOnPress };

            Sprite resumeButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/playbutton.png"))
            {
                Position = new Vector2(770, 350),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _ResumeButton = new Button(Int32.MaxValue, resumeButton) { OnButtonPress = ResumeButtonOnPress };

            _PauseIndicator = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Buttons/paused.png"))
            {
                Position = new Vector2(0, 0),
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _PauseIndicator.Offset = _PauseIndicator.Size / 2;

            _Help = new PowerUpHelp();
            _FrameCounter = new FpsCounter();
            LifeMeter = new LifeMeter(playertype);
            NukeButton = new NukeButton();
            LevelDisplay = new LevelDisplay();
            AnalogStick = new AnalogStick(new Vector2(-620, -220), 110);
        }

        /// <summary>
        /// Sets the state of the UI
        /// </summary>
        /// <param name="state"></param>
        private void SetState(Boolean state)
        {
            _FireButton.TouchEnabled = state;
            _FireButton.Visible = state;
            _PauseButton.TouchEnabled = state;
            _PauseButton.Visible = state;
            NukeButton.Visible = state;
            AnalogStick.TouchEnabled = state;
            AnalogStick.Visible = state;
            AnalogStick.ListeningForMove = state;

        }

        #region Inputs

        private void FireButtonRelease(Button obj)
        {
            foreach (IUIListener listener in _Listeners)
            {
                listener.FireButtonReleased();
            }
        }

        private void FireButtonPress(Button obj)
        {
            foreach (IUIListener listener in _Listeners)
            {
                listener.FireButtonPressed();
            }
        }

        private void PauseButtonOnPress(Button obj)
        {
            Game.GameTime.Multiplier = 0;
            SetState(false);
            _Help.Show();
            AudioManager.Instance.MusicVolume = 0;
            AudioManager.Instance.EffectVolume = 0;
            _PauseIndicator.Visible = true;
            _ResumeButton.TouchEnabled = true;
            _ResumeButton.Visible = true;
        }

        private void ResumeButtonOnPress(Button obj)
        {
            Game.GameTime.Multiplier = 1;
            SetState(true);
            _Help.Hide();
            AudioManager.Instance.MusicVolume = 1;
            AudioManager.Instance.EffectVolume = 1;
            _PauseIndicator.Visible = false;
            _ResumeButton.TouchEnabled = false;
            _ResumeButton.Visible = false;
        }

        #endregion

        #region Listener

        private readonly List<IUIListener> _Listeners = new List<IUIListener>();

        /// <inheritdoc />
        public void RegisterListener(IUIListener listener)
        {
            if (_Listeners.Contains(listener)) return;
            _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IUIListener listener)
        {
            if (!_Listeners.Contains(listener)) return;
            _Listeners.Remove(listener);
        }

        #endregion

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Listeners.Clear();
            _Help.Dispose();
            _FireButton.Dispose();
            _PauseButton.Dispose();
            _ResumeButton.Dispose();
            _FrameCounter.Dispose();
            LevelDisplay.Dispose();
            AnalogStick.Dispose();
            LifeMeter.Dispose();
            ScoreDisplay.Dispose();
        }
    }
}
