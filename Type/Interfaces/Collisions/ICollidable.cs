using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Interfaces;
using OpenTK;

namespace Type.Interfaces.Collisions
{
    /// <summary>
    /// Object that can collide with another <see cref="ICollidable"/>
    /// </summary>
    public interface ICollidable : IPositionable
    {
        /// <summary>
        /// The hitbox of the <see cref="ICollidable"/>
        /// </summary>
        Vector4 HitBox { get; set; }
    }
}
