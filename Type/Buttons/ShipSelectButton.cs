﻿using AmosShared.Base;
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
    public class ShipSelectButton : IPositionable, IUpdatable, INotifier<IShipSelectListener>
    {
        private readonly Button _Button;

        private readonly Sprite _Ship;

        private readonly TextDisplay _ShipName;

        private readonly TextDisplay _HitpointsLabel;

        private readonly TextDisplay _HitpointsValue;

        private readonly TextDisplay _FirerateLabel;

        private readonly TextDisplay _FirerateValue;

        private readonly TextDisplay _EngineSpeedLabel;

        private readonly TextDisplay _EngineSpeedValue;

        private readonly Int32 _ID;

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
                _Button.TouchEnabled = _Active;
            }
        }

        public ShipSelectButton(Int32 id, Vector2 position, String shipPath, String name, Int32 hitPoints, Int32 fireRate, Int32 speed)
        {
            _ID = id;

            Sprite buttonSprite = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/ship_select_button.png"))
            {
                Position = position - Renderer.Instance.TargetDimensions / 2,
            };
            _Button = new Button(Constants.ZOrders.UI, buttonSprite)
            {
                Visible = true,
            };
            _Button.OnButtonPress += OnButtonPress;
            _Button.OnButtonRelease += OnButtonRelease;

            _Ship = new Sprite(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture(shipPath))
            {
                Position = _Button.Position + new Vector2(303, 550),
                Scale = new Vector2(3, 3),
                Visible = true,
            };
            _Ship.Offset = new Vector2(_Ship.Size.X / 2 * _Ship.Scale.X, _Ship.Size.Y / 2 * _Ship.Scale.Y);
            _Ship.RotationOrigin = _Ship.Size / 2;
            _Ship.Rotation = 1.57;

            _ShipName = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{name}",
                Position = _Button.Position + new Vector2(250, 800),
                Visible = true,
                Scale = new Vector2(2.5f, 2.5f),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _ShipName.Offset = _ShipName.Size / 2;

            _HitpointsLabel = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"HIT POINTS:",
                Position = _Button.Position + new Vector2(60, 300),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _HitpointsValue = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{hitPoints}",
                Position = _Button.Position + new Vector2(390, 300),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _FirerateLabel = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"FIRE RATE:",
                Position = _Button.Position + new Vector2(60, 200),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _FirerateValue = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{fireRate}",
                Position = _Button.Position + new Vector2(390, 200),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _EngineSpeedLabel = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"SPEED:",
                Position = _Button.Position + new Vector2(60, 100),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _EngineSpeedValue = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{speed}",
                Position = _Button.Position + new Vector2(390, 100),
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = new Vector4(1, 1, 1, 1)
            };

        }


        private void OnButtonPress(Button button)
        {
            for (var index = _Listeners.Count - 1; index >= 0; index--)
            {
                IShipSelectListener listener = _Listeners[index];
                listener.OnButtonPressed(_ID);
            }
        }


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
