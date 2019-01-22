using AmosShared.Base;
using AmosShared.Interfaces;
using System;
using System.Collections.Generic;
using Type.Controllers;
using Type.Data;
using Type.Interfaces.Enemies;
using Type.Objects.Bosses;
using Type.Objects.Enemies;
using Type.Scenes;

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

        /// <summary> Whether ships are being spawned </summary>
        private Boolean IsSpawning;
        /// <summary> WHether the enemy factory is updating </summary>
        private Boolean IsActive;
        /// <summary> Tracker of the current wave data object </summary>
        private Int32 _WaveIndex;
        /// <summary> Tracker for current place in the wave data </summary>
        private Int32 _DataIndex;
        /// <summary> Time that has passed since the last spawn </summary>
        private TimeSpan _TimeSinceLastSpawn;
        /// <summary> Reference to the main game scene </summary>
        private GameScene _Scene;
        /// <summary> List of the levels wave data objects </summary>
        private List<WaveData> _LevelData;
        /// <summary> The data for the current wave </summary>
        private WaveData _CurrentWave;

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        /// <summary> Amount of enemies this wave </summary>
        public Int32 WaveCount => _CurrentWave.ShipCount;

        public EnemyFactory()
        {
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
            StartWave();
        }

        /// <summary>
        /// Sets the next wave data and begins spawning enemies
        /// </summary>
        /// <param name="data"></param>
        public void StartWave()
        {
            _DataIndex = 0;
            _CurrentWave = _LevelData[_WaveIndex];
            IsSpawning = true;
            IsActive = true;
            CollisionController.Instance.IsActive = true;
        }

                /// <summary>
        /// Creates a new enemies from the current <see cref="WaveData"/>
        /// </summary>
        private void Spawn()
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
                        enemy = new BossA("Content/Graphics/Bosses/boss1.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 300, TimeSpan.FromSeconds(0.7), 2500);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            enemy.OnDestroyedByPlayer.Add(() => _Scene.UpdateScore(enemy.PointValue));
            enemy.OnDestroyedByPlayer.Add(() => PositionRelayer.Instance.RemoveRecipient(enemy));

            CollisionController.Instance.RegisterEnemy(enemy);
            PositionRelayer.Instance.AddRecipient(enemy);

            _TimeSinceLastSpawn = TimeSpan.Zero;
            _DataIndex++;

            if (_DataIndex != _CurrentWave.ShipCount) return;

            IsSpawning = false;
            _DataIndex = 0;
        }
        

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {

        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {

        }




    }
}
