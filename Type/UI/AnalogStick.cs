using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using AmosShared.Touch;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Interfaces.Control;

namespace Type.UI
{
    public class AnalogStick : GameObject, ITouchListener
    {
        private readonly List<IAnalogListener> _Listeners;

        private readonly Sprite _Base;

        private readonly Sprite _Top;

        private readonly Single _Radius;

        private readonly Vector4 _HitBox;

        private Boolean _Visible;

        private Int32 _PressId;

        private Vector2 _DirectionNorm;

        private Single _PushDistance;

        private Vector2 _TouchBase;

        private Vector2 _TouchCurrent;

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

        public AnalogStick(Vector2 startPosition, Single radius)
        {
            _Listeners = new List<IAnalogListener>();

            _Top = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/analog_top.png"))
            {
                Offset = new Vector2(105, 105),
                Colour = new Vector4(1, 1, 1, 0.5f),
                Position = startPosition,
            };
            _Base = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/analog_base.png"))
            {
                Offset = new Vector2(105, 105),
                Colour = new Vector4(1, 1, 1, 0.5f),
                Position = startPosition,
            };
            Position = startPosition;

            _HitBox = new Vector4(_Base.Position.X - _Base.Offset.X * 4, _Base.Position.Y - _Base.Offset.Y * 4, _Base.Size.X * 4, _Base.Size.Y * 4);

            _Radius = radius;
            TouchOrder = Constants.ZOrders.UI;
            _PressId = -1;

            TouchManager.Instance.AddTouchListener(this);
        }


        public Boolean IsTouched(Vector2 position)
        {
            if (_PressId > -1) return false;

            return Contains(_HitBox, ConvertToCenterAligned(position));
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
            if (_PressId > -1) return false;

            _PressId = id;
            _Top.Visible = true;
            _Top.Position = _Base.Position;

            _TouchBase = ConvertToCenterAligned(position);

            return true;
        }

        public Boolean OnMove(Int32 id, Vector2 position)
        {
            if (_PressId != id) return false;

            // Amos was ere

            _TouchCurrent = ConvertToCenterAligned(position);

            Vector2 direction = _TouchCurrent - _TouchBase;

            Single length = Math.Min(_Radius, direction.Length);

            Single angle = (Single) (Math.Atan2(1, 0) - Math.Atan2(direction.Y, direction.X));

            Vector2 pointOnCircle = new Vector2((Single)(length * Math.Sin(angle)), (Single)(length * Math.Cos(angle)));

            _Top.Position = _Base.Position + pointOnCircle;

            PrepareListenerData(length);

            return false;
        }

        private void PrepareListenerData(Single length)
        {
            _DirectionNorm = _Top.Position - _Base.Position;

            // Prevent _DirectionNorm evaluating to NaN
            if (_DirectionNorm != Vector2.Zero) _DirectionNorm.Normalize();
            _PushDistance = length / _Radius;
        }

        public void OnRelease(Int32 id, Vector2 position)
        {
            if (_PressId != id) return;

            _TouchBase = Vector2.Zero;
            _TouchCurrent = Vector2.Zero;

            _Top.Position = Position;
            _Top.Visible = false;

            ResetListenerData();

            _PressId = -1;
        }

        private void ResetListenerData()
        {
            _DirectionNorm = Vector2.Zero;
            _PushDistance = 0;
        }

        public void OnCancel()
        {

        }

        private Vector2 ConvertToCenterAligned(Vector2 position)
        {
            return new Vector2(position.X - Renderer.Instance.TargetDimensions.X / 2, (position.Y - Renderer.Instance.TargetDimensions.Y / 2) * -1);
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

            foreach (IAnalogListener listener in _Listeners)
            {
                listener.UpdateAnalogData(_DirectionNorm, _PushDistance);
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
