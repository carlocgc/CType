using System;
using OpenTK;

namespace Type.Interfaces.Movement
{
    /// <summary>
    /// Object that implements this interface will apply an acceleration vector to another vector
    /// </summary>
    public interface IAccelerationProvider
    {
        /// <summary>
        /// Apply acceleration to a vector
        /// </summary>
        /// <param name="baseVector"> The vector to apply acceleration to </param>
        /// <param name="timeTilUpdate"></param>
        /// <returns> The acceleration vector  </returns>
        Vector2 ApplyAcceleration(Vector2 baseVector, TimeSpan timeTilUpdate);
    }
}
