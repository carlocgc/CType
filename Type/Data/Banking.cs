using System;

namespace Type.Data
{
    /// <summary>
    /// How far the player's ship rolls as it climbs and dives, and how quickly it gets there.
    /// </summary>
    /// <remarks>
    /// **A roll, not a turn.** The ship is drawn from above with its wings across the screen, so
    /// rolling about the fuselage swings one wing towards the viewer and the other away, and what
    /// that does to a flat sprite is foreshorten it vertically. This returns that foreshortening
    /// as a vertical scale: rotating the sprite instead reads as the ship steering, which is a
    /// different manoeuvre and the wrong one.
    /// <para>
    /// **It cannot say which wing is nearer, and nothing here can.** The far wing should be
    /// smaller than the near one, which is a shear the engine has no way to express, and there
    /// is no banked art to swap to. So the squash is symmetric: climbing and diving look alike.
    /// Banked frames for the four ships would fix it properly.
    /// </para>
    /// <para>
    /// Eased on a rate rather than a fixed step, and scaled by how far the stick is pushed, so a
    /// light analog nudge gets a little of it. **Not judged by eye** - see G6 in ROADMAP.md.
    /// </para>
    /// </remarks>
    public static class Banking
    {
        /// <summary>
        /// How much of the wingspan is left at full deflection. 0.68 is a roll of about
        /// forty-seven degrees, which is enough to read without the ship looking damaged.
        /// </summary>
        private const Single MaxSquash = 0.68f;

        /// <summary> How quickly the ship reaches the roll it is heading for, per second </summary>
        private const Single Rate = 8f;

        /// <summary>
        /// Moves a roll towards the one the current input asks for
        /// </summary>
        /// <param name="current"> The ship's vertical scale now, 1 being level </param>
        /// <param name="vertical"> Vertical input, -1 to 1, already scaled by how hard it is pushed </param>
        /// <param name="timeTilUpdate"> Time since the last update </param>
        /// <returns> The vertical scale to draw the ship at this frame </returns>
        public static Single Toward(Single current, Single vertical, TimeSpan timeTilUpdate)
        {
            Single amount = vertical < 0 ? -vertical : vertical;
            if (amount > 1) amount = 1;

            Single target = 1f - (1f - MaxSquash) * amount;
            Single step = Rate * (Single)timeTilUpdate.TotalSeconds;

            // Clamped so a long frame cannot overshoot the target and oscillate around it.
            if (step > 1) step = 1;

            return current + (target - current) * step;
        }
    }
}
