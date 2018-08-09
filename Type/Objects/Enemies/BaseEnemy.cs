using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Controllers;

namespace Type.Objects.Enemies
{
    /// <summary>
    /// Base for enemy objects
    /// </summary>
    public abstract class BaseEnemy : GameObject
    {
        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Amount of times the enemy can be hit before being destroyed </summary>
        private Int32 _HitPoints;

        /// <summary> Sprite for the Enemy </summary>
        protected Sprite _EnemySprite;
        /// <summary> The offset to the front of the ship, used for spawning bullets </summary>
        protected Vector2 _BulletSpawnPos;
        /// <summary> movement speed of the enemy </summary>
        protected Single _Speed;
        /// <summary> Direction the enemy is moving </summary>
        protected Vector2 _Direction;
        /// <summary> Whether the enemy is moving </summary>
        protected Boolean _IsMoving;
        /// <summary> Whether firing is allowed </summary>
        protected Boolean _IsWeaponLocked;
        /// <summary> Whether the enemy is allowed to fire projectiles </summary>
        protected Boolean _IsHostile;
        /// <summary> Whether the enemy has been detsroyed by the player </summary>
        protected Boolean _IsDestroyed;
        /// <summary> Called when the ship goes out of screen bounds </summary>
        public Action OnOutOfBounds;
        /// <summary> List of actions called when the ship is destroyed by the player </summary>
        public List<Action> OnDestroyedByPlayer;

        /// <summary> Whether the enemy is on screen and can be hit </summary>
        public Boolean IsAlive { get; set; }
        /// <summary> Point valuie for this enemy </summary>
        public Int32 PointValue { get; set; }

        /// <summary> amount of time between firing</summary>
        protected TimeSpan FireRate { get; set; }

        protected BaseEnemy(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate)
        {
            _EnemySprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, Texture.GetTexture(assetPath))
            {
                Position = spawnPos,
                Rotation = rotation,
                Visible = true,
            };
            AddSprite(_EnemySprite);

            Position = spawnPos;

            _BulletSpawnPos = new Vector2(-_EnemySprite.Width / 2, -_EnemySprite.Height / 2); ;

            _Direction = direction;
            _Speed = speed;
            FireRate = fireRate;

            OnDestroyedByPlayer = new List<Action>();

            _IsHostile = true;
            _IsMoving = true;
        }

        /// <summary>
        /// Fires a bullet
        /// </summary>
        protected abstract void Fire();

        /// <summary>
        /// Handles this ship being hit by a projectile
        /// </summary>
        public virtual void Hit()
        {
            if (!IsAlive) return;
            _HitPoints--;
            if (_HitPoints <= 0)
            {
                _IsDestroyed = true;
                Destroy();
            }
        }

        /// <summary>
        /// Destroys the Enemy
        /// </summary>
        protected virtual void Destroy()
        {
            IsAlive = false;
            if (_IsDestroyed)
            {
                foreach (Action action in OnDestroyedByPlayer)
                {
                    action?.Invoke();
                }
            }
            else
            {
                OnOutOfBounds?.Invoke();
            }

            Dispose();
        }

        /// <summary>
        /// Returns true if the enemy position is out of bounds of the screen
        /// </summary>
        /// <returns></returns>
        private Boolean CheckOutOfBounds()
        {
            return Position.X >= 2220 / 2 || Position.X + _EnemySprite.Width < -2220 / 2 ||
                   Position.Y + _EnemySprite.Width > 1280 / 2 || Position.Y < -1280 / 2;
        }

        /// <summary>
        /// Whether the enemy is on screen 
        /// </summary>
        /// <returns></returns>
        private Boolean CheckOnScreen()
        {
            return Position.X <= 1920 / 2 && Position.Y <= 1080 / 2 && Position.Y >= -1080 / 2;
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            if (IsAlive)
            {
                if (!_IsWeaponLocked)
                {
                    Fire();
                }
                else
                {
                    _TimeSinceLastFired += timeTilUpdate;
                    if (_TimeSinceLastFired >= FireRate)
                    {
                        _IsWeaponLocked = false;
                        _TimeSinceLastFired = TimeSpan.Zero;
                    }
                }
            }

            if (!_IsMoving) return;

            Position += _Direction * _Speed * (Single)timeTilUpdate.TotalSeconds;

            if (CheckOnScreen()) IsAlive = true;

            if (CheckOutOfBounds()) Destroy();
        }

        public override void Dispose()
        {
            base.Dispose();
            CollisionController.Instance.DeregisterEnemy(this);
        }
    }
}
