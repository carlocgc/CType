using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Interfaces.Weapons;
using Type.Objects.Projectiles;

namespace Type.Controllers
{
    /// <summary>
    /// Checks if registered objects have collided and handles those collisions
    /// </summary>
    public class CollisionController : IUpdatable
    {
        /// <summary> The instance of the Collision Controller </summary>
        private static CollisionController _Instance;
        /// <summary> The instance of the Collision Controller </summary>
        public static CollisionController Instance => _Instance ?? (_Instance = new CollisionController());

        /// <summary> List of all active player projectiles </summary>
        private readonly List<IProjectile> _PlayerProjectiles;
        /// <summary> List of all active enemy projectiles </summary>
        private readonly List<IProjectile> _EnemyProjectiles;
        /// <summary> List of all active enemies </summary>
        private readonly List<IEnemy> _Enemies;
        /// <summary> The players ship </summary>
        private IPlayer _Player;

        /// <summary> Whether the collision controller is disposed </summary>
        public Boolean IsDisposed { get; set; }
        /// <summary> Whether the collision controller is active </summary>
        public Boolean IsActive { get; set; }

        private CollisionController()
        {
            _PlayerProjectiles = new List<IProjectile>();
            _EnemyProjectiles = new List<IProjectile>();
            _Enemies = new List<IEnemy>();
            UpdateManager.Instance.AddUpdatable(this);
        }

        #region Register

        /// <summary>
        /// Adds enemy to the collision list
        /// </summary>
        /// <param name="enemy"></param>
        public void RegisterEnemy(IEnemy enemy)
        {
            _Enemies.Add(enemy);
        }

        /// <summary>
        /// Adds enemy projectile to the enemy projectile list
        /// </summary>
        /// <param name="projectile"></param>
        public void RegisterEnemyProjectile(IProjectile projectile)
        {
            _EnemyProjectiles.Add(projectile);
        }

        /// <summary>
        /// Adds player projectile to the player projectile list
        /// </summary>
        /// <param name="projectile"></param>
        public void RegisterPlayerProjectile(IProjectile projectile)
        {
            _PlayerProjectiles.Add(projectile);
        }

        /// <summary>
        /// Registers the player object to the collision controller
        /// </summary>
        /// <param name="player"></param>
        public void RegisterPlayer(IPlayer player)
        {
            _Player = player;
        }

        #endregion

        #region Checking

        /// <summary>
        /// Performs Intersection checks for each group of objects that will collide with each other and handles those collisions
        /// </summary>
        private void CheckCollisions()
        {
            CheckprojectilesToPlayer();
            CheckprojectilesToEnemies();
            CheckPlayerToEnemies();
        }

        /// <summary>
        /// Checks for enemy projectiles hitting the player
        /// </summary>
        private void CheckprojectilesToPlayer()
        {
            foreach (IProjectile projectile in _EnemyProjectiles.ToList())
            {
                if (Intersects(projectile.HitBox, _Player.HitBox))
                {
                    HandlePlayerHit(projectile);
                }
            }
        }

        /// <summary>
        /// Checks for player projectiles hitting enemies
        /// </summary>
        private void CheckprojectilesToEnemies()
        {
            foreach (IProjectile projectile in _PlayerProjectiles.ToList())
            {
                foreach (IEnemy enemy in _Enemies.ToList())
                {
                    if (Intersects(projectile.HitBox, enemy.HitBox))
                    {
                        HandleEnemyHit(projectile, enemy);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the player ship is colliding with an enemy ship
        /// </summary>
        private void CheckPlayerToEnemies()
        {
            foreach (IEnemy enemy in _Enemies.ToList())
            {
                if (Intersects(_Player.HitBox, enemy.HitBox))
                {
                    HandleEnemyPlayerCollision(enemy);
                }
            }
        }

        /// <summary>
        /// returns true if two rectangles intersects
        /// </summary>
        private Boolean Intersects(Vector4 rectA, Vector4 rectB)
        {
            return rectA.X < rectB.X + rectB.Z &&
                   rectA.X + rectA.Z > rectB.X &&
                   rectA.Y < rectB.Y + rectB.W &&
                   rectA.W + rectA.Y > rectB.Y;
        }

        #endregion

        #region Handling

        /// <summary>
        /// Handles collisions with the player ship
        /// </summary>
        private void HandlePlayerHit(IProjectile projectile = null)
        {
            _Player.Hit(projectile);

            if (projectile == null) return;
            _EnemyProjectiles.Remove(projectile);
            projectile.Destroy();
        }

        /// <summary>
        /// Handles collisions between player projectiles and enemies
        /// </summary>
        private void HandleEnemyHit(IProjectile projectile, IEnemy enemy)
        {
            _PlayerProjectiles.Remove(projectile);
            enemy.Hit(projectile);
            projectile?.Destroy();
        }

        /// <summary>
        /// Handles enemy collisions with the player
        /// </summary>
        /// <param name="enemy"></param>
        private void HandleEnemyPlayerCollision(IEnemy enemy)
        {
            enemy.Destroy();
            HandlePlayerHit();
        }

        #endregion

        #region CleanUp

        /// <summary>
        /// Clears all lists and deactivates the controller
        /// </summary>
        public void ClearObjects()
        {
            IsActive = false;
            foreach (IEnemy enemy in _Enemies.ToList())
            {
                enemy.Dispose();
            }
            _Enemies.Clear();
            ClearProjectiles();
            IsActive = false;
        }

        /// <summary>
        /// Disposes all alive projectiles
        /// </summary>
        private void ClearProjectiles()
        {
            foreach (IProjectile projectile in _EnemyProjectiles)
            {
                projectile.Dispose();
            }
            _EnemyProjectiles.Clear();
            foreach (IProjectile projectile in _PlayerProjectiles)
            {
                projectile.Dispose();
            }
            _PlayerProjectiles.Clear();
        }

        /// <summary>
        /// Removes the given enemy from the enenmy list
        /// </summary>
        /// <param name="enemy"></param>
        public void DeregisterEnemy(IEnemy enemy)
        {
            _Enemies.Remove(enemy);
        }

        /// <summary>
        /// Removes the given projectile from the collision controller
        /// </summary>
        /// <param name="projectile"></param>
        public void DeregisterProjectile(IProjectile projectile)
        {
            if (_EnemyProjectiles.Contains(projectile))
            {
                _EnemyProjectiles.Remove(projectile);
            }
            else if (_PlayerProjectiles.Contains(projectile))
            {
                _PlayerProjectiles.Remove(projectile);
            }
        }

        #endregion

        /// <summary>
        /// Returns whether the controller can update
        /// </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

        public void Update(TimeSpan timeTilUpdate)
        {
            if (IsActive)
            {
                CheckCollisions();
            }
        }

        public void Dispose()
        {
            foreach (IEnemy enemy in _Enemies)
            {
                enemy.Dispose();
            }
            _Enemies.Clear();
            ClearProjectiles();
            _Player.Dispose();
            IsDisposed = true;
        }
    }
}
