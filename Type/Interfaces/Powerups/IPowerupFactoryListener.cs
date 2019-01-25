using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Type.Interfaces.Powerups
{
    /// <summary>
    /// Interface for objects that listen to a <see cref="IPowerupFactory"/>
    /// </summary>
    public interface IPowerupFactoryListener
    {
        /// <summary>
        /// Invoked when a powerup is created
        /// </summary>
        void OnPowerupCreated(IPowerup powerup);
    }
}
