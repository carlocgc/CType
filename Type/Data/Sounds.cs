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
    /// <see cref="HitStop"/>.
    /// <para>
    /// Each interval is a global floor: the shortest time that may pass between any two plays of
    /// that sound, however many things ask for it. <see cref="TimeSpan.Zero"/> marks the sounds
    /// that cannot burst anyway, where a floor would only risk swallowing the one time they
    /// happen. **Tuned by measurement, not by ear** - see G4 in ROADMAP.md.
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

        /// <summary> A large enemy firing </summary>
        public static void LargeEnemyShot()
        {
            AudioController.Instance.Play("Content/Audio/laser4.wav", 1f, TimeSpan.FromMilliseconds(90));
        }

        /// <summary> Anything taking damage without dying. The busiest sound in the game </summary>
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

        /// <summary> A pickup the player could not use, converted to points. Bursts when a wave
        /// dies on top of the ship </summary>
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
