using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Enemies
{
    /// <summary> Interface for listener of the enemy factory</summary>
    public interface IEnemyFactoryListener
    {
        /// <summary>
        /// Invoked when an enemy is created
        /// </summary>
        /// <param name="enemy"> The enemy tha was created </param>
        void EnemyCreated(IEnemy enemy);
    }
}
