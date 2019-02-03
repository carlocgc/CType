using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Base;
using AmosShared.Competitive;

namespace Type.Controllers
{
    /// <summary>
    /// Service for setting the progress of game achievements
    /// </summary>
    public class AchievementController
    {
        /// <summary> The instance of the AchievementController </summary>
        private static AchievementController _Instance;

        /// <summary> The instance of the AchievementController </summary>
        public static AchievementController Instance => _Instance ?? (_Instance = new AchievementController());

        private AchievementController()
        {

        }

        /// <summary>
        /// Checks level complete acheivements
        /// </summary>
        /// <param name="level"></param>
        public void LevelCompleted(Int32 level)
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            switch (level)
            {
                case 1:
                    {
                        if (CompetitiveManager.Instance.GetAchievement(0).PercentageComplete < 1)
                        {
                            // Level 1 complete
                            CompetitiveManager.Instance.SetAchievementProgress(0, 1);
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Update all time score achievement
        /// </summary>
        /// <param name="allTimeScore"></param>
        public void AllTimeScoreUpdated(Int64 allTimeScore)
        {
            Int64 mil = 1000000000;
            Single percentage = 0;

            if (allTimeScore >= mil) percentage = 1;
            else
            {
                percentage = allTimeScore / mil;
            }

            if (!CompetitiveManager.Instance.Loaded) return;
            
            if (CompetitiveManager.Instance.GetAchievement(0).PercentageComplete < 1)
            {
                // Level 1 complete
                CompetitiveManager.Instance.SetAchievementProgress(1, percentage);
            }
        }
    }
}

