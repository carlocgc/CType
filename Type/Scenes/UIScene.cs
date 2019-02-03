using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Interfaces;
using Type.Interfaces.Control;
using Type.UI;

namespace Type.Scenes
{
    public class UIScene : Scene, INotifier<IUIListener>
    {
        private Boolean _Active;
        /// <summary> Fire button </summary>
        public Button FireButton { get; private set; }
        /// <summary> Floating analog stick </summary>
        public AnalogStick AnalogStick { get; private set; }
        /// <summary> Text printer that displays the score </summary>
        public TextDisplay ScoreDisplay { get; private set; }
        /// <summary> UI element that displays the amount oif lives remaining </summary>
        public LifeMeter LifeMeter { get; private set; }
        /// <summary> Object that shows the current level text </summary>
        public LevelDisplay LevelDisplay { get; private set; }
        /// <summary> UI element that displays the current FPS </summary>
        public FpsCounter FrameCounter { get; private set; }

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
                Visible = true,
                Colour = new Vector4(1, 1, 1, (Single)0.5)
            };
            FireButton = new Button(Int32.MaxValue, fireButton) { OnButtonPress = FireButtonPress, OnButtonRelease = FireButtonRelease };

            LifeMeter = new LifeMeter(playertype);
            FrameCounter = new FpsCounter();
            LevelDisplay = new LevelDisplay();
            AnalogStick = new AnalogStick(new Vector2(-620, -220), 110);
        }

        private void SetState(Boolean state)
        {
            FireButton.TouchEnabled = state;
            FireButton.Visible = state;
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
            AnalogStick.Dispose();
            FireButton.Dispose();
            LevelDisplay.Dispose();
            FrameCounter.Dispose();
            LifeMeter.Dispose();
            ScoreDisplay.Dispose();
        }
    }
}
