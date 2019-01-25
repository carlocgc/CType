using OpenTK;
using System;
using System.Collections.Generic;
using Type.Interfaces;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;

namespace Type.Powerups
{
    public class ExtraLifePowerup : IPowerup, INotifier<IPowerupListener>
    {
        /// <inheritdoc />
        public Int32 ID { get; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <inheritdoc />
        public Int32 HitPoints { get; }

        /// <inheritdoc />
        public Vector2 Position { get; set; }

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        /// <inheritdoc />
        public void Apply(IPlayer player)
        {

        }

        /// <inheritdoc />
        public void Hit(Int32 damage)
        {

        }

        /// <inheritdoc />
        public void Destroy()
        {

        }

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

        }

        #region Listener

        private readonly List<IPowerupListener> _Listeners = new List<IPowerupListener>();

        /// <inheritdoc />
        public void RegisterListener(IPowerupListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IPowerupListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

        #endregion
    }
}
