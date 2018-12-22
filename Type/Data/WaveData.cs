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

        public WaveData(TimeSpan interval, Int32 enemyType, Vector2[] spawnPosList = null)
        {
            ShipCount = spawnPosList.Length;
            SpawnInterval = interval;
            EnemyType = enemyType;
            if (spawnPosList == null)
            {
                SpawnPositions = new[]
                {
                        new Vector2(1100, 130),
                        new Vector2(1100, -210),
                        new Vector2(1100, 300),
                        new Vector2(1100, -140),
                        new Vector2(1100, 240),
                        new Vector2(1100, -270),
                        new Vector2(1100, 250)
                };
            }
            else
            {
                SpawnPositions = spawnPosList;
            }
        }
    }

}
