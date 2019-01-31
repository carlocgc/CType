using System;
using OpenTK;

namespace Type.Interfaces.Movement
{
    /// <summary>
    /// Object that implements this interface will provide an acceleration vector based on some rule
    /// </summary>
    public interface IAccelerationProvider
    {
        /// <summary>
        /// Gets acceleration vector
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        /// <returns> The acceleration vector  </returns>
        Vector2 GetAcceleration(TimeSpan timeTilUpdate);
    }
}
