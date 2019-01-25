using System;
using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.Weapons;

namespace Type.Interfaces.Player
{
    /// <summary>
    /// Interface for the player ship
    /// </summary>
    public interface IPlayer : ISpawnable, ICollidable, IProjectileShooter, IUIListener, INotifier<IPlayerListener>
    {
        /// <summary>
        /// Increase the shield level on the player
        /// </summary>
        void AddShield();

        /// <summary>
        /// Add a probe to the player
        /// </summary>
        /// <param name="id"> The type of probe to add </param>
        void AddProbe(Int32 id);

        /// <summary>
        /// Apply a power of the given type
        /// </summary>
        /// <param name="id"> The type of powerup </param>
        void ApplyPowerup(Int32 id);
    }
}
