using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using AmosShared.Touch;
using OpenTK.Audio;
using Type.Base;
using Type.Interfaces.Control;

namespace Type.UI
{
    public class AnalogStick : GameObject, ITouchListener
    {
        private readonly Sprite _Base;

        private readonly Sprite _Top;

        private Int32 _PressId;

        private Vector2 _Direction;

        private List<IAnalogListener> _Listeners;

        private Vector4 _HitBox;

        private Boolean _Visible;

        public Boolean TouchEnabled { get; set; }

        public Int32 TouchOrder { get; set; }

        public Boolean TouchOrderChanged { get; set; }

        public Boolean ListeningForMove { get; set; }

        public Boolean Visible
        {
            get => _Visible;
            set
            {
                _Visible = value;
                _Base.Visible = _Visible;
            }
        }

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                _Base.Position = value;
            }
        }

        public AnalogStick(Vector2 startPosition)
        {
            _Top = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/analog_top.png"))
            {
                Offset = new Vector2(105, 105),
                Position = startPosition,
            };
            _Base = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/analog_base.png"))
            {
                Offset = new Vector2(105, 105),
                Colour = new Vector4(1, 1, 1, 0.5f),
            };
            Position = startPosition;

            _HitBox = new Vector4(_Base.Position.X - _Base.Offset.X, _Base.Position.Y - _Base.Offset.Y, _Base.Size.X, _Base.Size.Y);
            TouchOrder = Constants.ZOrders.UI;
            _PressId = -1;

            TouchManager.Instance.AddTouchListener(this);
        }

        private Boolean Contains(Vector4 rect, Vector2 position)
        {
            Boolean within = position.X > rect.X 
                && position.X < rect.X + rect.W 
                && position.Y > rect.Y 
                && position.Y < rect.Y + rect.Z;
            return within;
        }

        public Boolean OnPress(Int32 id, Vector2 position)
        {
            _PressId = id;
            _Top.Visible = true;
            _Top.Position = Position;
            return true;
        }

        public Boolean OnMove(Int32 id, Vector2 position)
        {
            if (_PressId != id) return false;
            
            Vector2 newPosition = new Vector2(position.X - 960, position.Y -1080);

            _Top.Position = newPosition;
            return false;
        }

        public void OnRelease(Int32 id, Vector2 position)
        {
            if (_PressId != id) return;
            _Top.Position = Position;
            _Top.Visible = false;
            _PressId = -1;
        }

        public void OnCancel()
        {

        }

        public Boolean IsTouched(Vector2 position)
        {

            Vector4 topLeftAlignedHitBox = new Vector4(_HitBox.X + 960, _HitBox.Y + 1080, _HitBox.W, _HitBox.Z);

            return Contains(topLeftAlignedHitBox, position);
        }

        public void RegisterListener(IAnalogListener listener)
        {
            _Listeners.Add(listener);
        }

        public void DeRegisterListener(IAnalogListener listener)
        {
            _Listeners.Remove(listener);
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);
        }

        public override void Dispose()
        {
            base.Dispose();
            _Base.Dispose();
            _Top.Dispose();

            TouchManager.Instance.RemoveTouchListener(this);
        }
    }
}
