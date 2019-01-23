using OpenTK;
using System;
using Type.Interfaces.Collisions;
using Type.Interfaces.Weapons;

namespace Type.Interfaces.Probe
{
    /// <summary>
    /// Object that is attached to the player
    /// </summary>
    public interface IProbe : ICollidable, IHitable, IDestroyable, IDisposable, IProjectileShooter
    {
        /// <summary>
        /// Update position data
        /// </summary>
        /// <param name="position"></param>
        void UpdatePosition(Vector2 position);
    }
}
