using System;
using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.Powerups;
using Type.Interfaces.Weapons;

namespace Type.Interfaces.Player
{
    /// <summary>
    /// Interface for the player ship
    /// </summary>
    public interface IPlayer : ISpawnable, ICollidable, IProjectileShooter, IUIListener, INotifier<IPlayerListener>
    {
        /// <summary>
        /// Current amount of probes the player has
        /// </summary>
        Int32 CurrentProbes { get; }

        /// <summary>
        /// Apply a power of the given type
        /// </summary>
        void ApplyPowerup(IPowerup powerup);
    }
}
