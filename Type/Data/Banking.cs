using System;

namespace Type.Data
{
    /// <summary>
    /// How far the player's ship tips as it climbs and dives, and how quickly it gets there.
    /// </summary>
    /// <remarks>
    /// The ship art is a single sprite drawn side on, with no banked frames to swap to, so the
    /// bank is a rotation of that one sprite. Small on purpose: past about fifteen degrees a flat
    /// side view stops reading as a ship leaning and starts reading as a ship pointing the wrong
    /// way, because none of the perspective the art would need is there.
    /// <para>
    /// Eased rather than snapped, and eased on a rate rather than a fixed step, so the tip is
    /// proportional to how far the stick is pushed. A digital key press reaches full deflection
    /// and gets the full angle; a light analog nudge gets a little of it, which is most of what
    /// makes this read as flying rather than as an animation being triggered.
    /// </para>
    /// <para>
    /// **These values have not been judged by eye** - see G6 in ROADMAP.md.
    /// </para>
    /// </remarks>
    public static class Banking
    {
        /// <summary> Furthest the ship tips, in radians, at full deflection </summary>
        private const Single MaxAngle = 0.20f;

        /// <summary> How quickly the ship reaches the angle it is heading for, per second </summary>
        private const Single Rate = 8f;

        /// <summary>
        /// Moves a rotation towards the one the current input asks for
        /// </summary>
        /// <param name="current"> The ship's rotation now, in radians </param>
        /// <param name="vertical"> Vertical input, -1 to 1, already scaled by how hard it is pushed </param>
        /// <param name="timeTilUpdate"> Time since the last update </param>
        /// <returns> The rotation to use this frame </returns>
        public static Double Toward(Double current, Single vertical, TimeSpan timeTilUpdate)
        {
            Double target = vertical * MaxAngle;
            Double step = Rate * timeTilUpdate.TotalSeconds;

            // Clamped so a long frame cannot overshoot the target and oscillate around it.
            if (step > 1) step = 1;

            return current + (target - current) * step;
        }
    }
}
