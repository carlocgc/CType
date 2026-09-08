using OpenTK;
using System;

namespace Type.Data
{
    /// <summary>
    /// Everything that makes one wave enemy different from another.
    /// </summary>
    /// <remarks>
    /// There used to be six classes here - <c>SmallEnemyWeak</c> through <c>LargeEnemyStrong</c> -
    /// at 228 lines each, and a diff of any two of them was these seven values and nothing else.
    /// Every behaviour change had to be made six times identically; the last one to do that was
    /// G6, which deleted the same 32 line block from all six files.
    /// <para>
    /// A definition is immutable and shared. One instance per type serves every enemy of that
    /// type alive at once, so nothing here may be per-instance state - that lives on
    /// <see cref="Objects.Enemies.Enemy"/>.
    /// </para>
    /// </remarks>
    public sealed class EnemyDefinition
    {
        /// <summary> Texture drawn for this enemy, as an output path under <c>Content/</c> </summary>
        public String Texture { get; }

        /// <summary> Damage the enemy absorbs before it is destroyed </summary>
        public Int32 HitPoints { get; }

        /// <summary> Score awarded for killing it </summary>
        public Int32 Points { get; }

        /// <summary> Time between shots </summary>
        public TimeSpan FireRate { get; }

        /// <summary> Speed of the plasma ball this enemy fires </summary>
        public Single ProjectileSpeed { get; }

        /// <summary> Colour of the plasma ball this enemy fires </summary>
        public Vector4 ProjectileColour { get; }

        /// <summary>
        /// The noise this enemy makes when it fires.
        /// </summary>
        /// <remarks>
        /// An <see cref="Action"/> into <see cref="Sounds"/> rather than a file path, so
        /// <see cref="Sounds"/> stays the only place that names an audio file and sets its rate
        /// limit. G4 made that a rule; a path here would put a second one beside it.
        /// </remarks>
        public Action ShotSound { get; }

        /// <summary>
        /// Creates a new <see cref="EnemyDefinition"/>
        /// </summary>
        /// <param name="texture"> Texture drawn for this enemy </param>
        /// <param name="hitPoints"> Damage absorbed before it is destroyed </param>
        /// <param name="points"> Score awarded for killing it </param>
        /// <param name="fireRate"> Time between shots </param>
        /// <param name="projectileSpeed"> Speed of the plasma ball fired </param>
        /// <param name="projectileColour"> Colour of the plasma ball fired </param>
        /// <param name="shotSound"> The noise made when firing </param>
        public EnemyDefinition(String texture, Int32 hitPoints, Int32 points, TimeSpan fireRate, Single projectileSpeed, Vector4 projectileColour, Action shotSound)
        {
            Texture = texture;
            HitPoints = hitPoints;
            Points = points;
            FireRate = fireRate;
            ProjectileSpeed = projectileSpeed;
            ProjectileColour = projectileColour;
            ShotSound = shotSound;
        }
    }
}
