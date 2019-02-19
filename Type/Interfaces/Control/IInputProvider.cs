using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Control
{
    public interface IInputProvider
    {
        /// <summary>
        /// Add a listener
        /// </summary>
        void RegisterListener(IInputListener listener);

        /// <summary>
        /// Remove a listener
        /// </summary>
        void DeregisterListener(IInputListener listener);
    }
}
