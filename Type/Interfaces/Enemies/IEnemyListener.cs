using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// Object that implements this interface will be notified of enemy events
    /// </summary>
    public interface IEnemyListener
    {
        /// <summary>
        /// Invoked when an enemy is Destroyed
        /// </summary>
        void OnEnemyDestroyed(IEnemy enemy);
    }
}
