using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

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
        public TimeSpan SpawnInterval;
        /// <summary> List of positions to spawn the ships </summary>
        public Vector2[] SpawnPositions;
        /// <summary> What type of ships comprise the wave   </summary>
        public Int32 EnemyType;

        public WaveData(TimeSpan interval, Int32 enemyType, Vector2[] spawnPosList)
        {
            ShipCount = spawnPosList.Length;
            SpawnInterval = interval;
            EnemyType = enemyType;
            SpawnPositions = spawnPosList;
        }
    }

}
