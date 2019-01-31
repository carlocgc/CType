using AmosShared.Base;
using AmosShared.Interfaces;
using System;
using System.Collections.Generic;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Enemies;
using Type.Interfaces.Movement;
using Type.Objects.Bosses;
using Type.Objects.Enemies;
using Type.Objects.Enemies.Movement;
using Type.States;

namespace Type.Factories
{
    /// <summary>
    /// Spawns enemy ships from a given <see cref="WaveData"/> object
    /// </summary>

    public class EnemyFactory : IUpdatable, INotifier<IEnemyFactoryListener>
    {
        /// <summary> Tracker of the current wave data object </summary>
        private Int32 _WaveIndex;
        /// <summary> Tracker for current place in the wave data </summary>
        private Int32 _DataIndex;
        /// <summary> Time that has passed since the last spawn </summary>
        private TimeSpan _TimeSinceLastSpawn;
        /// <summary> List of the levels wave data objects </summary>
        private List<WaveData> _LevelData;
        /// <summary> The data for the current wave </summary>
        private WaveData _CurrentWave;

        /// <summary> Game state to add as a listener to created enemies </summary>
        public PlayingState ParentState { get; set; }
        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }
        /// <summary> Whether enemies are being created </summary>
        public Boolean Creating { get; private set; }

        public EnemyFactory()
        {
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Assigns list of wave data and starts spawning enemies
        /// </summary>
        /// <param name="waves"></param>
        public void Start(List<WaveData> waves)
        {
            _LevelData = waves;
            _DataIndex = 0;
            _CurrentWave = _LevelData[_WaveIndex];
            Creating = true;
        }

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            if (Creating)
            {
                _TimeSinceLastSpawn += timeTilUpdate;

                if (_TimeSinceLastSpawn >= _CurrentWave.SpawnInterval[_DataIndex])
                {
                    Create();
                }
            }
        }

        /// <summary>
        /// Creates a new enemies from the current <see cref="WaveData"/>
        /// </summary>
        private void Create()
        {
            IAccelerationProvider accel = null;
            IEnemy enemy;

            // Create move component
            switch (_CurrentWave.MovementTypes[_DataIndex])
            {
                case 1:
                    {
                        accel = new WaveMotion(_CurrentWave.MoveDirections[_DataIndex], _CurrentWave.MovementSpeeds[_DataIndex]);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("Movement type does not exist");
                    }
            }

            // Create enemy
            switch (_CurrentWave.EnemyTypes[_DataIndex])
            {
                case 0:
                    {
                        enemy = new Enemy_01(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 1:
                    {
                        enemy = new Enemy_02(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 2:
                    {
                        enemy = new Enemy_03(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 20:
                    {
                        enemy = new BossAlpha(_CurrentWave.Ypositions[_DataIndex]);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            enemy.RegisterListener(ParentState);

            foreach (IEnemyFactoryListener listener in _Listeners)
            {
                listener.EnemyCreated(enemy);
            }

            _TimeSinceLastSpawn = TimeSpan.Zero;
            _DataIndex++;

            if (_DataIndex != _CurrentWave.ShipCount) return;

            if (_WaveIndex != _LevelData.Count - 1)
            {
                WaveComplete();
            }
            else
            {
                Stop();
            }
        }

        /// <summary>
        /// Stops the factory creating
        /// </summary>
        private void WaveComplete()
        {
            _DataIndex = 0;
            _WaveIndex++;
            _CurrentWave = _LevelData[_WaveIndex];
            _TimeSinceLastSpawn = TimeSpan.Zero;
        }

        /// <summary>
        /// Restarts the current wave
        /// </summary>
        public void RestartWave()
        {
            _DataIndex = 0;
            Creating = true;
            _TimeSinceLastSpawn = TimeSpan.Zero;
        }

        /// <summary>
        /// Resets the data tracker and deactivates the Factory
        /// </summary>
        public void Stop()
        {
            Creating = false;
            _DataIndex = 0;
            _WaveIndex = 0;
            _TimeSinceLastSpawn = TimeSpan.Zero;
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _LevelData.Clear();
            IsDisposed = true;
        }

        #region Listener

        private readonly List<IEnemyFactoryListener> _Listeners = new List<IEnemyFactoryListener>();

        /// <inheritdoc />
        public void RegisterListener(IEnemyFactoryListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IEnemyFactoryListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        #endregion
    }
}
