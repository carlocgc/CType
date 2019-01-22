using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Data;
using Type.Interfaces.Enemies;
using Type.Interfaces.Weapons;
using Type.Objects.Projectiles;
using static Type.Constants.Global;

namespace Type.Objects.Enemies
{
    /// <summary>
    /// Enemy of type beta
    /// </summary>
    public class EnemyBeta : GameObject, IEnemy
    {
        /// <summary> How long to wait before playing the hit sound</summary>
        private readonly TimeSpan _HitSoundInterval = TimeSpan.FromSeconds(0.2f); // TODO FIXME Work around to stop so many sounds playing
        /// <summary> How long since the last hit occured </summary>
        private TimeSpan _TimeSinceLastSound; // TODO FIXME Work around to stop so many sounds playing
        /// <summary> Whether a sound is playing </summary>
        private Boolean _IsSoundPlaying; // TODO FIXME Work around to stop so many sounds playing

        /// <summary> Animation of an explosion, played on death </summary>
        private readonly AnimatedSprite _Explosion;
        /// <summary> List of <see cref="IEnemyListener"/>'s </summary>
        private readonly List<IEnemyListener> _Listeners;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Firerate of the enemy </summary>
        private TimeSpan _FireRate;
        /// <summary> Direction the enemy is moving </summary>
        private Vector2 _MoveDirection;
        /// <summary> The players current position </summary>
        private Vector2 _PlayerPosition;
        /// <summary> Relative direction to the player from this enemy </summary>
        private Vector2 _DirectionTowardsPlayer;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Whether the enemy is moving </summary>
        private Boolean _IsMoving;
        /// <summary> movement speed of the enemy </summary>
        private Single _Speed;

        /// <summary> Whether the enemy is on screen </summary>
        private Boolean OnScreen => Position.X >= ScreenLeft && Position.Y >= ScreenBottom && Position.Y <= ScreenTop;
        /// <summary> Whether the enemy is on screen and can be hit </summary>
        public Boolean IsAlive { get; private set; }
        /// <summary> Point valuie for this enemy </summary>
        public Int32 Points { get; private set; }
        /// <inheritdoc />
        public Boolean AutoFire { get; set; }
        /// <inheritdoc />
        public Vector4 HitBox { get; set; }
        /// <inheritdoc />
        public Int32 HitPoints { get; private set; }

        public EnemyBeta(Vector2 spawnPos)
        {
            _Listeners = new List<IEnemyListener>();

            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, Texture.GetTexture("Content/Graphics/enemy1.png"))
            {
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
            };
            _Explosion.Offset = _Explosion.Size / 2;
            AddSprite(_Explosion);

            _IsMoving = true;
            _IsWeaponLocked = true;
            _MoveDirection = new Vector2(-1, 0);
            _Speed = 600;
            _FireRate = TimeSpan.FromSeconds(0.8f);

            HitBox = GetRect();
            HitPoints = 9;
            Points = 25;
            Position = spawnPos;
        }

        /// <inheritdoc />
        public void Shoot()
        {
            Vector2 bulletDirection = _DirectionTowardsPlayer;
            if (bulletDirection != Vector2.Zero) bulletDirection.Normalize();
            new PlasmaBall(Position, bulletDirection, 1000, new Vector4(255, 255, 0, 1));
            _IsWeaponLocked = true;

            new AudioPlayer("Content/Audio/laser3.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <inheritdoc />
        public void Hit(IProjectile projectile)
        {
            HitPoints -= projectile.Damage;

            if (!_IsSoundPlaying)
            {
                new AudioPlayer("Content/Audio/hurt3.wav", false, AudioManager.Category.EFFECT, 1);
                _IsSoundPlaying = true;
                _TimeSinceLastSound = TimeSpan.Zero;
            }

            if (HitPoints > 0) return;

            Destroy();
        }

        /// <inheritdoc />
        public void Destroy()
        {
            IsAlive = false;
            GameStats.Instance.EnemiesKilled++;

            _Explosion.AddFrameAction((anim) =>
            {
                Dispose();
            }, 8);
            _Sprite.Visible = false;
            _Explosion.Visible = true;
            _Explosion.Playing = true;

            PositionRelayer.Instance.RemoveRecipient(this);
        }

        /// <inheritdoc />
        public void UpdatePositionData(Vector2 position)
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

            Rotation = (Single)Math.Atan2(_DirectionTowardsPlayer.Y, _DirectionTowardsPlayer.X);
        }

        /// <inheritdoc />
        public void RegisterListener(IEnemyListener listener)
        {
            _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IEnemyListener listener)
        {
            _Listeners.Remove(listener);
        }

        /// <inheritdoc />
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
                    Shoot();
                }
                else
                {
                    _TimeSinceLastFired += timeTilUpdate;
                    if (_TimeSinceLastFired >= _FireRate)
                    {
                        _IsWeaponLocked = false;
                        _TimeSinceLastFired = TimeSpan.Zero;
                    }
                }
            }
            if (!_IsMoving) return;

            Position += _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

            if (!OnScreen)
            {
                IsAlive = false;
                foreach (IEnemyListener listener in _Listeners)
                {
                    listener.OnEnemyOffscreen(this);
                }
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Explosion.Dispose();
            _Listeners.Clear();
        }
    }
}
