using OpenTK;
using System;
using System.Collections.Generic;

namespace Type.Data
{
    /// <summary>
    /// The bosses, and what makes each one different from the others.
    /// </summary>
    /// <remarks>
    /// The same shape as <see cref="EnemyDefinitions"/>, and keyed the same way: by the ids the
    /// level files already use, so <c>type=20</c> through <c>type=23</c> mean what they always
    /// meant. The bosses arrive on levels 5, 10, 15 and 20 in that order.
    /// <para>
    /// **The campaign is two hulls, each in a weak and a strong version.** That was true before
    /// and invisible, because it was spread across four files: the Strong variants reuse their
    /// base's cannon offsets exactly and bump each gun one tier - 50 to 75, 75 to 100, 100 to 125
    /// hit points - while shaving the fire rate. Seeing that is the point of putting the four
    /// side by side.
    /// </para>
    /// <para>
    /// A table in code rather than a data file, for the reason given on
    /// <see cref="EnemyDefinitions"/>: L1 is the item that builds a validated data format, and
    /// until it exists the compiler checks this and would not check a file.
    /// </para>
    /// </remarks>
    public static class BossDefinitions
    {
        /// <summary> Time between shots for the lightest gun on a weak hull </summary>
        private static readonly TimeSpan Slow = TimeSpan.FromMilliseconds(1500);
        /// <summary> Time between shots for the middle gun on a weak hull </summary>
        private static readonly TimeSpan Medium = TimeSpan.FromMilliseconds(1200);
        /// <summary> Time between shots for the heaviest gun, which is the same on both hulls </summary>
        private static readonly TimeSpan Fast = TimeSpan.FromMilliseconds(1000);
        /// <summary> The lightest gun on a strong hull </summary>
        private static readonly TimeSpan SlowStrong = TimeSpan.FromMilliseconds(1400);
        /// <summary> The middle gun on a strong hull </summary>
        private static readonly TimeSpan MediumStrong = TimeSpan.FromMilliseconds(1100);

        /// <summary> The definition for each boss type id used by the level data </summary>
        private static readonly Dictionary<Int32, BossDefinition> _Definitions = new Dictionary<Int32, BossDefinition>
        {
            // 20 - the fighter, level 5. Five guns down the hull, heaviest at the front.
            {
                20, new BossDefinition("Content/Graphics/Bosses/boss01.png",
                    new BossCannonDefinition(new Vector2(113, -200), 50, Slow),
                    new BossCannonDefinition(new Vector2(102, -130), 75, Medium),
                    new BossCannonDefinition(new Vector2(-149, 0), 100, Fast),
                    new BossCannonDefinition(new Vector2(102, 130), 75, Medium),
                    new BossCannonDefinition(new Vector2(113, 200), 50, Slow))
            },
            // 21 - the station, level 10. Seven guns in two rows, so it takes longer to strip.
            {
                21, new BossDefinition("Content/Graphics/Bosses/boss02.png",
                    new BossCannonDefinition(new Vector2(250, 195), 50, Slow),
                    new BossCannonDefinition(new Vector2(-205, 195), 50, Slow),
                    new BossCannonDefinition(new Vector2(10, 195), 75, Medium),
                    new BossCannonDefinition(new Vector2(8, 60), 100, Fast),
                    new BossCannonDefinition(new Vector2(10, -110), 75, Medium),
                    new BossCannonDefinition(new Vector2(250, -110), 50, Slow),
                    new BossCannonDefinition(new Vector2(-205, -110), 50, Slow))
            },
            // 22 - the fighter again, level 15. Same hull layout, every gun one tier up.
            {
                22, new BossDefinition("Content/Graphics/Bosses/boss03.png",
                    new BossCannonDefinition(new Vector2(113, -200), 75, SlowStrong),
                    new BossCannonDefinition(new Vector2(102, -130), 100, MediumStrong),
                    new BossCannonDefinition(new Vector2(-149, 0), 125, Fast),
                    new BossCannonDefinition(new Vector2(102, 130), 100, MediumStrong),
                    new BossCannonDefinition(new Vector2(113, 200), 75, SlowStrong))
            },
            // 23 - the station again, level 20. The last fight in the game.
            {
                23, new BossDefinition("Content/Graphics/Bosses/boss04.png",
                    new BossCannonDefinition(new Vector2(250, 195), 75, SlowStrong),
                    new BossCannonDefinition(new Vector2(-205, 195), 75, SlowStrong),
                    new BossCannonDefinition(new Vector2(10, 195), 100, MediumStrong),
                    new BossCannonDefinition(new Vector2(8, 60), 125, Fast),
                    new BossCannonDefinition(new Vector2(10, -110), 100, MediumStrong),
                    new BossCannonDefinition(new Vector2(250, -110), 75, SlowStrong),
                    new BossCannonDefinition(new Vector2(-205, -110), 75, SlowStrong))
            },
        };

        /// <summary>
        /// Gets the definition for the given boss type id
        /// </summary>
        /// <param name="type"> The enemy type id from the level data </param>
        /// <returns> The definition for that type </returns>
        /// <exception cref="ArgumentOutOfRangeException"> The id is not a boss </exception>
        public static BossDefinition Get(Int32 type)
        {
            if (_Definitions.TryGetValue(type, out BossDefinition definition)) return definition;

            throw new ArgumentOutOfRangeException(nameof(type), type, "No boss definition for this type id");
        }

        /// <summary>
        /// Whether the given type id names a boss
        /// </summary>
        /// <param name="type"> The enemy type id from the level data </param>
        /// <returns> True if a definition exists for the id </returns>
        public static Boolean Exists(Int32 type)
        {
            return _Definitions.ContainsKey(type);
        }
    }
}
