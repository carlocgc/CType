using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Audio;
using OpenTK;
using Type.Objects.Projectiles;

namespace Type.Objects.Enemies
{
    public class EnemyB : BaseEnemy
    {
        public EnemyB(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate, Int32 hitPoints)
            : base(assetPath, spawnPos, rotation, direction, speed, fireRate, hitPoints)
        {
            PointValue = 25;
        }

        protected override void Fire()
        {
            if (!_IsHostile) return;

            Vector2 bulletDirection = _DirectionTowardsPlayer;
            if (bulletDirection != Vector2.Zero) bulletDirection.Normalize();
            new Bullet("Content/Graphics/enemybullet.png", Position, bulletDirection, 1000, Rotation, false, new Vector4(255, 255, 0, 1));
            _IsWeaponLocked = true;

            new AudioPlayer("Content/Audio/laser3.wav", false, AudioManager.Category.EFFECT, 1);
        }
    }
}
