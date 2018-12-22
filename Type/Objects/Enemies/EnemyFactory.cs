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
        private GameScene _Scene;
        /// <summary> Whether the factory is disposed </summary>
        public Boolean IsDisposed { get; set; }
        /// <summary> Tracker for current place in the wave data </summary>
        private Int32 _DataIndex;
        /// <summary> Time that has passed since the last spawn </summary>
        private TimeSpan _TimeSinceLastSpawn;
        /// <summary> List of the levels wave data objects </summary>
        private List<WaveData> _LevelData;
        /// <summary> Tracker of the current wave data object </summary>
        private Int32 _WaveIndex;
        /// <summary> The data for the current wave </summary>
        private WaveData _CurrentWave;
        /// <summary> Count of the enemies still alive, used to seed the next wave </summary>
        private Int32 _AliveEnemies;

        private Boolean IsSpawning;

        private Boolean IsActive;

        public EnemyFactory(GameScene scene)
        {
            _Scene = scene;
            UpdateManager.Instance.AddUpdatable(this);
        }

        public void SetLevelData(List<WaveData> waves)
        {
            _LevelData = waves;
            _WaveIndex = 0;
            StartWave();
        }

        /// <summary>
        /// Clears previous wave data, seeds the next wave and begins spawning enemies
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
                        enemy = new EnemyA("Content/Graphics/enemy1.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 400, TimeSpan.FromSeconds(2));
                        enemy.OnOutOfBounds = OnShipDeath;
                        enemy.OnDestroyedByPlayer.Add(OnShipDeath);
                        enemy.OnDestroyedByPlayer.Add(() => _Scene.UpdateScore(enemy.PointValue));
                        CollisionController.Instance.RegisterEnemy(enemy);
                        break;
                    }
                case 1:
                    {
                        enemy = new EnemyB("Content/Graphics/enemy2.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 400, TimeSpan.FromSeconds(1));
                        enemy.OnOutOfBounds = OnShipDeath;
                        enemy.OnDestroyedByPlayer.Add(OnShipDeath);
                        enemy.OnDestroyedByPlayer.Add(() => _Scene.UpdateScore(enemy.PointValue));
                        CollisionController.Instance.RegisterEnemy(enemy);
                        break;
                    }
                case 2:
                    {
                        enemy = new EnemyC("Content/Graphics/enemy3.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 400, TimeSpan.FromSeconds(0.5));
                        enemy.OnOutOfBounds = OnShipDeath;
                        enemy.OnDestroyedByPlayer.Add(OnShipDeath);
                        enemy.OnDestroyedByPlayer.Add(() => _Scene.UpdateScore(enemy.PointValue));
                        CollisionController.Instance.RegisterEnemy(enemy);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }

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

        public void Update(TimeSpan timeTilUpdate)
        {
            if (IsActive)
            {
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
                    if (_AliveEnemies == 0)
                    {
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
            }
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
