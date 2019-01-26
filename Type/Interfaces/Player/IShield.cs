using AmosShared.Interfaces;
using System;
using OpenTK;

namespace Type.Interfaces
{
    /// <summary>
    /// Shield that can absorb impacts and has multiple stages
    /// </summary>
    public interface IShield : IPositionable, IDisposable
    {
        /// <summary>
        /// Whether the shield is active
        /// </summary>
        Boolean IsActive { get; }

        /// <summary>
        /// Whether the shield is max level
        /// </summary>
        Boolean IsMaxLevel { get; }

        /// <summary>
        /// Increases the shields level
        /// </summary>
        void Increase();

        /// <summary>
        /// Decreases the shields level
        /// </summary>
        void Decrease(Int32 damage);

        /// <summary>
        /// Completely deactivates the shield
        /// </summary>
        void Disable();

        /// <summary>
        /// Updates the shield position
        /// </summary>
        /// <param name="position"> The position to provide the shield </param>
        void UpdatePosition(Vector2 position);
    }
}
