using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Player
{
    /// <summary>
    /// Object that can spawn on screen
    /// </summary>
    public interface ISpawnable
    {
        /// <summary>
        /// Spawn the object
        /// </summary>
        void Spawn();
    }
}
