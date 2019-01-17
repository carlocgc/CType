using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using AmosShared.Base;
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

        private Vector2 _StartPosition;

        private Vector2 _MoveLimit = new Vector2(100, 100);

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
            _Listeners = new List<IAnalogListener>();

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
            _StartPosition = startPosition;
            Position = _StartPosition;

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

            //if (direction.X < 0 && newPosition.X > _Base.Position.X - pointOnCircle.X) newPosition = _StartPosition + pointOnCircle;
            //if (direction.Y < 0 && newPosition.Y > _Base.Position.Y - pointOnCircle.Y) newPosition = _StartPosition + pointOnCircle;
            //if (direction.X > 0 && newPosition.X < _Base.Position.X + pointOnCircle.X) newPosition = _StartPosition + pointOnCircle;
            //if (direction.Y > 0 && newPosition.Y < _Base.Position.Y + pointOnCircle.Y) newPosition = _StartPosition + pointOnCircle;

            Vector2 newPosition = new Vector2(position.X - Renderer.Instance.TargetDimensions.X / 2, (position.Y - Renderer.Instance.TargetDimensions.Y / 2) * -1);

            Vector2 direction = newPosition - _StartPosition;

            Single length = Math.Min(200, _Top.Position.Length);

            Single angle = (Single) (Math.Atan2(1, 0) - Math.Atan2(direction.Y, direction.X));

            Vector2 pointOnCircle = new Vector2((Single)(length * Math.Sin(angle)), (Single)(length * Math.Cos(angle)));

            _Top.Position = _StartPosition + pointOnCircle;

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

            Vector2 startPoint = _Base.Position;
            Vector2 currentPoint = _Top.Position;

            _Direction = startPoint - currentPoint;

            foreach (IAnalogListener listener in _Listeners)
            {
                listener.UpdatePosition(_Direction);
            }
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
