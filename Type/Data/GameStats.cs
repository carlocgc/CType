using AmosShared.Interfaces;
using System;

namespace Type.Data
{
    /// <summary>
    /// Stores the game data to be displayed in the results screen
    /// </summary>
    public class GameStats
    {
        private static GameStats _Instance;

        public static GameStats Instance
        {
            get
            {
                if (_Instance == null) return _Instance = new GameStats();
                else { return _Instance; }
            }
        }

        /// <summary> Time the game started </summary>
        private DateTime _StartTime;
        /// <summary> Time the game ended</summary>
        private DateTime _EndTime;

        /// <summary> How many shots the player fired </summary>
        public Int32 BulletsFired { get; set; }
        /// <summary> How many probes the player created </summary>
        public Int32 ProbesCreated { get; set; }
        /// <summary> How many shields the player created </summary>
        public Int32 ShieldsCreated { get; set; }
        /// <summary> How many times the player died </summary>
        public Int32 Deaths { get; set; }
        /// <summary> How many enemies the player has killed </summary>
        public Int32 EnemiesKilled { get; set; }
        /// <summary> Player score </summary>
        public Int32 Score { get; set; }

        /// <summary> Total game time </summary>
        public TimeSpan PlayTime => _EndTime - _StartTime;

        /// <summary>
        /// Sets the game start time
        /// </summary>
        public void GameStart()
        {
            _StartTime = DateTime.Now;
        }

        /// <summary>
        /// Sets the game end time
        /// </summary>
        public void GameEnd()
        {
            _EndTime = DateTime.Now;
        }

        public void Clear()
        {
            BulletsFired = 0;
            ProbesCreated = 0;
            ShieldsCreated = 0;
            EnemiesKilled = 0;
            Deaths = 0;
            GameStart();
            GameEnd();
        }
    }
}
