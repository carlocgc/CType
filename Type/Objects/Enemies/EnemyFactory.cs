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
        /// <summary> Whether the factory is active </summary>
        public Boolean IsActive;
        /// <summary> Tracker for current place in the wave data </summary>
        private Int32 _DataIndex;
        /// <summary> Time that has passed since the last spawn </summary>
        private TimeSpan _TimeSinceLastSpawn;

        private List<WaveData> _LevelData;
        /// <summary> The data for the current wave </summary>
        private WaveData _CurrentWave;
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
            _CurrentWave = data;
            _AliveEnemies = _CurrentWave.ShipCount;
            IsActive = true;
            CollisionController.Instance.IsActive = true;
        }

        public void ReSeed()
        {
            Seed(_CurrentWave);
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

            switch (_CurrentWave.EnemyType)
            {
                case 0:
                    {
                        enemy = new EnemyA("Content/Graphics/enemy2.png", _CurrentWave.SpawnPositions[_DataIndex], 0, new Vector2(-1, 0), 400, TimeSpan.FromSeconds(3));
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

                if (_TimeSinceLastSpawn >= _CurrentWave.SpawnInterval)
                {
                    Spawn();
                }
            }
            else
            {
                if (_AliveEnemies == 0)
                {
                    // TODO Start next wave
                    // If no wave to move to the level complete
                    Seed(_CurrentWave);
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
