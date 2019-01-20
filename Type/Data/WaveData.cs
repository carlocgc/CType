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
        public Int32 ShipCount;
        /// <summary> How long between each spawn </summary>
        public TimeSpan[] SpawnInterval;
        /// <summary> Array of positions to spawn the ships </summary>
        public Vector2[] SpawnPositions;
        /// <summary> Array of ship types </summary>
        public Int32[] EnemyTypes;

        /// <summary>
        /// Data for a wave of enemies
        /// </summary>
        /// <param name="interval"> Interval to spawn each ship </param>
        /// <param name="enemyTypes"> Array of ship types </param>
        /// <param name="spawnPositions"> Array of positions to spawn the ships </param>
        public WaveData(TimeSpan[] interval, Int32[] enemyTypes, Vector2[] spawnPositions)
        {
            ShipCount = spawnPositions.Length;
            SpawnInterval = interval;
            EnemyTypes = enemyTypes;
            SpawnPositions = spawnPositions;
        }
    }

}
