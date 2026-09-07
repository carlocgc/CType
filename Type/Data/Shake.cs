using System;
using Type.Controllers;

namespace Type.Data
{
    /// <summary>
    /// The events that shake the camera, and how hard each one hits.
    /// </summary>
    /// <remarks>
    /// Named events rather than numbers at the call sites, the same shape as <see cref="Rumble"/>
    /// and <see cref="Particles"/> and for the same reason: what matters is that a boss outweighs
    /// a nuke, and that is only visible if the two sit next to each other.
    /// <para>
    /// **Deliberately a short list.** Screen shake stops meaning anything the moment it happens
    /// often, so it is reserved for the three events that end something: a nuke, a boss, and the
    /// player. Ordinary enemy deaths get particles and nothing else.
    /// </para>
    /// <para>
    /// Magnitudes are in world units on a fixed 1920x1080 field, so 30 is about one and a half
    /// percent of the width. **These values have not been tuned by eye** — see G3 in ROADMAP.md.
    /// </para>
    /// </remarks>
    public static class Shake
    {
        /// <summary> A nuke going off, which clears the screen </summary>
        public static void Nuke()
        {
            ScreenShakeController.Instance.Shake(26f, TimeSpan.FromMilliseconds(450));
        }

        /// <summary> A boss breaking up, the biggest single event in a level </summary>
        public static void BossDestroyed()
        {
            ScreenShakeController.Instance.Shake(34f, TimeSpan.FromMilliseconds(600));
        }

        /// <summary>
        /// The player's ship exploding. Lighter than the two above on purpose: it is already the
        /// most legible moment in the game, and it is the one the player is least able to
        /// afford being unable to see through.
        /// </summary>
        public static void PlayerDeath()
        {
            ScreenShakeController.Instance.Shake(20f, TimeSpan.FromMilliseconds(400));
        }
    }
}
