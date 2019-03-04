using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using AmosShared.Touch;
using OpenTK;
using System;
using Type.Base;

namespace Type.Buttons
{
    /// <summary>
    /// Onscreen analog stick
    /// </summary>
    public class VirtualAnalogStick : GameObject, ITouchListener
    {
        /// <summary> Sprite of the analog base </summary>
        private readonly Sprite _Base;
        /// <summary> SPrite of the analog top </summary>
        private readonly Sprite _Top;
        /// <summary> Tops max distance form base </summary>
        private readonly Single _Radius;
        /// <summary> The touch hitbox of the analog stick </summary>
        private readonly Vector4 _HitBox;
        /// <summary> Whether the stick is visible </summary>
        private Boolean _Visible;
        /// <summary> Touch press id, used to check the which press is engaging the stick </summary>
        private Int32 _PressId;
        /// <summary> Position of the initial press that engaged the stick </summary>
        private Vector2 _InitialPressPosition;
        /// <summary> The current position of the press that has engaged the stick </summary>
        private Vector2 _CurrentPressPosition;
        /// <summary> Whether or not the listener is enabled </summary>
        public Boolean TouchEnabled { get; set; }
        /// <summary> The priority of the touch </summary>
        public Int32 TouchOrder { get; set; }
        /// <summary> Whether or not the touch order has changed </summary>
        public Boolean TouchOrderChanged { get; set; }
        /// <summary> Whether or not the listener is listening for a touch moving event </summary>
        public Boolean ListeningForMove { get; set; }
        /// <summary> The current X output position of the stick </summary>
        public Single X { get; set; }
        /// <summary> The current Y output position of the stick </summary>
        public Single Y { get; set; }
        /// <summary> The strength of the stick push </summary>
        public Single Magnitude { get; set; }

        /// <summary> Whether the stick is visible </summary>
        public Boolean Visible
        {
            get => _Visible;
            set
            {
                _Visible = value;
                _Base.Visible = _Visible;
                if (_Top.Visible) _Top.Visible = false;
            }
        }

        /// <summary> The sticks position on the screen </summary>
        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                _Base.Position = value;
            }
        }

        public VirtualAnalogStick(Vector2 startPosition, Single radius)
        {
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

        /// <summary> Check to see if the listener has been touched </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Boolean IsTouched(Vector2 position)
        {
            if (_PressId > -1) return false;

            return Contains(_HitBox, ConvertToCenterAligned(position));
        }

        /// <summary>
        /// Bounds check to see if a press engages the stick
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private Boolean Contains(Vector4 rect, Vector2 position)
        {
            Boolean within = position.X > rect.X
                && position.X < rect.X + rect.W
                && position.Y > rect.Y
                && position.Y < rect.Y + rect.Z;
            return within;
        }

        /// <summary> Function that is called when the touch listener is pressed </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        public Boolean OnPress(Int32 id, Vector2 position)
        {
            if (_PressId > -1) return false;

            _PressId = id;
            _Top.Visible = true;
            _Top.Position = _Base.Position;

            _InitialPressPosition = ConvertToCenterAligned(position);

            return true;
        }

        /// <summary> Function that is called when the mouse moves </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        public Boolean OnMove(Int32 id, Vector2 position)
        {
            if (_PressId != id) return false;

            // Amos was ere

            _CurrentPressPosition = ConvertToCenterAligned(position);

            Vector2 direction = _CurrentPressPosition - _InitialPressPosition;

            Single length = Math.Min(_Radius, direction.Length);

            Single angle = (Single) (Math.Atan2(1, 0) - Math.Atan2(direction.Y, direction.X));

            Vector2 pointOnCircle = new Vector2((Single)(length * Math.Sin(angle)), (Single)(length * Math.Cos(angle)));

            _Top.Position = _Base.Position + pointOnCircle;

            PrepareListenerData(length);

            return false;
        }

        /// <summary>
        /// Prepares the sticks position data to be consumed by a listener
        /// </summary>
        /// <param name="length"></param>
        private void PrepareListenerData(Single length)
        {
            Vector2 direction = _Top.Position - _Base.Position;

            // Prevent _DirectionNorm evaluating to NaN
            if (direction != Vector2.Zero) direction.Normalize();

            X = direction.X;
            Y = direction.Y;
            Magnitude = length / _Radius;
        }

        /// <summary> Function that is called when the mouse is released </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        public void OnRelease(Int32 id, Vector2 position)
        {
            if (_PressId != id) return;

            _InitialPressPosition = Vector2.Zero;
            _CurrentPressPosition = Vector2.Zero;

            _Top.Position = Position;
            _Top.Visible = false;

            ResetListenerData();

            _PressId = -1;
        }

        /// <summary>
        /// Resets the stick public data
        /// </summary>
        private void ResetListenerData()
        {
            X = 0;
            Y = 0;
            Magnitude = 0;
        }

        /// <summary> Called when the touch has been cancelled </summary>
        public void OnCancel()
        {

        }

        /// <summary>
        /// Converts a given position to be center aligned in relation to the screen
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector2 ConvertToCenterAligned(Vector2 position)
        {
            return new Vector2(position.X - Renderer.Instance.TargetDimensions.X / 2, (position.Y - Renderer.Instance.TargetDimensions.Y / 2) * -1);
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
