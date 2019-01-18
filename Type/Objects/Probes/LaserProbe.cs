using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Base;
using Type.Interfaces.Probe;
using Type.Objects.Projectiles;

namespace Type.Objects.Probes
{
    public class LaserProbe : GameObject, IProbe
    {
        /// <summary> How often the probe fires </summary>
        private readonly TimeSpan _FireRate;
        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;

        private Vector2 _OrbitAnchor;

        private Single _Angle;

        private Single _Radius;

        /// <summary> Whether the probe should be shooting </summary>
        public Boolean Shoot { get; set; }

        public LaserProbe(Vector2 orbitPosition)
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/Probes/laser-probe.png"))
            {
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);

            _Radius = 100;
            _FireRate = TimeSpan.FromMilliseconds(100);

            Position = new Vector2(orbitPosition.X, orbitPosition.Y + _Radius);
        }

        /// <summary>
        /// Creates a new Bullet
        /// </summary>
        private void FireForward()
        {
            new Bullet("Content/Graphics/bullet.png", Position, new Vector2(1, 0), 1000, 0, true, new Vector4(1, 1, 1, 1));
            _IsWeaponLocked = true;
        }

        public void UpdatePosition(Vector2 position)
        {
            Position = new Vector2(position.X, position.Y + _Radius);
        }

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

            if (Shoot && !_IsWeaponLocked) FireForward();
        }
    }
}
