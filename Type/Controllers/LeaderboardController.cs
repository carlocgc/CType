using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Competitive;

namespace Type.Controllers
{
    /// <summary> Object that controls the setting of leaderboard scores </summary>
    public class LeaderboardController
    {
        /// <summary> The instance of the LeaderboardController </summary>
        private static LeaderboardController _Instance;
        /// <summary> The instance of the LeaderboardController </summary>
        public static LeaderboardController Instance => _Instance ?? (_Instance = new LeaderboardController());

        private LeaderboardController()
        {

        }

        public void ScoreUpdated(Int32 score)
        {
            //// Update Highscore board
            // CompetitiveManager.Instance.UpdateLeaderboardProgress(0, score);
        }
    }
}
