using OpenTK;
using System;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using Type.Base;
using Type.Controllers;
using Type.Interfaces.Weapons;
using Type.Objects.Player;
using Type.Objects.Probes;
using static Type.Constants.Global;

namespace Type.Objects.Projectiles
{
    /// <summary>
    /// Projectile fired by <see cref="PlayerAlpha"/> & <see cref="LaserProbe"/>'s
    /// </summary>
    public class Laser : GameObject, IProjectile
    {
        /// <summary> The speed of the projectile </summary>
        private readonly Single _Speed;
        /// <summary> Direction of the projectile </summary>
        private readonly Vector2 _Direction;

        /// <inheritdoc />
        public Int32 HitPoints { get; private set; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <inheritdoc />
        public Int32 Damage { get; }

        /// <summary> Whether the enemy is on screen </summary>
        private Boolean OnScreen => Position.X - _Sprite.Offset.X <= ScreenRight && Position.Y - _Sprite.Offset.Y >= ScreenBottom && Position.Y + _Sprite.Offset.Y <= ScreenTop;

        public Laser(Vector2 spawnPos, Vector2 direction, Single speed, Double rotation)
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.BULLETS, Texture.GetTexture("Content/Graphics/Player/alpha-laser.png"))
            {
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            _Sprite.RotationOrigin = _Sprite.Size / 2;
            _Sprite.Rotation = rotation;
            AddSprite(_Sprite);

            _Direction = direction;
            _Speed = speed;

            HitBox = GetRect();
            Position = spawnPos;
            Rotation = rotation;
            Damage = 1;

            CollisionController.Instance.RegisterPlayerProjectile(this);
        }

        /// <summary>
        /// Updates the position every update and destroys itself if out of bounds of the screen
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            Position += _Speed * _Direction * (Single)timeTilUpdate.TotalSeconds;
            HitBox = GetRect();

            if (!OnScreen) Destroy();
        }

        public void Hit(Int32 damage)
        {
        }

        /// <inheritdoc />
        public void Destroy()
        {
            Dispose();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            CollisionController.Instance.DeregisterProjectile(this);
            base.Dispose();
        }
    }
}
