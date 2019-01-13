using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Type.Objects.Projectiles;

namespace Type.Objects.Enemies
{
    class EnemyC : BaseEnemy
    {
        public EnemyC(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate)
            : base(assetPath, spawnPos, rotation, direction, speed, fireRate)
        {
            PointValue = 50;
        }

        protected override void Fire()
        {
            if (!_IsHostile) return;
            new Bullet("Content/Graphics/bullet.png", GetCenter(), new Vector2(-1, 0), 1200, Math.PI, false, new Vector4(255, 0, 255, 1));
            _IsWeaponLocked = true;
        }
    }
}
