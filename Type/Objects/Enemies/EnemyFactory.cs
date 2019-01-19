using System;
using System.Collections.Generic;
using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using Type.Controllers;
using Type.Data;
using Type.Scenes;

namespace Type.Objects.Enemies
{
    /// <summary>
    /// Spawns enemy ships from a given <see cref="WaveData"/> object
    /// </summary>
    public class EnemyFactory : IUpdatable
    {
        /// <summary> Whether ships are being spawned </summary>
        private Boolean IsSpawning;
        /// <summary> WHether the enemy factory is updating </summary>
        private Boolean IsActive;
        /// <summary> Tracker of the current wave data object </summary>
        private Int32 _WaveIndex;
        /// <summary> Tracker for current place in the wave data </summary>
        private Int32 _DataIndex;
        /// <summary> Count of the enemies still alive, used to seed the next wave </summary>
        private Int32 _AliveEnemies;
        /// <summary> Time that has passed since the last spawn </summary>
        private TimeSpan _TimeSinceLastSpawn;
        /// <summary> Reference to the main game scene </summary>
        private GameScene _Scene;
        /// <summary> List of the levels wave data objects </summary>
        private List<WaveData> _LevelData;
        /// <summary> The data for the current wave </summary>
        private WaveData _CurrentWave;

        /// <summary> Whether the factory is disposed </summary>
        public Boolean IsDisposed { get; set; }

        /// <summary>
        /// Spawns enemies based on wave data objects parameters </summary>
        /// <param name="scene"></param>
        public EnemyFactory(GameScene scene)
        {
            _Scene = scene;
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
            _AliveEnemies = _CurrentWave.ShipCount;
            IsSpawning = true;
            IsActive = true;
            CollisionController.Instance.IsActive = true;
        }

        /// <summary>
        /// Creates a new enemies from the current <see cref="WaveData"/>
        /// </summary>
        private void Spawn()
        {
            BaseEnemy enemy;

            switch (_CurrentWave.EnemyType)
            {
                case 0:
                    {
                        enemy = new EnemyA("Content/Graphics/enemy1.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 600, TimeSpan.FromSeconds(1.5f), 3);
                        break;
                    }
                case 1:
                    {
                        enemy = new EnemyB("Content/Graphics/enemy2.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 625, TimeSpan.FromSeconds(1.3f), 7);
                        break;
                    }
                case 2:
                    {
                        enemy = new EnemyC("Content/Graphics/enemy4.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 725, TimeSpan.FromSeconds(1.2), 10);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

            enemy.OnOutOfBounds = OnShipDeath;
            enemy.OnDestroyedByPlayer.Add(OnShipDeath);
            enemy.OnDestroyedByPlayer.Add(() => _Scene.UpdateScore(enemy.PointValue));
            enemy.OnDestroyedByPlayer.Add(() => PositionRelayer.Instance.RemoveRecipient(enemy));

            CollisionController.Instance.RegisterEnemy(enemy);
            PositionRelayer.Instance.AddRecipient(enemy);

            _TimeSinceLastSpawn = TimeSpan.Zero;
            _DataIndex++;

            if (_DataIndex == _CurrentWave.ShipCount)
            {
                IsSpawning = false;
            }
        }

        /// <summary>
        /// Action passed to every ship, updates <see cref="_AliveEnemies"/>
        /// </summary>
        private void OnShipDeath()
        {
            _AliveEnemies--;
            if (_AliveEnemies == 0) _DataIndex = 0;
        }

        /// <summary>
        /// Spawns enemies until there are none left to spawn, checks if all enemies are dead and starts the next wave if true
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            if (!IsActive) return;

            if (IsSpawning)
            {
                _TimeSinceLastSpawn += timeTilUpdate;

                if (_TimeSinceLastSpawn >= _CurrentWave.SpawnInterval)
                {
                    Spawn();
                }
            }
            else
            {
                if (_AliveEnemies != 0) return;

                _WaveIndex++;

                if (_WaveIndex >= _LevelData.Count)
                {
                    Reset();
                    _Scene.LevelComplete();
                }
                else
                {
                    _CurrentWave = _LevelData[_WaveIndex];
                    StartWave();
                }
            }
        }

        /// <summary>
        /// Resets the data tracker and deactivates the Factory
        /// </summary>
        public void Reset()
        {
            IsActive = false;
            IsSpawning = false;
            _TimeSinceLastSpawn = TimeSpan.Zero;
            _DataIndex = 0;
            CollisionController.Instance.ClearObjects();
        }

        /// <summary>
        /// Whether the factory can update
        /// </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <summary>
        /// Disposes the factory
        /// </summary>
        public void Dispose()
        {
            _Scene = null;
            IsDisposed = true;
        }
    }
}
