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
            if (!CompetitiveManager.Instance.Loaded) return;

            // Update Highscore board
            CompetitiveManager.Instance.UpdateLeaderboardProgress(0, score);
        }

        public void KillsUpdated(Int32 kills)
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            // Update kills board
            CompetitiveManager.Instance.UpdateLeaderboardProgress(1, kills);
        }

        public void AllTimeScoreUpdated(Int32 alltimeScore)
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            // Update alltime score board
            CompetitiveManager.Instance.UpdateLeaderboardProgress(2, alltimeScore);
        }

        public void AllTimeKillsUpdated(Int32 alltimeKills)
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            // Update alltime kills board
            CompetitiveManager.Instance.UpdateLeaderboardProgress(3, alltimeKills);
        }
    }
}
