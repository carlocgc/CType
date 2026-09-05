using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Buttons;
using Type.Interfaces;
using Type.Interfaces.Control;
using Type.Data;
using Type.UI;

namespace Type.Scenes
{
    public class ShipSelectScene : Scene, INotifier<IBackButtonListener>
    {
        /// <summary> Width of a card at its full size </summary>
        private const Single CardSourceWidth = 600;
        /// <summary> Width of the screen the layout is designed against </summary>
        private const Single LayoutWidth = 1920;
        /// <summary> How many craft are shown </summary>
        private const Int32 CardCount = 4;

        /// <summary>
        /// How much each card is shrunk. Four cards at full size are wider than the screen, and
        /// even three left no room above the input prompts.
        /// </summary>
        private const Single CardScale = 0.72f;

        /// <summary> Width of a card once shrunk </summary>
        private const Single CardWidth = CardSourceWidth * CardScale;
        /// <summary> Gap between neighbouring cards, and between the row and each screen edge </summary>
        private const Single CardGap = (LayoutWidth - CardCount * CardWidth) / (CardCount + 1);
        /// <summary> Distance between the left edges of neighbouring cards </summary>
        private const Single CardStride = CardWidth + CardGap;
        /// <summary> Bottom edge of the row, high enough to clear the input prompts below it </summary>
        private const Single CardBottom = 210;

        private readonly List<IBackButtonListener> _BackButtonListeners = new List<IBackButtonListener>();

        private Boolean _Active;

        private readonly TextDisplay _Title;

        private readonly Sprite _Background;

        /// <summary> Tells the player how to choose a craft </summary>
        private readonly InputPrompt _SelectPrompt;
        /// <summary> Tells the player how to leave the screen </summary>
        private readonly InputPrompt _BackPrompt;

#if __ANDROID__
        /// <summary> On screen back control. Touch only: a desktop leaves by CANCEL, which the
        /// back prompt names, so the button would be an unexplained third way out </summary>
        private readonly Button _BackButton;
#endif // #if __ANDROID__

        public ShipSelectButton AlphaButton { get; }

        public ShipSelectButton BetaButton { get; }

        public ShipSelectButton GammaButton { get; }

        /// <summary> The hidden craft, shown locked until the game has been completed once </summary>
        public ShipSelectButton OmegaButton { get; }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                AlphaButton.Active = _Active;
                BetaButton.Active = _Active;
                GammaButton.Active = _Active;
                OmegaButton.Active = _Active;
            }
        }

        public ShipSelectScene()
        {
            _Title = new TextDisplay(Game.MainCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "SHIP SELECT",
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

            _SelectPrompt = new InputPrompt(ButtonData.Type.CONFIRM, "SELECT", new Vector2(-880, -480));
            _BackPrompt = new InputPrompt(ButtonData.Type.CANCEL, "BACK", new Vector2(-880, -530));

            AlphaButton = new ShipSelectButton(0, CardPosition(0), CardScale, "Content/Graphics/Player/player-alpha.png", "ALPHA", 1, 100, 100, false);
            BetaButton = new ShipSelectButton(1, CardPosition(1), CardScale, "Content/Graphics/Player/player-beta.png", "BETA", 2, 80, 80, false);
            GammaButton = new ShipSelectButton(2, CardPosition(2), CardScale, "Content/Graphics/Player/player-gamma.png", "GAMMA", 3, 60, 60, false);
            OmegaButton = new ShipSelectButton(3, CardPosition(3), CardScale, "Content/Graphics/Player/player_omega.png", "OMEGA", 1, 200, 120, !Progress.GameCompleted);

#if __ANDROID__
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
#endif // #if __ANDROID__
        }

        /// <summary>
        /// Bottom left of the card at the given place in the row
        /// </summary>
        /// <param name="index"> Place in the row, counted from the left </param>
        private static Vector2 CardPosition(Int32 index)
        {
            return new Vector2(CardGap + index * CardStride, CardBottom);
        }

#if __ANDROID__
        /// <summary>
        /// Leaves the screen when the on screen back control is touched
        /// </summary>
        /// <param name="obj"> The control that was touched </param>
        private void BackButtonOnPress(Button obj)
        {
            BackPressed();
        }
#endif // #if __ANDROID__

        public void BackPressed()
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
            _SelectPrompt.Dispose();
            _BackPrompt.Dispose();
#if __ANDROID__
            _BackButton.Dispose();
#endif // #if __ANDROID__
            AlphaButton.Dispose();
            BetaButton.Dispose();
            GammaButton.Dispose();
            OmegaButton.Dispose();
        }

    }
}
