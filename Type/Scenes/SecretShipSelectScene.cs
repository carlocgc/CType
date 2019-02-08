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
    public class SecretShipSelectScene : Scene, INotifier<IBackButtonListener>
    {
        private readonly List<IBackButtonListener> _BackButtonListeners = new List<IBackButtonListener>();

        private readonly TextDisplay _Title;

        private readonly Sprite _Background;

        private readonly Button _BackButton;

        private Boolean _Active;

        public ShipSelectButton OmegaButton { get; }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                OmegaButton.Active = _Active;
            }
        }

        public SecretShipSelectScene()
        {
            _Title = new TextDisplay(Game.MainCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "SECRET SHIP",
                Position = new Vector2(0, 450),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;
            _Background = new Sprite(Game.UiCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Background/stars-2.png"))
            {
                Position = new Vector2(0, 0),
                Visible = true,
            };
            _Background.Offset = _Background.Size / 2;

            Sprite backButton = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Buttons/exitbutton.png"))
            {
                Position = new Vector2(770, 375),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 1f),
                Scale = new Vector2(0.8f, 0.8f)
            };
            _BackButton = new Button(Int32.MaxValue, backButton) { OnButtonPress = BackButtonOnPress };
            _BackButton.TouchEnabled = true;
            _BackButton.Visible = true;

            OmegaButton = new ShipSelectButton(3, new Vector2(658, 50), "Content/Graphics/Player/player_omega.png", "OMEGA", 1, 200, 120);
        }

        private void BackButtonOnPress(Button obj)
        {
            foreach (IBackButtonListener listener in _BackButtonListeners)
            {
                listener.OnBackPressed();
            }
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        #region Implementation of INotifier<in IBackButtonListener>

        /// <summary>
        /// Add a listener
        /// </summary>
        public void RegisterListener(IBackButtonListener listener)
        {
            if (!_BackButtonListeners.Contains(listener)) _BackButtonListeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener
        /// </summary>
        public void DeregisterListener(IBackButtonListener listener)
        {
            if (_BackButtonListeners.Contains(listener)) _BackButtonListeners.Remove(listener);
        }

        #endregion

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _BackButtonListeners.Clear();
            _Title.Dispose();
            _Background.Dispose();
            _BackButton.Dispose();
            OmegaButton.Dispose();
        }
    }
}
