using OpenTK;
using System;

namespace Type.Data
{
    /// <summary>
    /// Data for a ship wave
    /// </summary>
    public struct WaveData
    {
        /// <summary> Amount of ships in the wave </summary>
        public readonly Int32 ShipCount;
        /// <summary> How long between each spawn </summary>
        public readonly TimeSpan[] SpawnInterval;
        /// <summary> Array of positions to spawn the ships </summary>
        public readonly Single[] Ypositions;
        /// <summary> Array of ship types </summary>
        public readonly Int32[] EnemyTypes;
        /// <summary> Movement controller types </summary>
        public readonly Int32[] MovementTypes;
        /// <summary> Directions of each ship in the wave </summary>
        public readonly Vector2[] MoveDirections;
        /// <summary> Movement speeds of each ship in the wave </summary>
        public readonly Single[] MovementSpeeds;

        /// <summary>
        /// Data for a wave of enemies
        /// </summary>
        /// <param name="interval"> Interval to spawn each ship </param>
        /// <param name="enemyTypes"> Array of ship types </param>
        /// <param name="yPositions"> Array of positions to spawn the ships </param>
        /// <param name="moveTypes"> Movement controller types </param>
        /// <param name="directions"> Directions of each ship in the wave </param>
        /// <param name="speeds"> Movement speeds of each ship in the wave </param>
        public WaveData(TimeSpan[] interval, Int32[] enemyTypes, Single[] yPositions, Int32[] moveTypes, Vector2[] directions, Single[] speeds)
        {
            ShipCount = yPositions.Length;
            SpawnInterval = interval;
            EnemyTypes = enemyTypes;
            Ypositions = yPositions;
            MovementTypes = moveTypes;
            MoveDirections = directions;
            MovementSpeeds = speeds;
        }
    }

}
