﻿using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
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
    public class Bullet : GameObject, IProjectile
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

        public Bullet(Vector2 spawnPos, Vector2 direction, Single speed, Double rotation)
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.BULLETS, Texture.GetTexture("Content/Graphics/Player/omega_bullet.png")) {Visible = true,};
            _Sprite.Offset = _Sprite.Size / 2;
            _Sprite.RotationOrigin = _Sprite.Size / 2;
            _Sprite.Rotation = rotation;
            AddSprite(_Sprite);

            _Direction = direction;
            _Speed = speed;
            Position = spawnPos;
            Rotation = rotation;
            Damage = 1;
            HitBox = GetRect();

            CollisionController.Instance.RegisterPlayerProjectile(this);
        }

        /// <summary>
        /// Updates the position every update and destroys itself if out of bounds of the screen
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            Position += _Speed * _Direction * (Single) timeTilUpdate.TotalSeconds;
            HitBox = GetRect();

            if (!OnScreen) Destroy();
        }

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
