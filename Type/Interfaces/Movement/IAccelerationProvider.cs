using System;
using OpenTK;

namespace Type.Interfaces.Movement
{
    /// <summary>
    /// Object that implements this interface will modify a given vector with some acceleration logic and return the result
    /// </summary>
    public interface IAccelerationProvider
    {
        /// <summary>
        /// Updates a given vector using a modifier
        /// </summary>
        /// <param name="position"> The vector to be modified </param>
        /// <param name="timeTilUpdate"></param>
        /// <returns> The modifed vector </returns>
        Vector2 UpdatePosition(TimeSpan timeTilUpdate);
    }
}
