using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using AmosShared.Touch;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Interfaces;
using Type.Interfaces.Control;

namespace Type.Buttons
{
    public class ShipSelectButton : IPositionable, IUpdatable, INotifier<IShipSelectListener>, IFocusable
    {
        /// <summary> Name shown in place of a locked craft's own </summary>
        /// <remarks>
        /// This and the two hint lines are spelled out of the letters, digits, colon, dot, percent
        /// and space that <see cref="Constants.Font.Map"/> covers. The font atlas has no question
        /// mark, and <see cref="TextDisplay"/> throws on a character it cannot map.
        /// </remarks>
        private const String LockedName = "LOCKED";
        /// <summary> First line of the hint shown where a locked craft's statistics would be </summary>
        private const String UnlockHintFirstLine = "COMPLETE THE";
        /// <summary> Second line of that hint </summary>
        /// <remarks>
        /// The longer of the two at fourteen characters. A glyph advances 15 units, so at the
        /// statistics' scale of 2 the line is 420 units wide and clears the card's 600 unit width
        /// with its borders. A longer line would run over them.
        /// </remarks>
        private const String UnlockHintSecondLine = "GAME TO UNLOCK";

        /// <summary>
        /// Colour a locked craft is drawn in, dark enough to read as a silhouette but still
        /// lighter than the card behind it
        /// </summary>
        private static readonly Vector4 LockedShipColour = new Vector4(0.25f, 0.25f, 0.28f, 1);

        private readonly Button _Button;

        private readonly Sprite _Ship;

        private readonly TextDisplay _ShipName;

        private readonly TextDisplay _HitpointsLabel;

        private readonly TextDisplay _HitpointsValue;

        private readonly TextDisplay _FirerateLabel;

        private readonly TextDisplay _FirerateValue;

        private readonly TextDisplay _EngineSpeedLabel;

        private readonly TextDisplay _EngineSpeedValue;

        /// <summary> First line of the hint telling the player how to earn a locked craft </summary>
        private readonly TextDisplay _UnlockHintFirst;

        /// <summary> Second line of that hint </summary>
        private readonly TextDisplay _UnlockHintSecond;

        private readonly Int32 _ID;

        /// <summary> Whether the craft has yet to be earned, and so cannot be chosen </summary>
        private readonly Boolean _Locked;

