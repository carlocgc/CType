using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Type.Objects.Projectiles;

namespace Type.Objects.Enemies
{
    public class EnemyA : BaseEnemy
    {
        public EnemyA(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate)
            : base(assetPath, spawnPos, rotation, direction, speed, fireRate)
        {
            PointValue = 10;
        }

        protected override void Fire()
        {
            if (!_IsHostile) return;
            new Bullet("Content/Graphics/bullet.png", Position, _BulletSpawnPos, new Vector2(-1, 0), 700, Math.PI, false, true);
            _IsWeaponLocked = true;
        }
    }
}
