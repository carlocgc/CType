using AmosShared.Interfaces;
using System;
using AmosShared.Base;
using Type.Controllers;

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
        /// <summary> The players current highscore, loaded from data store </summary>
        private Int32 _HighScore;
        /// <summary> Running total of all time score </summary>
        private Int32 _AllTimeScore;
        /// <summary> Enemies killed over all play thorughs </summary>
        private Int32 _AllTimeKills;

        public Boolean CanShowAds { get; private set; }
        /// <summary> Whether the current score is a new highscore </summary>
        public Boolean IsNewHighScore { get; private set; }
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
            Score = 0;
            _StartTime = DateTime.Now;
            _HighScore = Convert.ToInt32(DataLoader.GetValue("HIGH_SCORE"));
            _AllTimeScore = Convert.ToInt32(DataLoader.GetValue("ALLTIME_SCORE"));
            _AllTimeKills = Convert.ToInt32(DataLoader.GetValue("ALLTIME_KILLS"));
        }

        /// <summary>
        /// Sets the game end time, and saves score
        /// </summary>
        public void GameEnd()
        {
            CanShowAds = true;
            _EndTime = DateTime.Now;

            _AllTimeKills += EnemiesKilled;
            _AllTimeScore += Score;

            // Save highscore
            if (Score > _HighScore)
            {
                DataLoader.SetValue("HIGH_SCORE", Score);
                IsNewHighScore = true;
            }

            // Save all-time data
            DataLoader.SetValue("ALLTIME_SCORE", _AllTimeScore);
            DataLoader.SetValue("ALLTIME_KILLS", _AllTimeKills);

            // Check achievement progress
            AchievementController.Instance.AllTimeScoreUpdated(_AllTimeScore);

            // Push leaderboard data
            LeaderboardController.Instance.ScoreUpdated(Score);
            LeaderboardController.Instance.AllTimeScoreUpdated(_AllTimeScore);
            LeaderboardController.Instance.KillsUpdated(EnemiesKilled);
            LeaderboardController.Instance.AllTimeKillsUpdated(_AllTimeKills);
        }

        public void Clear()
        {
            BulletsFired = 0;
            ProbesCreated = 0;
            ShieldsCreated = 0;
            EnemiesKilled = 0;
            Deaths = 0;
            IsNewHighScore = false;
        }
    }
}
