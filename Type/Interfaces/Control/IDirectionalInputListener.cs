using OpenTK;
using System;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Recieves position data from the analog stick
    /// </summary>
    public interface IDirectionalInputListener
    {
        /// <summary>
        /// Update data from the analog stic
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> The distance the stick is pushed </param>
        void UpdateDirectionData(Vector2 direction, Single strength);
    }
}
