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

        /// <summary> List of all active player bullets </summary>
        private List<IProjectile> _PlayerBullets;
        /// <summary> List of all active enemy bullets </summary>
        private List<IProjectile> _EnemyBullets;
        /// <summary> List of all active enemies </summary>
        private List<IEnemy> _Enemies;
        /// <summary> The players ship </summary>
        private IPlayer _Player;

        /// <summary> Whether the collision controller is disposed </summary>
        public Boolean IsDisposed { get; set; }
        /// <summary> Whether the collision controller is active </summary>
        public Boolean IsActive { get; set; }

        private CollisionController()
        {
            _PlayerBullets = new List<IProjectile>();
            _EnemyBullets = new List<IProjectile>();
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
        /// <param name="bullet"></param>
        public void RegisterEnemyBullet(IProjectile bullet)
        {
            _EnemyBullets.Add(bullet);
        }

        /// <summary>
        /// Adds player projectile to the player projectile list
        /// </summary>
        /// <param name="bullet"></param>
        public void RegisterPlayerBullet(IProjectile bullet)
        {
            _PlayerBullets.Add(bullet);
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
            CheckBulletsToPlayer();
            CheckBulletsToEnemies();
            CheckPlayerToEnemies();
        }

        /// <summary>
        /// Checks for enemy bullets hitting the player
        /// </summary>
        private void CheckBulletsToPlayer()
        {
            foreach (IProjectile bullet in _EnemyBullets.ToList())
            {
                if (Intersects(bullet.HitBox, _Player.HitBox))
                {
                    HandlePlayerHit(bullet);
                }
            }
        }

        /// <summary>
        /// Checks for player bullets hitting enemies
        /// </summary>
        private void CheckBulletsToEnemies()
        {
            foreach (IProjectile bullet in _PlayerBullets.ToList())
            {
                foreach (IEnemy enemy in _Enemies.ToList())
                {
                    if (Intersects(bullet.HitBox, enemy.HitBox))
                    {
                        HandleEnemyHit(bullet, enemy);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the player ship is colliding with an enemy ship
        /// </summary>
        private void CheckPlayerToEnemies()
        {
            foreach (IEnemy baseEnemy in _Enemies.ToList())
            {
                if (Intersects(_Player.HitBox, baseEnemy.HitBox))
                {
                    HandleEnemyPlayerCollision(baseEnemy);
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
        private void HandlePlayerHit(IProjectile bullet = null)
        {
            if (bullet != null)
            {
                _EnemyBullets.Remove(bullet);
                bullet.Destroy();
            }
            _Player.Hit();
        }

        /// <summary>
        /// Handles collisions between player projectiles and enemies
        /// </summary>
        private void HandleEnemyHit(IProjectile bullet, IEnemy enemy)
        {
            _PlayerBullets.Remove(bullet);
            bullet?.Destroy();
            enemy.Hit();
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
            foreach (IEnemy baseEnemy in _Enemies.ToList())
            {
                baseEnemy.Dispose();
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
            foreach (IProjectile enemyBullet in _EnemyBullets)
            {
                enemyBullet.Dispose();
            }
            _EnemyBullets.Clear();
            foreach (Bullet playerBullet in _PlayerBullets)
            {
                playerBullet.Dispose();
            }
            _PlayerBullets.Clear();
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
        /// Removes the given bullet from the collision controller
        /// </summary>
        /// <param name="bullet"></param>
        public void DeregiterBullet(IProjectile bullet)
        {
            if (_EnemyBullets.Contains(bullet))
            {
                _EnemyBullets.Remove(bullet);
            }
            else if (_PlayerBullets.Contains(bullet))
            {
                _PlayerBullets.Remove(bullet);
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
            foreach (IEnemy baseEnemy in _Enemies)
            {
                baseEnemy.Dispose();
            }
            _Enemies.Clear();
            ClearProjectiles();

            _Player.Dispose();
            IsDisposed = true;
        }
    }
}
