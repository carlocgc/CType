using AmosShared.Interfaces;
using System;
using System.Collections.Generic;
using Type.Interfaces;
using Type.Interfaces.Control;

namespace Type.Android.Source.Controllers
{
    public class AndroidInputProvider : IInputProvider, INotifier<IInputListener>, IUpdatable
    {
        private readonly List<IInputListener> _Listeners = new List<IInputListener>();

        #region Implementation of IUpdatable

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {

        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return false;
        }

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        #endregion

        #region Implementation of INotifier<in IInputListener>

        /// <summary>
        /// Add a listener
        /// </summary>
        public void RegisterListener(IInputListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener
        /// </summary>
        public void DeregisterListener(IInputListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }

        #endregion
    }
}