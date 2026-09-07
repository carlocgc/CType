using System;
using Type.Controllers;

namespace Type.Data
{
    /// <summary>
    /// The events that white the screen out, and how hard each one hits.
    /// </summary>
    /// <remarks>
    /// Named events rather than numbers at the call sites, the same shape as
    /// <see cref="Rumble"/>, <see cref="Particles"/>, <see cref="Shake"/>, <see cref="HitStop"/>
    /// and <see cref="Sounds"/>.
    /// <para>
    /// A short list on purpose. A flash is the bluntest thing the game can do to the player's
    /// eyes, so it is spent only on a boss coming apart - the individual blasts stay well under
    /// half opacity, and only the final one is allowed to be bright.
    /// </para>
    /// <para>
    /// **These values have not been judged by eye** - see G6 in ROADMAP.md.
    /// </para>
    /// </remarks>
    public static class Flash
    {
        /// <summary> One of the blasts running across a dying boss </summary>
        public static void BossDeathBlast()
        {
            ScreenFlashController.Instance.Flash(0.28f, TimeSpan.FromMilliseconds(140));
        }

        /// <summary> The blast that finishes a boss off </summary>
        public static void BossDeathFinal()
        {
            ScreenFlashController.Instance.Flash(0.85f, TimeSpan.FromMilliseconds(420));
        }
    }
}
