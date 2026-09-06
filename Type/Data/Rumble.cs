using System;
using Type.Services;

namespace Type.Data
{
    /// <summary>
    /// The events the controller rumbles for, and how hard each one feels.
    /// </summary>
    /// <remarks>
    /// Named events rather than numbers at the call sites, so the weights can be compared with
    /// each other in one place. Relative weight is the whole point of a rumble table: what
    /// matters is that a nuke outweighs a hit, not what either is in the absolute.
    /// <para>
    /// **These values are a starting point, not a tuned set.** They were chosen by reasoning
    /// about which event should outweigh which, and have never been felt — see the note against
    /// I8 in ROADMAP.md.
    /// </para>
    /// </remarks>
    public static class Rumble
    {
        /// <summary> The player's ship exploding </summary>
        public static void PlayerDeath()
        {
            InputService.Instance.Vibrate(1f, TimeSpan.FromMilliseconds(200));
        }

        /// <summary> A nuke going off, the heaviest thing the player can cause </summary>
        public static void Nuke()
        {
            InputService.Instance.Vibrate(1f, TimeSpan.FromMilliseconds(500));
        }

        /// <summary> The ship taking damage with no shield to absorb it </summary>
        public static void PlayerHit()
        {
            InputService.Instance.Vibrate(0.6f, TimeSpan.FromMilliseconds(150));
        }

        /// <summary>
        /// A shield taking a hit and holding. Deliberately the lightest event there is: it is
        /// the good outcome, and it can happen several times in a second.
        /// </summary>
        public static void ShieldAbsorbed()
        {
            InputService.Instance.Vibrate(0.35f, TimeSpan.FromMilliseconds(100));
        }

        /// <summary> A shield taking its last hit and going down </summary>
        public static void ShieldLost()
        {
            InputService.Instance.Vibrate(0.7f, TimeSpan.FromMilliseconds(250));
        }

        /// <summary> A boss being destroyed </summary>
        public static void BossDestroyed()
        {
            InputService.Instance.Vibrate(1f, TimeSpan.FromMilliseconds(600));
        }
    }
}
