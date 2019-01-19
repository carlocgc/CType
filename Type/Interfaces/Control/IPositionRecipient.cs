using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;


namespace Type.Interfaces.Control
{
    /// <summary> Objects that can recieve a vector2 from another object </summary>
    public interface IPositionRecipient
    {
        void Receive(Vector2 position);
    }
}
