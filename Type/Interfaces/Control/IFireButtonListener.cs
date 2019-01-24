using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Interface for listeners to the fire button
    /// </summary>
    public interface IFireButtonListener
    {
        void FireButtonPressed();

        void FireButtonReleased();
    }
}
