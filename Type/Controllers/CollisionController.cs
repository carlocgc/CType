using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using Type.Objects.Enemies;
using Type.Objects.Player;
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

        /// <summary> Whether the collision controller is disposed </summary>
        public Boolean IsDisposed { get; set; }

        /// <summary> List of all active player bullets </summary>
        private List<Bullet> _PlayerBullets;
        /// <summary> List of all active enemy bullets </summary>
        private List<Bullet> _EnemyBullets;
        /// <summary> List of all active enemies </summary>
        private List<BaseEnemy> _Enemies;
        /// <summary> The players ship </summary>
        private Player _Player;

        /// <summary> Whether the collision controller is active </summary>
        public Boolean IsActive { get; set; }

        private CollisionController()
        {
            _PlayerBullets = new List<Bullet>();
            _EnemyBullets = new List<Bullet>();
            _Enemies = new List<BaseEnemy>();
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Returns whether the controller can update 
        /// </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

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
            foreach (Bullet bullet in _EnemyBullets.ToList())
            {
                if (Intersects(bullet.GetRect(), _Player.GetRect()))
                {
                    HandlePlayerHit();
                }
                if (bullet.IsDisposed) break;
            }
        }

        /// <summary>
        /// Checks for player bullets hitting enemies
        /// </summary>
        private void CheckBulletsToEnemies()
        {
            foreach (Bullet bullet in _PlayerBullets.ToList())
            {
                foreach (BaseEnemy enemy in _Enemies.Where(e => e.IsAlive).ToList())
                {
                    if (Intersects(bullet.GetRect(), enemy.GetRect()))
                    {
                        HandleEnemyHit(bullet, enemy);
                    }

                    if (bullet.IsDisposed) break;
                }
            }
        }

        /// <summary>
        /// Checks if the player ship is colliding with an enemy ship
        /// </summary>
        private void CheckPlayerToEnemies()
        {
            foreach (BaseEnemy baseEnemy in _Enemies.ToList())
            {
                if (Intersects(_Player.GetRect(), baseEnemy.GetRect()))
                {
                    HandlePlayerHit();
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

        /// <summary>
        /// Handles collisions with the player ship
        /// </summary>
        private void HandlePlayerHit()
        {
            _Player.LoseLife();
            ClearProjectiles();
        }

        /// <summary>
        /// Handles collisions between player projectiles and enemies
        /// </summary>
        private void HandleEnemyHit(Bullet bullet, BaseEnemy enemy)
        {
            bullet.Dispose();
            _PlayerBullets.Remove(bullet);
            enemy.Hit();
        }

        /// <summary>
        /// Disposes all alive projectiles
        /// </summary>
        private void ClearProjectiles()
        {
            foreach (Bullet enemyBullet in _EnemyBullets)
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
        /// Adds enemy to the collision list
        /// </summary>
        /// <param name="enemy"></param>
        public void RegisterEnemy(BaseEnemy enemy)
        {
            _Enemies.Add(enemy);
        }

        /// <summary>
        /// Adds enemy projectile to the enemy projectile list
        /// </summary>
        /// <param name="bullet"></param>
        public void RegisterEnemyBullet(Bullet bullet)
        {
            _EnemyBullets.Add(bullet);
        }

        /// <summary>
        /// Adds player projectile to the player projectile list
        /// </summary>
        /// <param name="bullet"></param>
        public void RegisterPlayerBullet(Bullet bullet)
        {
            _PlayerBullets.Add(bullet);
        }

        /// <summary>
        /// Registers the player object to the collision controller
        /// </summary>
        /// <param name="player"></param>
        public void RegisterPlayer(Player player)
        {
            _Player = player;
        }

        /// <summary>
        /// Removes the given enemy from the enenmy list
        /// </summary>
        /// <param name="enemy"></param>
        public void DeregisterEnemy(BaseEnemy enemy)
        {
            _Enemies.Remove(enemy);
        }

        /// <summary>
        /// Removes the given bullet from the collision controller
        /// </summary>
        /// <param name="bullet"></param>
        public void DeregiterBullet(Bullet bullet)
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

        /// <summary>
        /// Clears all lists and deactivates the controller
        /// </summary>
        public void ClearObjects()
        {
            IsActive = false;
            foreach (BaseEnemy baseEnemy in _Enemies.ToList())
            {
                baseEnemy.Dispose();
            }
            _Enemies.Clear();
            ClearProjectiles();
            IsActive = false;
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
            foreach (BaseEnemy baseEnemy in _Enemies)
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
