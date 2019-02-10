using System;

namespace Type.Interfaces
{
    /// <summary> Interface for destroyable objects </summary>
    public interface IDestroyable
    {
        /// <summary>
        /// Whether the enemy is destroyed
        /// </summary>
        Boolean IsDestroyed { get; set; }

        /// <summary>
        /// Destroy the object
        /// </summary>
        void Destroy();
    }
}
