using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Base;
using Type.Data;
using Type.Interfaces.Probe;
using Type.Interfaces.Weapons;
using Type.Objects.Projectiles;

namespace Type.Objects.Probes
{
    public class LaserProbe : GameObject, IProbe
    {
        /// <summary> How often the probe fires </summary>
        private readonly TimeSpan _FireRate;
        /// <summary> The speed the probe is moving </summary>
        private readonly Single _Speed;
        /// <summary> how far away from the <see cref="_OrbitAnchor"/> the probe is </summary>
        private readonly Single _Radius;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Point the probe orbits </summary>
        private Vector2 _OrbitAnchor;
        /// <summary>  Angle the probe is at from north </summary>
        private Single _Angle;

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }
        /// <inheritdoc />
        public Boolean AutoFire { get; set; }
        /// <inheritdoc />
        public Int32 HitPoints { get; }

        /// <summary>
        /// Laser probe that orbits the player ship
        /// </summary>
        /// <param name="orbitPosition"> The player ship position </param>
        /// <param name="angle"> The initial angle of the probe on the orbit around the player </param>
        public LaserProbe(Vector2 orbitPosition, Single angle)
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/Probes/laser-probe.png"));
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);

            _Radius = 150;
            _Angle = angle;
            _Speed = 0.05f;
            _FireRate = TimeSpan.FromMilliseconds(300);

            _OrbitAnchor = orbitPosition;
            HitBox = GetRect();
            HitPoints = 1;

            Position = new Vector2(_OrbitAnchor.X, _OrbitAnchor.Y + _Radius);
        }

        public void UpdatePosition(Vector2 position)
        {
            _OrbitAnchor = position;

            Single x = _OrbitAnchor.X + (Single)Math.Cos(_Angle) * _Radius;
            Single y = _OrbitAnchor.Y + (Single)Math.Sin(_Angle) * _Radius;
            Position = new Vector2(x, y);

            _Angle += _Speed;

            if (!_Sprite.Visible) _Sprite.Visible = true;
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);
            if (_IsWeaponLocked)
            {
                _TimeSinceLastFired += timeTilUpdate;
                if (_TimeSinceLastFired >= _FireRate)
                {
                    _IsWeaponLocked = false;
                    _TimeSinceLastFired = TimeSpan.Zero;
                }
            }

            if (AutoFire && !_IsWeaponLocked) Shoot();
        }

        /// <inheritdoc />
        public void Shoot()
        {
            new Laser(Position + new Vector2(_Sprite.Width / 2, 0), new Vector2(1, 0), 1000, 0);
            _IsWeaponLocked = true;
            GameStats.Instance.BulletsFired++;
        }

        /// <inheritdoc />
        public void Hit(Int32 damage)
        {
        }

        /// <inheritdoc />
        public void Destroy()
        {
        }
    }
}
