using OpenTK;
using System;

namespace Type.Data
{
    /// <summary>
    /// One gun on a boss: where it sits on the hull, how much it takes, and how fast it fires.
    /// </summary>
    /// <remarks>
    /// The cannon list is the whole of what makes one boss different from another, apart from the
    /// body texture - see <see cref="BossDefinition"/>. A boss dies when its last cannon does, so
    /// this table is also the fight's difficulty and its length.
    /// </remarks>
    public sealed class BossCannonDefinition
    {
        /// <summary> Position of the cannon relative to the centre of the boss </summary>
        public Vector2 Offset { get; }

        /// <summary> Damage the cannon absorbs before it is destroyed </summary>
        public Int32 HitPoints { get; }

        /// <summary> Time between shots </summary>
        public TimeSpan FireRate { get; }

        /// <summary>
        /// Creates a new <see cref="BossCannonDefinition"/>
        /// </summary>
        /// <param name="offset"> Position relative to the centre of the boss </param>
        /// <param name="hitPoints"> Damage absorbed before it is destroyed </param>
        /// <param name="fireRate"> Time between shots </param>
        public BossCannonDefinition(Vector2 offset, Int32 hitPoints, TimeSpan fireRate)
        {
            Offset = offset;
            HitPoints = hitPoints;
            FireRate = fireRate;
        }
    }
}
