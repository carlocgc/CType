using AmosShared.Base;
using AmosShared.Interfaces;
using System;
using System.Collections.Generic;
using Type.Controllers;
using Type.Data;
using Type.Interfaces.Enemies;
using Type.Objects.Bosses;
using Type.Objects.Enemies;
using Type.States;

namespace Type.Factories
{
    /// <summary>
    /// Spawns enemy ships from a given <see cref="WaveData"/> object
    /// </summary>

    public class EnemyFactory : IUpdatable
    {
        /// <summary> The instance of the Enemy Factory </summary>
        private static EnemyFactory _Instance;
        /// <summary> The instance of the Enemy Factory </summary>
        public static EnemyFactory Instance => _Instance ?? (_Instance = new EnemyFactory());

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

        private EnemyFactory()
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
            IEnemy enemy;

            switch (_CurrentWave.EnemyTypes[_DataIndex])
            {
                case 0:
                    {
                        enemy = new EnemyAlpha(_CurrentWave.Ypositions[_DataIndex]);
                        break;
                    }
                case 1:
                    {
                        enemy = new EnemyBeta(_CurrentWave.Ypositions[_DataIndex]);
                        break;
                    }
                case 2:
                    {
                        enemy = new EnemyGamma(_CurrentWave.Ypositions[_DataIndex]);
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
            CollisionController.Instance.RegisterEnemy(enemy);
            PositionRelayer.Instance.AddRecipient(enemy);

            _TimeSinceLastSpawn = TimeSpan.Zero;
            _DataIndex++;

            if (_DataIndex != _CurrentWave.ShipCount) return;

            Stop();

            if (_WaveIndex != _LevelData.Count) return;

            Reset();
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
    }
}
