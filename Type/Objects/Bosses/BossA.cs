using OpenTK;
using System;
using Type.Objects.Enemies;

namespace Type.Objects.Bosses
{
    public class BossA : BaseEnemy
    {
        /// <inheritdoc />
        public BossA(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate, Int32 hitPoints) : base(assetPath, spawnPos, rotation, direction, speed, fireRate, hitPoints)
        {
            PointValue = 5000;
        }

        /// <inheritdoc />
        protected override void Fire()
        {

        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);
        }
    }
}
