using System;
using AmosShared.Interfaces;
using Type.Interfaces.Collisions;

namespace Type.Interfaces.Weapons
{
    /// <summary>
    /// Object that inherits this interface can be shot from <see cref="IProjectileShooter"/>
    /// </summary>
    public interface IProjectile : ICollidable, IRotatable
    {
        /// <summary>
        /// Damage this projectile inflicts
        /// </summary>
        Int32 Damage { get; }
    }
}
