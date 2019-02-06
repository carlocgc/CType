using AmosShared.Competitive;
using System;

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
                case 3:
                    {
                        if (CompetitiveManager.Instance.GetAchievement(0).PercentageComplete < 1)
                        {
                            // ACHIEVEMENT : Level 3 complete
                            CompetitiveManager.Instance.SetAchievementProgress(0, 1);
                        }

                        break;
                    }
                case 7:
                    {
                        if (CompetitiveManager.Instance.GetAchievement(1).PercentageComplete < 1)
                        {
                            // ACHIEVEMENT : Level 7 complete
                            CompetitiveManager.Instance.SetAchievementProgress(1, 1);
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
            if (!CompetitiveManager.Instance.Loaded) return;

            Int64 mil = 1000000000;
            Single percentage = 0;

            if (allTimeScore >= mil) percentage = 1;
            else percentage = (Single)allTimeScore / mil;

            if (CompetitiveManager.Instance.GetAchievement(7).PercentageComplete < 1 && percentage >= 1)
            {
                // ACHIEVEMENT :  Score one million
                CompetitiveManager.Instance.SetAchievementProgress(7, 1);
            }
        }

        /// <summary>
        /// Updates the prototype achievement
        /// </summary>
        public void PrototypeFound()
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            if (CompetitiveManager.Instance.GetAchievement(6).PercentageComplete < 1)
            {
                // ACHIEVEMENT :  Prototype
                CompetitiveManager.Instance.SetAchievementProgress(6, 1);
            }
        }

        /// <summary>
        /// Updates game complete achievements
        /// </summary>
        /// <param name="playerShipId"></param>
        public void GameComplete(Int32 playerShipId)
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            switch (playerShipId)
            {
                case 0:
                    {
                        if (CompetitiveManager.Instance.GetAchievement(3).PercentageComplete < 1)
                        {
                            // ACHIEVEMENT : Alpha victor
                            CompetitiveManager.Instance.SetAchievementProgress(3, 1);
                        }

                        break;
                    }
                case 1:
                    {
                        if (CompetitiveManager.Instance.GetAchievement(4).PercentageComplete < 1)
                        {
                            // ACHIEVEMENT : Beta victor
                            CompetitiveManager.Instance.SetAchievementProgress(4, 1);
                        }

                        break;
                    }
                case 2:
                    {
                        if (CompetitiveManager.Instance.GetAchievement(5).PercentageComplete < 1)
                        {
                            // ACHIEVEMENT : Gamma victor
                            CompetitiveManager.Instance.SetAchievementProgress(5, 1);
                        }

                        break;
                    }
            }
        }
    }
}

