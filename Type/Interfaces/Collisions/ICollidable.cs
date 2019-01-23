using System;
using AmosShared.Interfaces;
using OpenTK;

namespace Type.Interfaces.Collisions
{
    /// <summary>
    /// Object that can collide with another <see cref="ICollidable"/>
    /// </summary>
    public interface ICollidable : IPositionable, IHitable, IDestroyable, IDisposable
    {
        /// <summary>
        /// The hitbox of the <see cref="ICollidable"/>
        /// </summary>
        Vector4 HitBox { get; set; }
    }
}
