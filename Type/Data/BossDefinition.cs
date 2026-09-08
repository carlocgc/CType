using System;
using System.Collections.Generic;

namespace Type.Data
{
    /// <summary>
    /// Everything that makes one boss different from another.
    /// </summary>
    /// <remarks>
    /// There used to be four classes - <c>BossFighter</c>, <c>BossFighterStrong</c>,
    /// <c>BossStation</c> and <c>BossStationStrong</c> - at 273 to 278 lines each, and stripped of
    /// whitespace they were the same file twice over. They differed in the body texture and the
    /// cannon list and in nothing else: points, speed, move direction and stop position were
    /// identical in all four.
    /// <para>
    /// A definition is immutable and shared, so nothing here may be per-instance state - that
    /// lives on <see cref="Objects.Bosses.Boss"/>.
    /// </para>
    /// </remarks>
    public sealed class BossDefinition
    {
        /// <summary> Texture drawn for the boss body, as an output path under <c>Content/</c> </summary>
        public String Texture { get; }

        /// <summary> The guns on this boss. Killing the last one starts the death sequence </summary>
        public IReadOnlyList<BossCannonDefinition> Cannons { get; }

        /// <summary>
        /// Creates a new <see cref="BossDefinition"/>
        /// </summary>
        /// <param name="texture"> Texture drawn for the boss body </param>
        /// <param name="cannons"> The guns on this boss </param>
        public BossDefinition(String texture, params BossCannonDefinition[] cannons)
        {
            Texture = texture;
            Cannons = cannons;
        }
    }
}
