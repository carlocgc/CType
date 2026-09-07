using System;
using Type.Controllers;

namespace Type.Data
{
    /// <summary>
    /// The events that slow the clock, and by how much.
    /// </summary>
    /// <remarks>
    /// Named events rather than numbers at the call sites, the same shape as <see cref="Rumble"/>
    /// and <see cref="Shake"/> and for the same reason.
    /// <para>
    /// **A shorter list than the shake table, and that is the point.** Hit stop takes the
    /// controls away for as long as it lasts, so it is spent only where the player has nothing
    /// to do anyway: the frame a boss dies, and the frame they do. Putting one on every kill
    /// would make a firefight feel like a bad connection.
    /// </para>
    /// <para>
    /// Durations are wall clock, not game time, so they are the length they say they are
    /// regardless of how far the clock has been slowed. **These values have not been tuned by
    /// feel** — see G3 in ROADMAP.md.
    /// </para>
    /// </remarks>
    public static class HitStop
    {
        /// <summary> A boss breaking up </summary>
        public static void BossDestroyed()
        {
            TimeScaleController.Instance.HitStop(0.15f, TimeSpan.FromMilliseconds(180));
        }

        /// <summary>
        /// The player's ship exploding. Shorter and less severe than the boss, because a death
        /// is followed by a respawn the player is waiting on rather than by a reward.
        /// </summary>
        public static void PlayerDeath()
        {
            TimeScaleController.Instance.HitStop(0.25f, TimeSpan.FromMilliseconds(140));
        }
    }
}
