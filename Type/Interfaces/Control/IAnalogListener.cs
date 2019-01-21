using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Recieves position data from the analog stick
    /// </summary>
    public interface IAnalogListener
    {
        /// <summary>
        /// Update data from the analog stic
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> The distance the stick is pushed </param>
        void UpdateAnalogData(Vector2 direction, Single strength);
    }
}
