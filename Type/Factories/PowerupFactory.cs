using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Type.Interfaces;
using Type.Interfaces.Powerups;

namespace Type.Factories
{
    public class PowerupFactory : IPowerupFactory, INotifier<IPowerupFactoryListener>
    {
        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        public PowerupFactory()
        {

        }

        /// <inheritdoc />
        public void Create(Int32 weight, Vector2 position, Int32 amount)
        {

        }

        #region Listener

        private readonly List<IPowerupFactoryListener> _Listeners = new List<IPowerupFactoryListener>();

        /// <inheritdoc />
        public void RegisterListener(IPowerupFactoryListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IPowerupFactoryListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

        #endregion

        
        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {

        }


        /// <inheritdoc />
        public void Dispose()
        {
            _Listeners.Clear();
            IsDisposed = true;
        }
    }
}
