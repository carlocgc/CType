using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using AmosShared.Interfaces;
using Type.Base;
using Type.Controllers;
using Type.Data;
using Type.Glide;
using Type.Interfaces;
using Type.Interfaces.Control;

namespace Type.Objects.Enemies
{
    /// <summary>
    /// Base for enemy objects
    /// </summary>
    public abstract class BaseEnemy : GameObject, IHitable, IPositionRecipient
    {
        /// <summary> Animation of an explosion, played on death </summary>
        private readonly AnimatedSprite _Explosion;
        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Amount of times the enemy can be hit before being destroyed </summary>
        private Int32 _HitPoints;

        /// <summary> How long to wait before playing the hit sound</summary>
        private readonly TimeSpan _HitSoundInterval = TimeSpan.FromSeconds(0.2f); // TODO FIXME Work around to stop so many sounds playing
        /// <summary> How long since the last hit occured </summary>
        private TimeSpan _TimeSinceLastSound; // TODO FIXME Work around to stop so many sounds playing
        /// <summary> Whether a sound is playing </summary>
        private Boolean _IsSoundPlaying; // TODO FIXME Work around to stop so many sounds playing

        /// <summary> movement speed of the enemy </summary>
        protected Single _Speed;
        /// <summary> Direction the enemy is moving </summary>
        protected Vector2 _MoveDirection;
        /// <summary> Whether the enemy is moving </summary>
        protected Boolean _IsMoving;
        /// <summary> Whether firing is allowed </summary>
        protected Boolean _IsWeaponLocked;
        /// <summary> Whether the enemy is allowed to fire projectiles </summary>
        protected Boolean _IsHostile;
        /// <summary> Whether the enemy has been detsroyed by the player </summary>
        protected Boolean _IsDestroyed;

        /// <summary> The players current position </summary>
        protected Vector2 _PlayerPosition;
        /// <summary> Relative direction to the player from this enemy </summary>
        protected Vector2 _DirectionTowardsPlayer;

        /// <summary> Firerate of the enemy </summary>
        protected TimeSpan FireRate { get; set; }
        /// <summary> List of actions called when the ship is destroyed by the player </summary>
        public List<Action> OnDestroyedByPlayer { get; set; }
        /// <summary> Called when the ship goes out of screen bounds </summary>
        public Action OnOutOfBounds { get; set; }
        /// <summary> Whether the enemy is on screen and can be hit </summary>
        public Boolean IsAlive { get; set; }
        /// <summary> Point valuie for this enemy </summary>
        public Int32 PointValue { get; set; }

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                _Explosion.Position = value;
            }
        }

        protected BaseEnemy(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate, Int32 hitPoints)
        {
            OnDestroyedByPlayer = new List<Action>();

            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, Texture.GetTexture(assetPath))
            {
                Position = spawnPos,
                Rotation = rotation,
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            _Sprite.RotationOrigin = _Sprite.Size / 2;
            AddSprite(_Sprite);

            _Explosion = new AnimatedSprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, new[]
                {
                    Texture.GetTexture("Content/Graphics/Explosion/explosion00.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion01.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion02.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion03.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion04.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion05.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion06.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion07.png"),
                    Texture.GetTexture("Content/Graphics/Explosion/explosion08.png"),
                }, 9)
            {
                Visible = false,
                Playing = false,
                Position = _Sprite.Position,
                AnimEndBehaviour = AnimatedSprite.EndBehaviour.STOP,
                CurrentFrame = 0,
            };
            _Explosion.Offset = _Explosion.Size / 2;

            _IsHostile = true;
            _IsMoving = true;
            _MoveDirection = direction;
            _Speed = speed;
            _HitPoints = hitPoints;

            FireRate = fireRate;
            Position = spawnPos;

            _IsWeaponLocked = true;
        }

        /// <inheritdoc />
        public void Receive(Vector2 position)
        {
            _PlayerPosition = position;
            UpdateRotation();
        }

        /// <summary>
        /// Updates the ship rotation so it is facing the players poistion
        /// </summary>
        private void UpdateRotation()
        {
            _DirectionTowardsPlayer = _PlayerPosition - Position;
            //Rotation = (Single)(Math.Atan2(1, 0) - Math.Atan2(_DirectionTowardsPlayer.Y, _DirectionTowardsPlayer.X));
            Rotation = (Single) Math.Atan2(_DirectionTowardsPlayer.Y, _DirectionTowardsPlayer.X);
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

            if (!_IsSoundPlaying)
            {
                new AudioPlayer("Content/Audio/hurt3.wav", false, AudioManager.Category.EFFECT, 1);
                _IsSoundPlaying = true;
                _TimeSinceLastSound = TimeSpan.Zero;
            }

            if (_HitPoints > 0) return;

            _IsDestroyed = true;
            Destroy();
        }

        public void Collide()
        {
            _HitPoints = 0;
            _IsDestroyed = true;
            new AudioPlayer("Content/Audio/hurt3.wav", false, AudioManager.Category.EFFECT, 1);
            Destroy();
        }

        /// <summary>
        /// Destroys the Enemy
        /// </summary>
        public virtual void Destroy()
        {
            IsAlive = false;
            GameStats.Instance.EnemiesKilled++;

            if (_IsDestroyed)
            {
                _Explosion.AddFrameAction((anim) =>
                {
                    Dispose();
                }, 8);
                _Sprite.Visible = false;
                _Explosion.Visible = true;
                _Explosion.Playing = true;

                foreach (Action action in OnDestroyedByPlayer)
                {
                    action?.Invoke();
                }
            }
            else
            {
                _Explosion.RemoveAllFrameActions();
                OnOutOfBounds?.Invoke();
                Dispose();
            }

            PositionRelayer.Instance.RemoveRecipient(this);
        }

        /// <summary>
        /// Returns true if the enemy position is out of bounds of the screen
        /// </summary>
        /// <returns></returns>
        private Boolean CheckOutOfBounds()
        {
            return Position.X >= 2220 / 2 || Position.X + GetSprite().Width < -2220 / 2 ||
                   Position.Y + GetSprite().Width > 1280 / 2 || Position.Y < -1280 / 2;
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
                if (_IsSoundPlaying) // TODO FIXME Work around to limit sounds created
                {
                    _TimeSinceLastSound += timeTilUpdate;
                    if (_TimeSinceLastSound >= _HitSoundInterval)
                    {
                        _IsSoundPlaying = false;
                        _TimeSinceLastSound = TimeSpan.Zero;
                    }
                }

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

            Position += _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

            if (_IsDestroyed) return;

            if (CheckOnScreen()) IsAlive = true;

            if (CheckOutOfBounds()) Destroy();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (!_Explosion.IsDisposed) _Explosion.Dispose();
            CollisionController.Instance.DeregisterEnemy(this);
        }
    }
}
