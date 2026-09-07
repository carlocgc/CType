using System;
using Type.Controllers;

namespace Type.Data
{
    /// <summary>
    /// The events that make a noise, which file each one plays, and how often it is allowed to.
    /// </summary>
    /// <remarks>
    /// Named events rather than file paths at the call sites, the same shape as
    /// <see cref="Rumble"/>, <see cref="Particles"/>, <see cref="Shake"/> and
    /// <see cref="HitStop"/>, and for the same reason: the intervals below are only meaningful
    /// relative to each other, and that is invisible when they are spread across fifty call
    /// sites in twenty files.
    /// <para>
    /// **The intervals are the whole point of this file.** They replace seven copies of a
    /// per-enemy timer that bounded one enemy each and the game not at all. Every number here is
    /// a global floor: the gap below is the shortest time that may pass between any two plays of
    /// that sound, no matter how many things asked for it.
    /// </para>
    /// <para>
    /// **A short interval is not the same as no interval.** Anything at
    /// <see cref="TimeSpan.Zero"/> below is a sound that already cannot repeat quickly — a nuke,
    /// a pickup, a death — and giving those a floor would only risk swallowing the one time they
    /// happen. The sounds with real numbers are the ones a firefight asks for in bursts.
    /// </para>
    /// <para>
    /// **These values were tuned by measurement, not by ear** — the drop counts in G4 in
    /// ROADMAP.md. They have not been judged by anyone listening to them, which is a different
    /// question and still open, exactly as it is for the G3 magnitudes.
    /// </para>
    /// </remarks>
    public static class Sounds
    {
        /// <summary> The player's ship firing. Already bounded by the weapon's own fire rate </summary>
        public static void PlayerShot()
        {
            AudioController.Instance.Play("Content/Audio/laser1.wav", 0.5f, TimeSpan.Zero);
        }

        /// <summary> A small or medium enemy, or a boss cannon, firing </summary>
        public static void EnemyShot()
        {
            AudioController.Instance.Play("Content/Audio/laser2.wav", 1f, TimeSpan.FromMilliseconds(90));
        }

        /// <summary> A large enemy firing, which is a heavier sound than the rest </summary>
        public static void LargeEnemyShot()
        {
            AudioController.Instance.Play("Content/Audio/laser4.wav", 1f, TimeSpan.FromMilliseconds(90));
        }

        /// <summary>
        /// Anything taking damage without dying. The busiest sound in the game by a wide margin,
        /// and the one the old per-enemy timers existed to hold back.
        /// </summary>
        public static void Hit()
        {
            AudioController.Instance.Play("Content/Audio/hurt3.wav", 1f, TimeSpan.FromMilliseconds(80));
        }

        /// <summary> Anything blowing up. Second busiest, and it arrives in clusters </summary>
        public static void Destroyed()
        {
            AudioController.Instance.Play("Content/Audio/explode.wav", 1f, TimeSpan.FromMilliseconds(60));
        }

        /// <summary> A pickup collected </summary>
        public static void PointsPickup()
        {
            AudioController.Instance.Play("Content/Audio/points_pickup.wav", 1f, TimeSpan.FromMilliseconds(50));
        }

        /// <summary>
        /// A pickup the player could not use, converted to points instead. Bursts when a wave
        /// dies on top of the ship, so it needs a floor for the same reason the hit sound does.
        /// </summary>
        public static void PointsInstead()
        {
            AudioController.Instance.Play("Content/Audio/points_instead.wav", 1f, TimeSpan.FromMilliseconds(50));
        }

        /// <summary> A probe attaching to the ship </summary>
        public static void ProbeAttached()
        {
            AudioController.Instance.Play("Content/Audio/upgrade1.wav", 1f, TimeSpan.Zero);
        }

        /// <summary> The shield coming up </summary>
        public static void ShieldOn()
        {
            AudioController.Instance.Play("Content/Audio/shield_on.wav", 1f, TimeSpan.Zero);
        }

        /// <summary> The shield going down </summary>
        public static void ShieldOff()
        {
            AudioController.Instance.Play("Content/Audio/shield_off.wav", 1f, TimeSpan.Zero);
        }

        /// <summary> A nuke picked up </summary>
        public static void NukePickup()
        {
            AudioController.Instance.Play("Content/Audio/nuke_pickup.wav", 1f, TimeSpan.Zero);
        }

        /// <summary> A nuke going off, which clears the screen </summary>
        public static void NukeDetonated()
        {
            AudioController.Instance.Play("Content/Audio/nuke.wav", 1f, TimeSpan.Zero);
        }

        /// <summary> An extra life gained </summary>
        public static void LifeGained()
        {
            AudioController.Instance.Play("Content/Audio/lifeup.wav", 1f, TimeSpan.Zero);
        }

        /// <summary> A life lost </summary>
        public static void LifeLost()
        {
            AudioController.Instance.Play("Content/Audio/death.wav", 1f, TimeSpan.Zero);
        }

        /// <summary> The engine splash on startup </summary>
        public static void EngineSplash()
        {
            AudioController.Instance.Play("Content/Audio/Hello.wav", 1f, TimeSpan.Zero);
        }
    }
}
