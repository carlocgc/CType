using AmosShared.Base;
using AmosShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using AmosShared.State;
using Type.Controllers;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Communication;
using Type.Interfaces.Enemies;
using Type.Objects.Bosses;
using Type.Objects.Enemies;
using Type.Scenes;
using Type.States;

namespace Type.Factories
{
    /// <summary>
    /// Spawns enemy ships from a given <see cref="WaveData"/> object
    /// </summary>

    public class EnemyFactory : IUpdatable, INotifier<IEnemyFactoryListener>
    {
        /// <summary> The instance of the Enemy Factory </summary>
        private static EnemyFactory _Instance;
        /// <summary> The instance of the Enemy Factory </summary>
        public static EnemyFactory Instance => _Instance ?? (_Instance = new EnemyFactory());

        /// <summary> List of objects listening to this factory </summary>
        private List<IEnemyFactoryListener> _Listeners;
        /// <summary> WHether the enemy factory is updating </summary>
        private Boolean IsActive;
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
        /// <summary> Amount of enemies this wave </summary>
        public Int32 WaveCount => _CurrentWave.ShipCount;

        public EnemyFactory()
        {
            _Listeners = new List<IEnemyFactoryListener>();
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Assigns list of wave data
        /// </summary>
        /// <param name="waves"></param>
        public void SetLevelData(List<WaveData> waves)
        {
            _LevelData = waves;
            _WaveIndex = 0;
        }

        /// <summary>
        /// Sets the next wave data and begins spawning enemies
        /// </summary>
        public void Start()
        {
            _DataIndex = 0;
            _CurrentWave = _LevelData[_WaveIndex];
            Creating = true;
            IsActive = true;
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
            IEnemy enemy;

            switch (_CurrentWave.EnemyTypes[_DataIndex])
            {
                case 0:
                    {
                        enemy = new EnemyAlpha(_CurrentWave.SpawnPositions[_DataIndex]);
                        break;
                    }
                case 1:
                    {
                        enemy = new EnemyBeta(_CurrentWave.SpawnPositions[_DataIndex]);
                        break;
                    }
                case 2:
                    {
                        enemy = new EnemyGamma(_CurrentWave.SpawnPositions[_DataIndex]);
                        break;
                    }
                case 20:
                    {
                        //enemy = new BossA("Content/Graphics/Bosses/boss1.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 300, TimeSpan.FromSeconds(0.7), 2500);
                        throw new ArgumentOutOfRangeException("No boss creation defined");
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            enemy.RegisterListener(ParentState);
            CollisionController.Instance.RegisterEnemy(enemy);
            PositionRelayer.Instance.AddRecipient(enemy);

            _TimeSinceLastSpawn = TimeSpan.Zero;
            _DataIndex++;

            if (_DataIndex != _CurrentWave.ShipCount) return;

            foreach (IEnemyFactoryListener listener in _Listeners)
            {
                listener.OnWaveCreated();
            }

            if (_WaveIndex != _LevelData.Count) return;

            foreach (IEnemyFactoryListener listener in _Listeners)
            {
                listener.OnAllWavesCreated();
            }
        }

        /// <summary>
        /// Stops the factory creating
        /// </summary>
        public void Stop()
        {
            _DataIndex = 0;
            _WaveIndex++;
            Creating = false;
            _TimeSinceLastSpawn = TimeSpan.Zero;
        }

        /// <summary>
        /// Resets the data tracker and deactivates the Factory
        /// </summary>
        public void Reset()
        {
            IsActive = false;
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
        public void DeregisterListener(IEnemyFactoryListener listener)
        {
            _Listeners.Remove(listener);
        }

        /// <inheritdoc />
        public void RegisterListener(IEnemyFactoryListener listener)
        {
            _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Listeners.Clear();
            _LevelData.Clear();
            IsDisposed = true;
        }
    }
}
