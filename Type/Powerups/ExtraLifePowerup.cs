using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Interfaces;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;

namespace Type.Powerups
{
    /// <summary>
    /// Powerup that grants an extra life
    /// </summary>
    public class ExtraLifePowerup : GameObject, IPowerup, INotifier<IPowerupListener>
    {
        /// <summary> Center screen, point around which the powerup will orbit </summary>
        private readonly Vector2 _Center = new Vector2(0, 0);
        /// <summary> Sprite for the powerup </summary>
        private readonly Sprite _Sprite;
        /// <summary> How long it takes for the powerup to expire </summary>
        private readonly TimeSpan _ExpireTime = TimeSpan.FromSeconds(10);
        /// <summary> how long ahs passed since this power up was created </summary>
        private TimeSpan _TimeSinceCreated;
        /// <summary> Number used to generate X point on the orbit </summary>
        private Single _XAngle = 0;
        /// <summary> Number used to generate Y point on the orbit </summary>
        private Single _YAngle = 0;
        /// <summary> Number used to increment XAngle each update </summary>
        private Single _XSpeed = 0.1f;
        /// <summary> Number used to increment YAngle each update </summary>
        private Single _YSpeed = 0.131f;
        /// <summary> Max distance from the center in the X axis </summary>
        private readonly Single _XRadius = Renderer.Instance.TargetDimensions.X / 2;
        /// <summary> Max distance from the center in the Y axis </summary>
        private readonly Single _YRadius = Renderer.Instance.TargetDimensions.Y / 2;

        /// <inheritdoc />
        public Int32 ID { get; private set; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <inheritdoc />
        public Int32 HitPoints { get; }

        public ExtraLifePowerup(Vector2 position)
        {
            ID = 1;
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.POWERUPS, Texture.GetTexture("Content/Graphics/Powerups/extralife_powerup.png"))
            {
                Position = position,
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);
            HitBox = GetRect();
        }

        /// <inheritdoc />
        public void Apply(IPlayer player)
        {
            player.ApplyPowerup(ID);

            foreach (IPowerupListener listener in _Listeners)
            {
                listener.OnPowerupApplied(this);
            }
        }

        /// <inheritdoc />
        public void Hit(Int32 damage)
        {
        }

        /// <inheritdoc />
        public void Destroy()
        {
            CollisionController.Instance.DeregisterPowerup(this);
            foreach (IPowerupListener listener in _Listeners)
            {
                listener.OnPowerupExpired(this);
            }
            Dispose();
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            Single x = _Center.X + (Single)Math.Cos(_XAngle) * _XRadius;
            Single y = _Center.Y + (Single)Math.Cos(_YAngle) * _YRadius;

            Position = new Vector2(x, y);
            HitBox = GetRect();

            _XAngle += _XSpeed;
            _YAngle += _YSpeed;

            if (_XAngle > 360) _XAngle = 0;
            if (_YAngle > 360) _YAngle = 0;

            _TimeSinceCreated += timeTilUpdate;
            if (_TimeSinceCreated > _ExpireTime) Destroy();
        }

        #region Listener

        private readonly List<IPowerupListener> _Listeners = new List<IPowerupListener>();

        /// <inheritdoc />
        public void RegisterListener(IPowerupListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IPowerupListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

        #endregion
    }
}
