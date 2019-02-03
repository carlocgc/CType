using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
