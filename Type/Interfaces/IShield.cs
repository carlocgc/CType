using AmosShared.Interfaces;
using System;
using OpenTK;

namespace Type.Interfaces
{
    public interface IShield : IPositionable
    {
        /// <summary>
        /// Whether the shield is on
        /// </summary>
        Boolean IsActive { get; }

        /// <summary>
        /// Increases the shields level
        /// </summary>
        void Increase();

        /// <summary>
        /// Decreases the shields level
        /// </summary>
        void Decrease();

        /// <summary>
        /// Completely deactivates the shield
        /// </summary>
        void Disable();

        /// <summary>
        /// Updates the shield position
        /// </summary>
        /// <param name="position"></param>
        void UpdatePosition(Vector2 position);
    }
}
