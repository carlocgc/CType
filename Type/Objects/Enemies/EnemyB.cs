using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Type.Objects.Projectiles;

namespace Type.Objects.Enemies
{
    public class EnemyB : BaseEnemy
    {
        public EnemyB(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate)
            : base(assetPath, spawnPos, rotation, direction, speed, fireRate)
        {
            PointValue = 25;
        }

        protected override void Fire()
        {
            if (!_IsHostile) return;
            new Bullet("Content/Graphics/bullet.png", GetCenter(), new Vector2(-1, 0), 700, Math.PI, false);
            _IsWeaponLocked = true;
        }
    }
}
