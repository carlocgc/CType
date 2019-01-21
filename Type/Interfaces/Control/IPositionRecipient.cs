using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;


namespace Type.Interfaces.Control
{
    /// <summary> Objects that can receive a vector2 from another object </summary>
    public interface IPositionRecipient
    {
        /// <summary>
        /// Update position data
        /// </summary>
        /// <param name="position"> The received position data </param>
        void UpdatePositionData(Vector2 position);
    }
}
