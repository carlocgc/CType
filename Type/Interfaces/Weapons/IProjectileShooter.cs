using System;

namespace Type.Interfaces.Weapons
{
    /// <summary>
    /// Object that implements this interface can shoot a <see cref="IProjectile"/>
    /// </summary>
    public interface IProjectileShooter
    {
        /// <summary>
        /// Whether this object is Autofiring
        /// </summary>
        Boolean AutoFire { get; set; }

        /// <summary>
        /// Shoot a projectile
        /// </summary>
        void Shoot();
    }
}
