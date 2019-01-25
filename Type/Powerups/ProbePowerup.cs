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
        public Int32 ID { get; private set; }

        /// <inheritdoc />
        public Int32 PointValue { get; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <inheritdoc />
        public Int32 HitPoints { get; }

        public ProbePowerup(Vector2 position)
        {
            ID = 2;
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.POWERUPS, Texture.GetTexture("Content/Graphics/Powerups/weapon_powerup.png")) { Visible = true, };
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);
            Position = position;
            HitBox = GetRect();
            _Direction = new Vector2((Single)_Rnd.NextDouble(), (Single)_Rnd.NextDouble());

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
            Position += _Direction * _Speed * (Single)timeTilUpdate.TotalSeconds;
            ;

            if (Position.X >= ScreenRight - GetSprite().Width / 2 || Position.X <= ScreenLeft + GetSprite().Width / 2)
            {
                _Direction = new Vector2((Single)_Rnd.NextDouble() * -1, _Direction.Y);
            }

            if (Position.Y >= ScreenTop - GetSprite().Height / 2 || Position.Y <= ScreenBottom + GetSprite().Height / 2)
            {
                _Direction = new Vector2(_Direction.X, (Single)_Rnd.NextDouble() * -1);
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
