using OpenTK;
using System;
using System.Collections.Generic;

namespace Type.Data
{
    /// <summary>
    /// The wave enemies, and what makes each one different from the others.
    /// </summary>
    /// <remarks>
    /// The same shape as <see cref="Rumble"/>, <see cref="Particles"/>, <see cref="Shake"/>,
    /// <see cref="HitStop"/>, <see cref="Sounds"/> and <see cref="Flash"/>, and for the same
    /// reason: the roster is a difficulty curve, and a curve is only visible when its numbers sit
    /// together. Two hit points and ten points at one end, six and a hundred and twenty five at
    /// the other, was previously spread across six files that had to be opened side by side to
    /// see it at all.
    /// <para>
    /// **Keyed by the id the level files already use.** <c>type=0</c> through <c>type=5</c> in
    /// <c>Assets/Levels</c> mean exactly what they meant before, so no level data changes. Boss
    /// ids 20 to 23 are deliberately absent - bosses are still bespoke classes, and E6 is what
    /// changes that.
    /// </para>
    /// <para>
    /// **A table in code rather than a data file, deliberately, and this is a departure from what
    /// E1 asked for.** The roadmap said "an <c>EnemyDefinition</c> loaded from a data file". Doing
    /// that now means inventing a file format, a parser, and asset registration in two csprojs
    /// ahead of L1, which is the item that replaces the unvalidated pipe-delimited level format
    /// with schema-validated JSON. Everything a definitions file would need, L1 has to build
    /// anyway. Until then a table in code is checked by the compiler, which the level files
    /// notably are not. **Moving these to the L1 format is the intended end state**, and nothing
    /// here is shaped to prevent it.
    /// </para>
    /// </remarks>
    public static class EnemyDefinitions
    {
        /// <summary> The definition for each enemy type id used by the level data </summary>
        private static readonly Dictionary<Int32, EnemyDefinition> _Definitions = new Dictionary<Int32, EnemyDefinition>
        {
            // Small, weak. The opening enemy: slowest gun, least reward, red shot.
            { 0, new EnemyDefinition("Content/Graphics/Enemies/enemy1.png", 2, 10, TimeSpan.FromSeconds(2f), 1000, new Vector4(100, 0, 0, 1), Sounds.EnemyShot) },
            // Medium, weak.
            { 1, new EnemyDefinition("Content/Graphics/Enemies/enemy2.png", 3, 25, TimeSpan.FromSeconds(1.7f), 1050, new Vector4(100, 100, 0, 1), Sounds.EnemyShot) },
            // Large, weak. The first to use the heavier shot sound.
            { 2, new EnemyDefinition("Content/Graphics/Enemies/enemy4.png", 5, 50, TimeSpan.FromSeconds(1.4f), 1100, new Vector4(100, 0, 100, 1), Sounds.LargeEnemyShot) },
            // Small, strong. Fewer hit points than a large but worth more, because it is the
            // fastest gun on the smallest target.
            { 3, new EnemyDefinition("Content/Graphics/Enemies/enemy5.png", 3, 75, TimeSpan.FromSeconds(1.1f), 1000, new Vector4(100, 0, 0, 1), Sounds.EnemyShot) },
            // Medium, strong.
            { 4, new EnemyDefinition("Content/Graphics/Enemies/enemy6.png", 4, 100, TimeSpan.FromSeconds(1f), 1050, new Vector4(100, 100, 0, 1), Sounds.EnemyShot) },
            // Large, strong. The hardest wave enemy in the game.
            { 5, new EnemyDefinition("Content/Graphics/Enemies/enemy8.png", 6, 125, TimeSpan.FromSeconds(1.2f), 1100, new Vector4(100, 0, 100, 1), Sounds.LargeEnemyShot) },
        };

        /// <summary>
        /// Gets the definition for the given enemy type id
        /// </summary>
        /// <param name="type"> The enemy type id from the level data </param>
        /// <returns> The definition for that type </returns>
        /// <exception cref="ArgumentOutOfRangeException"> The id is not a wave enemy </exception>
        public static EnemyDefinition Get(Int32 type)
        {
            if (_Definitions.TryGetValue(type, out EnemyDefinition definition)) return definition;

            throw new ArgumentOutOfRangeException(nameof(type), type, "No enemy definition for this type id");
        }
    }
}