        private Boolean _Active;

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        /// <inheritdoc />
        public Vector2 Position { get; set; }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                _Button.TouchEnabled = _Active && !_Locked;
            }
        }

        /// <summary>
        /// Creates a card showing one craft and its statistics
        /// </summary>
        /// <param name="id"> Identifies the craft to listeners and to the game scene </param>
        /// <param name="position"> Bottom left of the card, in screen space </param>
        /// <param name="scale"> How much the card and its contents are shrunk, so that the row
        /// of cards fits across the screen </param>
        /// <param name="shipPath"> Content path of the craft's sprite </param>
        /// <param name="name"> The craft's name </param>
        /// <param name="hitPoints"> Hit points shown on the card </param>
        /// <param name="fireRate"> Fire rate shown on the card </param>
        /// <param name="speed"> Engine speed shown on the card </param>
        /// <param name="locked"> Whether the craft has yet to be earned; a locked card gives
        /// nothing about the craft away and cannot be chosen </param>
        public ShipSelectButton(Int32 id, Vector2 position, Single scale, String shipPath, String name, Int32 hitPoints, Int32 fireRate, Int32 speed, Boolean locked)
        {
            _ID = id;
            _Locked = locked;

            Sprite buttonSprite = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/ship_select_button.png"))
            {
                Position = position - Renderer.Instance.TargetDimensions / 2,
            };
            // Resized rather than scaled: the engine tests touches against the sprite's width and
            // height, so a scaled card would keep the hit area of a full sized one.
            buttonSprite.Size = buttonSprite.Size * scale;

            _Button = new Button(Constants.ZOrders.UI, buttonSprite)
            {
                Visible = true,
            };
            _Button.OnButtonRelease += OnButtonRelease;

            _Ship = new Sprite(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture(shipPath))
            {
                Position = _Button.Position + new Vector2(303, 550) * scale,
                Scale = new Vector2(3, 3) * scale,
                Visible = true,
            };
            _Ship.Offset = new Vector2(_Ship.Size.X / 2 * _Ship.Scale.X, _Ship.Size.Y / 2 * _Ship.Scale.Y);
            _Ship.RotationOrigin = _Ship.Size / 2;
            _Ship.Rotation = 1.57;

            _ShipName = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = _Locked ? LockedName : $"{name}",
                Position = _Button.Position + new Vector2(250, 800) * scale,
                Visible = true,
                Scale = new Vector2(2.5f, 2.5f) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _ShipName.Offset = _ShipName.Size / 2;

            _HitpointsLabel = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"HIT POINTS:",
                Position = _Button.Position + new Vector2(60, 300) * scale,
                Visible = !_Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _HitpointsValue = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{hitPoints}",
                Position = _Button.Position + new Vector2(390, 300) * scale,
                Visible = !_Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _FirerateLabel = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"FIRE RATE:",
                Position = _Button.Position + new Vector2(60, 200) * scale,
                Visible = !_Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _FirerateValue = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{fireRate}",
                Position = _Button.Position + new Vector2(390, 200) * scale,
                Visible = !_Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _EngineSpeedLabel = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"SPEED:",
                Position = _Button.Position + new Vector2(60, 100) * scale,
                Visible = !_Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _EngineSpeedValue = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{speed}",
                Position = _Button.Position + new Vector2(390, 100) * scale,
                Visible = !_Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };

            // Stands in the space the statistics leave, which on a locked card would only have
            // shown that everything about the craft is being withheld.
            _UnlockHintFirst = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = UnlockHintFirstLine,
                Position = _Button.Position + new Vector2(300, 250) * scale,
                Visible = _Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _UnlockHintFirst.Offset = new Vector2(_UnlockHintFirst.Size.X * _UnlockHintFirst.Scale.X, _UnlockHintFirst.Size.Y * _UnlockHintFirst.Scale.Y) / 2;

            _UnlockHintSecond = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = UnlockHintSecondLine,
                Position = _Button.Position + new Vector2(300, 150) * scale,
                Visible = _Locked,
                Scale = new Vector2(2, 2) * scale,
                Colour = new Vector4(1, 1, 1, 1)
            };
            _UnlockHintSecond.Offset = new Vector2(_UnlockHintSecond.Size.X * _UnlockHintSecond.Scale.X, _UnlockHintSecond.Size.Y * _UnlockHintSecond.Scale.Y) / 2;

            // A locked card is never focused, so without this it would keep the appearance it was
            // built with and read as the selected one.
            if (_Locked) SetFocused(false);
        }


        #region Implementation of IFocusable

        /// <inheritdoc />
        /// <remarks>
        /// A craft that has not been earned stays on the row so the player can see there is one
        /// left to find, but the focus cursor passes straight over it.
        /// </remarks>
        public Boolean CanFocus => Active && !_Locked;

        /// <inheritdoc />
        /// <remarks>
        /// The whole card dims rather than only the button, so the focused craft reads as the
        /// selected one at a glance from a desktop viewing distance.
        /// </remarks>
        public void SetFocused(Boolean focused)
        {
            Vector4 tint = focused ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 1);

            _Button.Sprite.Colour = tint;
            _Ship.Colour = _Locked ? LockedShipColour : tint;
            _ShipName.Colour = tint;
            _HitpointsLabel.Colour = tint;
            _HitpointsValue.Colour = tint;
            _FirerateLabel.Colour = tint;
            _FirerateValue.Colour = tint;
            _EngineSpeedLabel.Colour = tint;
            _EngineSpeedValue.Colour = tint;
            _UnlockHintFirst.Colour = tint;
            _UnlockHintSecond.Colour = tint;
        }

        /// <inheritdoc />
        public void Activate()
        {
            for (Int32 index = _Listeners.Count - 1; index >= 0; index--)
            {
                _Listeners[index].OnButtonReleased(_ID);
            }
        }

        #endregion

        private void OnButtonRelease(Button button)
        {
            for (var index = _Listeners.Count - 1; index >= 0; index--)
            {
                IShipSelectListener listener = _Listeners[index];
                listener.OnButtonReleased(_ID);
            }
        }

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Button.Dispose();
            _Ship.Dispose();
            _ShipName.Dispose();
            _HitpointsLabel.Dispose();
            _HitpointsValue.Dispose();
            _FirerateLabel.Dispose();
            _FirerateValue.Dispose();
            _EngineSpeedLabel.Dispose();
            _EngineSpeedValue.Dispose();
            _UnlockHintFirst.Dispose();
            _UnlockHintSecond.Dispose();
        }

        private readonly List<IShipSelectListener> _Listeners = new List<IShipSelectListener>();

        /// <inheritdoc />
        public void RegisterListener(IShipSelectListener listener)
        {
            _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IShipSelectListener listener)
        {
            _Listeners.Remove(listener);
        }
    }
}
