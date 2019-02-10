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

        /// <inheritdoc />
        public void Create(Int32 weight, Vector2 position, Int32 currentLevel)
        {
            IPowerup powerup;

            // Weight category for different distribution of powerups
            switch (weight)
            {
                // -------------------------- STANDARD WEIGHTING (Points 50% - Shield 5% - Probe 5% - Life 1%)
                case 0:
                    {

                        Int32 rnd = _Rnd.Next(0, 101);

                        switch (rnd)
                        {
                            case Int32 n when n >= 0 && n < 85:
                                {
                                    powerup = new PointsPickup(position, currentLevel);
                                    break;
                                }
                            case Int32 n when n >= 85 && n < 91:
                                {
                                    powerup = new ShieldPowerup(position);
                                    break;
                                }
                            case Int32 n when n >= 91 && n < 94:
                                {
                                    powerup = new ProbePowerup(position);
                                    break;
                                }
                            case Int32 n when n >= 94 && n < 99:
                                {
                                    powerup = new NukePickup(position);
                                    break;
                                }
                            case Int32 n when n >= 99:
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
                // -------------------------- PLAYER DEATH WEIGHTING (Probe 100%)
                case 1:
                    {
                        powerup = new ProbePowerup(position);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("Powerup factory weight category does not exist");
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
