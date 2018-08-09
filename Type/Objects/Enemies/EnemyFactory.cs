using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using Type.Controllers;
using Type.Scenes;

namespace Type.Objects.Enemies
{
    /// <summary>
    /// Data for a ship wave
    /// </summary>
    public struct WaveData
    {
        /// <summary> Amount of ships in the wave </summary>
        public Int32 ShipCount;
        /// <summary> How long between each spawn </summary>
        public TimeSpan SpawnInterval;
        /// <summary> List of positions to spawn the ships </summary>
        public Vector2[] SpawnPositions;
        /// <summary> What type of ships comprise the wave   </summary>
        public Int32 EnemyType;

        public WaveData(Int32 amount, TimeSpan interval, Int32 enemyType, Vector2[] spawnPosList = null)
        {
            ShipCount = amount;
            SpawnInterval = interval;
            EnemyType = enemyType;
            if (spawnPosList == null)
            {
                SpawnPositions = new[]
                {
                    new Vector2(1100, 300),
                    new Vector2(1100, 200),
                    new Vector2(1100, 100),
                    new Vector2(1100, 0),
                    new Vector2(1100, -100),
                    new Vector2(1100, -200),
                    new Vector2(1100, -300),
                };
            }
            else
            {
                SpawnPositions = spawnPosList;
            }
        }
    }

    /// <summary>
    /// Spawns enemy ships from a given <see cref="WaveData"/> object
    /// </summary>
    public class EnemyFactory : IUpdatable
    {
        private GameScene _Scene;
        /// <summary> Whether the factory is disposed </summary>
        public Boolean IsDisposed { get; set; }
        /// <summary> Whether the factory is active </summary>
        public Boolean IsActive;
        /// <summary> Tracker for current place in the wave data </summary>
        private Int32 _DataIndex;
        /// <summary> Time that has passed since the last spawn </summary>
        private TimeSpan _TimeSinceLastSpawn;
        /// <summary> The data for the current wave </summary>
        private WaveData _WaveData;
        /// <summary> Count of the enemies still alive, used to seed the next wave </summary>
        private Int32 _AliveEnemies;

        public EnemyFactory(GameScene scene)
        {
            _Scene = scene;
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Clears previous wave data, seeds the next wave and begins spawning enemies
        /// </summary>
        /// <param name="data"></param>
        public void Seed(WaveData data)
        {
            _WaveData = data;
            _AliveEnemies = _WaveData.ShipCount;
            IsActive = true;
            CollisionController.Instance.IsActive = true;
        }

        public void ReSeed()
        {
            Seed(_WaveData);
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
        /// Creates a new enemies from the current <see cref="WaveData"/>
        /// </summary>
        private void Spawn()
        {
            BaseEnemy enemy;

            switch (_WaveData.EnemyType)
            {
                case 0:
                    {
                        enemy = new EnemyA("Content/Graphics/enemy2.png", _WaveData.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 400, TimeSpan.FromSeconds(3));
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

            if (_DataIndex == _WaveData.ShipCount)
            {
                IsActive = false;
            }
        }

        /// <summary>
        /// Resets the data tracker and deactivates the Factory
        /// </summary>
        public void Reset()
        {
            IsActive = false;
            _TimeSinceLastSpawn = TimeSpan.Zero;
            _DataIndex = 0;
            CollisionController.Instance.ClearObjects();
        }

        public void Update(TimeSpan timeTilUpdate)
        {
            if (IsActive)
            {
                _TimeSinceLastSpawn += timeTilUpdate;

                if (_TimeSinceLastSpawn >= _WaveData.SpawnInterval)
                {
                    Spawn();
                }
            }
            else
            {
                if (_AliveEnemies == 0)
                {
                    Seed(_WaveData);
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
