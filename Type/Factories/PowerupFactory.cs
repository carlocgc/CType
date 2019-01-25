using OpenTK;
using System;
using System.Collections.Generic;
using Type.Interfaces;
using Type.Interfaces.Powerups;
using Type.Powerups;

namespace Type.Factories
{
    public class PowerupFactory : IPowerupFactory, INotifier<IPowerupFactoryListener>
    {
        private readonly Random _Rnd = new Random(Environment.TickCount);

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        public PowerupFactory()
        {
        }

        /// <inheritdoc />
        public void Create(Int32 weight, Vector2 position)
        {
            IPowerup powerup;

            // Weight category for different distribution of powerups
            switch (weight)
            {
                case 0:
                    {
                        //Int32 rnd = _Rnd.Next(0, 4);
                        Int32 rnd = 0;

                        switch (rnd)
                        {
                            case 0:
                                {
                                    powerup = new ExtraLifePowerup(position);
                                    break;
                                }

                            default:
                                {
                                    throw new ArgumentOutOfRangeException("Powerup type does not exist");
                                }
                        }

                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("Powerupfactory weight category does not exist");
                    }
            }

            foreach (IPowerupFactoryListener listener in _Listeners)
            {
                listener.OnPowerupCreated(powerup);
            }
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
