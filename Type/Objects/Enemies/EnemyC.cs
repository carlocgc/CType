using AmosShared.Audio;
using OpenTK;
using System;
using Type.Objects.Projectiles;

namespace Type.Objects.Enemies
{
    class EnemyC : BaseEnemy
    {
        public EnemyC(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate, Int32 hitPoints)
            : base(assetPath, spawnPos, rotation, direction, speed, fireRate, hitPoints)
        {
            PointValue = 50;
        }

        protected override void Fire()
        {
            if (!_IsHostile) return;

            Vector2 bulletDirection = _DirectionTowardsPlayer;
            if (bulletDirection != Vector2.Zero) bulletDirection.Normalize();
            new Bullet("Content/Graphics/enemybullet.png", Position, bulletDirection, 1050, Rotation, false, new Vector4(255, 0, 255, 1));

            _IsWeaponLocked = true;
            new AudioPlayer("Content/Audio/laser4.wav", false, AudioManager.Category.EFFECT, 1);
        }
    }
}
