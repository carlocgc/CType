using AmosShared.Interfaces;
using OpenTK;
using System;

namespace Type.Interfaces.Powerups
{
    /// <summary>
    /// Interface for a factory that creates <see cref="IPowerup"/>'s
    /// </summary>
    public interface IPowerupFactory : IUpdatable
    {
        /// <summary>
        /// Creates a powerup
        /// </summary>
        /// <param name="weight"> Weight category </param>
        /// <param name="position"> Position to spawn powerup </param>
        void Create(Int32 weight, Vector2 position);
    }
}
