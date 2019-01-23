using System;
using Type.Interfaces.Weapons;

namespace Type.Interfaces
{
    /// <summary> Interface for objects that can be hit </summary>
    public interface IHitable
    {
        /// <summary>
        /// The hitpoints of the <see cref="IHitable"/>
        /// </summary>
        Int32 HitPoints { get; }

        /// <summary>
        /// Hit the <see cref="IHitable"/>
        /// </summary>
        void Hit(IProjectile projectile);
    }
}
