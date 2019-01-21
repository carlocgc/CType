using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces
{
    /// <summary>
    /// Object that implements this interface can notify listeners of events
    /// </summary>
    /// <typeparam name="T"> The type of listener this object notifies </typeparam>
    public interface INotifier<in T>
    {
        /// <summary>
        /// Add a listener
        /// </summary>
        void RegisterListener(T listener);

        /// <summary>
        /// Remove a listener
        /// </summary>
        void DeregisterListener(T listener);
    }
}
