using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using static Type.Constants.Global;

namespace Type.Powerups
{
    /// <summary>
    /// Powerup that grants a probe
    /// </summary>
    public class ProbePowerup : GameObject, IPowerup
    {
        private readonly Random _Rnd = new Random(Environment.TickCount);

        /// <summary> Sprite for the powerup </summary>
        private readonly Sprite _Sprite;

        /// <summary> How long it takes for the powerup to expire </summary>
        private readonly TimeSpan _ExpireTime = TimeSpan.FromSeconds(10);

        /// <summary> how long ahs passed since this power up was created </summary>
        private TimeSpan _TimeSinceCreated;

        /// <summary> _direction the powerup is moving </summary>
        private Vector2 _Direction;

        /// <summary> The speed the powerup is moving </summary>
        private Single _Speed = 200;

        /// <inheritdoc />
        public Int32 ID { get; }

        /// <inheritdoc />
        public Int32 PointValue { get; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <inheritdoc />
        public Int32 HitPoints { get; }

        public ProbePowerup(Vector2 position)
        {
            ID = 2;
            PointValue = 200;
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.POWERUPS, Texture.GetTexture("Content/Graphics/Powerups/weapon_powerup.png")) { Visible = true, };
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);
            Position = position;
            HitBox = GetRect();

            _Direction = new Vector2((Single)Math.Sin(_Rnd.Next(0, 360)), (Single)Math.Sin(_Rnd.Next(0, 360)));
        }

        /// <inheritdoc />
        public void Apply(IPlayer player)
        {
            player.ApplyPowerup(this);

            foreach (IPowerupListener listener in _Listeners)
            {
                listener.OnPowerupApplied(this);
            }

            CollisionController.Instance.DeregisterPowerup(this);
            Dispose();
        }

        /// <inheritdoc />
        public void Hit(Int32 damage)
        {
        }

        /// <summary>
        /// Whether the enemy is destroyed
        /// </summary>
        public Boolean IsDestroyed { get; set; }

        /// <inheritdoc />
        public void Destroy()
        {
            IsDestroyed = true;
            CollisionController.Instance.DeregisterPowerup(this);
            foreach (IPowerupListener listener in _Listeners)
            {
                listener.OnPowerupExpired(this);
            }

            Dispose();
        }

        /// <inheritdoc />        
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            Position += _Direction * _Speed * (Single)timeTilUpdate.TotalSeconds;

            if (_Direction.X > 0 && Position.X >= ScreenRight - _Sprite.Offset.X)
            {
                _Direction = new Vector2((Single)Math.Sin(_Rnd.Next(190, 350)), _Direction.Y);
            }
            if (_Direction.X < 0 && Position.X <= ScreenLeft + _Sprite.Offset.X)
            {
                _Direction = new Vector2((Single)Math.Sin(_Rnd.Next(10, 80)), _Direction.Y);
            }
            if (_Direction.Y > 0 && Position.Y >= ScreenTop - _Sprite.Offset.Y)
            {
                _Direction = new Vector2(_Direction.X, (Single)Math.Sin(_Rnd.Next(190, 350)));
            }
            if (_Direction.Y < 0 && Position.Y <= ScreenBottom + _Sprite.Offset.Y)
            {
                _Direction = new Vector2(_Direction.X, (Single)Math.Sin(_Rnd.Next(10, 80)));
            }

            HitBox = GetRect();

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
