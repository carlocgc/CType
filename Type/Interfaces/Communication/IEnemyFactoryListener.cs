using System;
using System.Collections.Generic;
using System.Text;
using Type.Factories;

namespace Type.Interfaces.Communication
{
    /// <summary>
    /// Interface for objects that listen to the <see cref="EnemyFactory"/>
    /// </summary>
    public interface IEnemyFactoryListener
    {
        /// <summary>
        /// Invoked when the factory begins creating enemies
        /// </summary>
        void OnFactoryStarted();

        /// <summary>
        /// Invoked when a wave has been created
        /// </summary>
        void OnWaveCreated();

        /// <summary>
        /// Invoked when all enemies in a level have spawned
        /// </summary>
        void OnAllWavesCreated();
    }
}
