using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Powerups
{
    /// <summary>
    /// Interface for objects listening to <see cref="IPowerup"/>'s
    /// </summary>
    public interface IPowerupListener
    {
        /// <summary>
        /// Invoked when a powerup is applied
        /// </summary>
        /// <param name="ID"> The type of powerup applied </param>
        void OnPowerupApplied(Int32 ID);

        /// <summary>
        /// Invoked when a power up expires
        /// </summary>
        void OnPowerupExpired();
    }
}
