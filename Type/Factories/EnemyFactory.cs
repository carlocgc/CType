﻿using AmosShared.Base;
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
        /// <summary> Whether enemies are being created </summary>
        private Boolean _Spawning;

        /// <summary> Game state to add as a listener to created enemies </summary>
        public PlayingState ParentState { get; set; }

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

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

            Int32 total = 0;

            foreach (WaveData data in _LevelData)
            {
                total += data.ShipCount;
            }

            foreach (IEnemyFactoryListener listener in _Listeners)
            {
                listener.OnLevelStarted(total);
            }

            _Spawning = true;
        }

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            if (_Spawning)
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
                case -1:
                    {
                        // No acceleration provider
                        break;
                    }
                case 0:
                    {
                        accel = new LinearMotion(_CurrentWave.MoveDirections[_DataIndex], _CurrentWave.MovementSpeeds[_DataIndex]);
                        break;
                    }
                case 1:
                    {
                        accel = new WaveMotion(_CurrentWave.MoveDirections[_DataIndex], _CurrentWave.MovementSpeeds[_DataIndex]);
                        break;
                    }
                case 2:
                    {
                        accel = new EllipseDecreaseMotion(_CurrentWave.MoveDirections[_DataIndex], _CurrentWave.MovementSpeeds[_DataIndex]);
                        break;
                    }
                case 3:
                    {
                        accel = new EllipseIncreaseMotion(_CurrentWave.MoveDirections[_DataIndex], _CurrentWave.MovementSpeeds[_DataIndex]);
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
                        enemy = new SmallEnemyWeak(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 1:
                    {
                        enemy = new MediumEnemyWeak(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 2:
                    {
                        enemy = new LargeEnemyWeak(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 3:
                    {
                        enemy = new SmallEnemyStrong(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 4:
                    {
                        enemy = new MediumEnemyStrong(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 5:
                    {
                        enemy = new LargeEnemyStrong(_CurrentWave.Ypositions[_DataIndex], accel);
                        break;
                    }
                case 20:
                    {
                        enemy = new BossFighter();
                        break;
                    }
                case 21:
                    {
                        enemy = new BossStation();
                        break;
                    }
                case 22:
                    {
                        enemy = new BossFighterStrong();
                        break;
                    }
                case 23:
                    {
                        enemy = new BossStationStrong();
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
        /// Resets the data tracker and deactivates the Factory
        /// </summary>
        public void Stop()
        {
            _Spawning = false;
            _DataIndex = 0;
            _WaveIndex = 0;
            _TimeSinceLastSpawn = TimeSpan.Zero;

            foreach (IEnemyFactoryListener listener in _Listeners)
            {
                listener.OnLevelFinishedSpawning();
            }
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsDisposed) return;
            _Spawning = false;
            _Listeners.Clear();
            _LevelData.Clear();
            ParentState = null;
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
