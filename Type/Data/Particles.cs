using OpenTK;
using System;
using Type.Controllers;

namespace Type.Data
{
    /// <summary>
    /// The events that throw particles, and what each one looks like.
    /// </summary>
    /// <remarks>
    /// Named effects rather than numbers at the call sites, the same shape as
    /// <see cref="Rumble"/> and for the same reason: what matters is that a boss outweighs a
    /// fighter, and that is only visible if the two sit next to each other.
    /// <para>
    /// **These are the whole explosion now.** Deaths used to play a nine frame sprite animation
    /// as well, which read as disconnected from the debris thrown at the same moment - one
    /// fireball sitting still while the particles flew out of it. The animation is gone and these
    /// carry the death on their own, which is why the counts here are roughly double what they
    /// were.
    /// </para>
    /// <para>
    /// Each effect is layered: a small, bright, short lived core that reads as the flash, fast
    /// sparks that carry the shape outwards, and slower embers that linger. That ordering matters
    /// more than any single number - a burst with one layer reads as confetti.
    /// </para>
    /// </remarks>
    public static class Particles
    {
        /// <summary> The flash at the centre of a detonation, effectively white </summary>
        private static readonly Vector4 Core = new Vector4(1f, 1f, 0.92f, 1f);

        /// <summary> Hot debris, close to white </summary>
        private static readonly Vector4 Spark = new Vector4(1f, 0.95f, 0.75f, 1f);

        /// <summary> Cooler outer debris </summary>
        private static readonly Vector4 Ember = new Vector4(1f, 0.6f, 0.25f, 1f);

        /// <summary> A wave enemy breaking up </summary>
        public static void EnemyDestroyed(Vector2 position)
        {
            ParticleController.Instance.Burst(position, 10, 20, 110, Core, 0.22f, 6f, 4.5f);
            ParticleController.Instance.Burst(position, 26, 140, 460, Spark, 0.45f, 3.2f, 2.2f);
            ParticleController.Instance.Burst(position, 20, 60, 240, Ember, 0.9f, 2.6f, 1.4f);
        }

        /// <summary>
        /// One of the blasts running across a boss that is coming apart
        /// </summary>
        /// <remarks>
        /// Smaller than a wave enemy's death on purpose. Around thirty of these land in a row, so
        /// each one has to read as a piece of the boss failing rather than as a kill in itself.
        /// </remarks>
        public static void BossDeathBlast(Vector2 position)
        {
            ParticleController.Instance.Burst(position, 6, 20, 90, Core, 0.18f, 4.5f, 4.5f);
            ParticleController.Instance.Burst(position, 14, 110, 330, Spark, 0.4f, 2.6f, 2.4f);
            ParticleController.Instance.Burst(position, 10, 50, 180, Ember, 0.75f, 2.2f, 1.5f);
        }

        /// <summary> A boss finally going up, at the end of the blasts </summary>
        public static void BossDestroyed(Vector2 position)
        {
            ParticleController.Instance.Burst(position, 18, 30, 170, Core, 0.32f, 10f, 3.5f);
            ParticleController.Instance.Burst(position, 48, 180, 620, Spark, 0.8f, 5.5f, 1.6f);
            ParticleController.Instance.Burst(position, 40, 80, 320, Ember, 1.4f, 4.5f, 1.0f);
        }

        /// <summary> The player's ship exploding </summary>
        public static void PlayerDestroyed(Vector2 position)
        {
            ParticleController.Instance.Burst(position, 14, 25, 140, Core, 0.28f, 7f, 4f);
            ParticleController.Instance.Burst(position, 34, 160, 520, Spark, 0.7f, 4.5f, 1.8f);
            ParticleController.Instance.Burst(position, 28, 70, 280, Ember, 1.2f, 3.6f, 1.1f);
        }
    }
}
