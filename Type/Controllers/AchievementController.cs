using AmosShared.Competitive;
using System;
using Type.Data;

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
                case 2:
                    {
                        if (GameStats.Instance.BulletsFired == 0) Reflexes();
                        break;
                    }
                case 3:
                    {
                        // ACHIEVEMENT : Level 3 complete
                        CompetitiveManager.Instance.SetAchievementProgress(0, 1);
                        break;
                    }
                case 5:
                    {
                        // ACHIEVEMENT : Level 5 complete
                        CompetitiveManager.Instance.SetAchievementProgress(1, 1);
                        break;
                    }
                case 7:
                    {
                        // ACHIEVEMENT : Level 7 complete
                        CompetitiveManager.Instance.SetAchievementProgress(2, 1);
                        break;
                    }
                case 10:
                    {
                        // ACHIEVEMENT : Level 10 complete
                        CompetitiveManager.Instance.SetAchievementProgress(3, 1);
                        break;
                    }
                case 13:
                    {
                        // ACHIEVEMENT : Level 13 complete
                        CompetitiveManager.Instance.SetAchievementProgress(4, 1);
                        break;
                    }
                case 15:
                    {
                        // ACHIEVEMENT : Level 15 complete
                        CompetitiveManager.Instance.SetAchievementProgress(5, 1);
                        break;
                    }
                case 17:
                    {
                        // ACHIEVEMENT : Level 17 complete
                        CompetitiveManager.Instance.SetAchievementProgress(6, 1);
                        break;
                    }
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
                        // ACHIEVEMENT : Alpha victor
                        CompetitiveManager.Instance.SetAchievementProgress(7, 1);
                        break;
                    }
                case 1:
                    {
                        // ACHIEVEMENT : Beta victor
                        CompetitiveManager.Instance.SetAchievementProgress(8, 1);
                        break;
                    }
                case 2:
                    {
                        // ACHIEVEMENT : Gamma victor
                        CompetitiveManager.Instance.SetAchievementProgress(9, 1);
                        break;
                    }
            }
        }

        /// <summary>
        /// Updates the prototype achievement
        /// </summary>
        public void PrototypeFound()
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            // ACHIEVEMENT :  Prototype
            CompetitiveManager.Instance.SetAchievementProgress(10, 1);
        }

        /// <summary>
        /// Updates the reflexes achievement
        /// </summary>
        private void Reflexes()
        {
            if (!CompetitiveManager.Instance.Loaded) return;
            // ACHIEVEMENT :  Reflexes
            CompetitiveManager.Instance.SetAchievementProgress(11, 1);
        }

        /// <summary>
        /// Update all time score achievement
        /// </summary>
        /// <param name="allTimeScore"></param>
        public void AllTimeScoreUpdated(Int64 allTimeScore)
        {
            if (!CompetitiveManager.Instance.Loaded) return;

            Int64 mil = 1000000;
            Single percentage = 0;

            if (allTimeScore >= mil) percentage = 1;
            else percentage = (Single)allTimeScore / mil;

            if (percentage >= 1)
            {
                // ACHIEVEMENT :  Score one million
                CompetitiveManager.Instance.SetAchievementProgress(12, 1);
            }
        }
    }
}
