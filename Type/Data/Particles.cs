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
    /// Colours are warm and light rather than matched to the thing that died. The explosion
    /// sprite is one shared animation for every enemy in the game, so debris tinted per enemy
    /// would be the only part of a death that knew what died, which reads as inconsistent
    /// rather than as detail. G6 is where that gets fixed properly.
    /// </para>
    /// </remarks>
    public static class Particles
    {
        /// <summary> Hot core of an explosion, close to white </summary>
        private static readonly Vector4 Spark = new Vector4(1f, 0.95f, 0.75f, 1f);

        /// <summary> Cooler outer debris </summary>
        private static readonly Vector4 Ember = new Vector4(1f, 0.6f, 0.25f, 1f);

        /// <summary> A wave enemy breaking up </summary>
        public static void EnemyDestroyed(Vector2 position)
        {
            ParticleController.Instance.Burst(position, 14, 90, 300, Spark, 0.5f, 3.5f, 1.8f);
            ParticleController.Instance.Burst(position, 12, 40, 170, Ember, 0.8f, 2.8f, 1.2f);
        }

        /// <summary>
        /// A boss breaking up. Bigger, slower and longer than a fighter on every axis, so the
        /// difference is legible without needing a different effect.
        /// </summary>
        public static void BossDestroyed(Vector2 position)
        {
            ParticleController.Instance.Burst(position, 30, 120, 460, Spark, 0.9f, 7f, 1.4f);
            ParticleController.Instance.Burst(position, 26, 60, 260, Ember, 1.3f, 5.5f, 0.9f);
        }

        /// <summary> The player's ship exploding </summary>
        public static void PlayerDestroyed(Vector2 position)
        {
            ParticleController.Instance.Burst(position, 22, 110, 360, Spark, 0.8f, 5f, 1.5f);
            ParticleController.Instance.Burst(position, 18, 50, 210, Ember, 1.1f, 4f, 1.0f);
        }
    }
}
