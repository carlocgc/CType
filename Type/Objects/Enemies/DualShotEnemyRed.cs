using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Data;
using Type.Interfaces.Enemies;
using Type.Interfaces.Movement;
using Type.Objects.Projectiles;
using static Type.Constants.Global;

namespace Type.Objects.Enemies
{
    public class DualShotEnemyRed : GameObject, IEnemy
    {
        /// <summary> How long to wait before playing the hit sound</summary>
        private readonly TimeSpan _HitSoundInterval = TimeSpan.FromSeconds(0.2f); // TODO FIXME Work around to stop so many sounds playing
        /// <summary> How long since the last hit occured </summary>
        private TimeSpan _TimeSinceLastSound; // TODO FIXME Work around to stop so many sounds playing
        /// <summary> Whether a sound is playing </summary>
        private Boolean _IsSoundPlaying; // TODO FIXME Work around to stop so many sounds playing

        private readonly IAccelerationProvider _MovementController;
        /// <summary> Animation of an explosion, played on death </summary>
        private readonly AnimatedSprite _Explosion;
        /// <summary> List of <see cref="IEnemyListener"/>'s </summary>
        private readonly List<IEnemyListener> _Listeners;

        /// <summary> Callback used to change the colour back after being hit by a projectile </summary>
        private TimedCallback _ColourCallback;
        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Firerate of the enemy </summary>
        private TimeSpan _FireRate;
        /// <summary> The players current position </summary>
        private Vector2 _PlayerPosition;
        /// <summary> Relative direction to the player from this enemy </summary>
        private Vector2 _DirectionTowardsPlayer;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Whether the enemy is moving </summary>
        private Boolean _IsMoving;
        /// <summary> Whether the enemy has entered the game area </summary>
        private Boolean InPlay;

        /// <summary> Whether the enemy has been destroyed  </summary>
        public Boolean IsDestroyed { get; private set; }

        /// <summary> Point valuie for this enemy </summary>
        public Int32 Points { get; private set; }

        /// <inheritdoc />
        public Int32 HitPoints { get; private set; }

        /// <inheritdoc />
        public Boolean AutoFire { get; set; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <summary> Whether the enemy can be roadkilled </summary>
        public Boolean CanBeRoadKilled { get; }

        /// <summary> Whether the enemy is completely on screen, used to add the object to the collision controller </summary>
        public Boolean OnScreen =>
            Position.X - _Sprite.Offset.X >= ScreenLeft &&
            Position.X + _Sprite.Offset.X <= ScreenRight &&
            Position.Y - _Sprite.Offset.Y >= ScreenBottom &&
            Position.Y + _Sprite.Offset.Y <= ScreenTop;

        /// <summary> Whether the enemy is completely offscreen, used to destroy the object </summary>
        private Boolean OffScreen =>
            Position.X + _Sprite.Offset.X <= ScreenLeft ||
            Position.X - _Sprite.Offset.X >= ScreenRight ||
            Position.Y + _Sprite.Offset.Y <= ScreenBottom ||
            Position.Y - _Sprite.Offset.Y >= ScreenTop;

        public DualShotEnemyRed(Single yPos, IAccelerationProvider moveController)
        {
            _Listeners = new List<IEnemyListener>();

            _IsMoving = true;
            _IsWeaponLocked = true;
            _FireRate = TimeSpan.FromSeconds(1.7f);

            HitPoints = 3;
            Points = 25;
            CanBeRoadKilled = true;

            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, Texture.GetTexture("Content/Graphics/Enemies/enemy4.png"))
            {
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            _Sprite.RotationOrigin = _Sprite.Size / 2;
            AddSprite(_Sprite);

            HitBox = GetRect();

            _Explosion = new AnimatedSprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, new[]
            {
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion00.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion01.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion02.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion03.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion04.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion05.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion06.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion07.png"),
                Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion08.png"),
            }, 9)
            {
                Visible = false,
                Playing = false,
                AnimEndBehaviour = AnimatedSprite.EndBehaviour.STOP,
                CurrentFrame = 0,
            };
            _Explosion.Scale = new Vector2(2.1f, 2.1f);
            _Explosion.Offset = new Vector2(_Explosion.Size.X / 2 * _Explosion.Scale.X, _Explosion.Size.Y / 2 * _Explosion.Scale.Y);

            Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 + _Sprite.Offset.X, yPos);
            _Explosion.Position = Position;

            _MovementController = moveController;

            PositionRelayer.Instance.AddRecipient(this);
        }

        /// <inheritdoc />
        public void Shoot()
        {
            Vector2 bulletDirection = _DirectionTowardsPlayer;
            if (bulletDirection != Vector2.Zero) bulletDirection.Normalize();
            new PlasmaBall(Position, bulletDirection, 1050, new Vector4(100, 100, 0, 1));
            new PlasmaBall(Position, bulletDirection, 1050, new Vector4(100, 100, 0, 1));
            _IsWeaponLocked = true;
            new AudioPlayer("Content/Audio/laser2.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <inheritdoc />
        public void Hit(Int32 damage)
        {
            HitPoints -= damage;

            if (!_IsSoundPlaying)
            {
                new AudioPlayer("Content/Audio/hurt3.wav", false, AudioManager.Category.EFFECT, 1);
                _IsSoundPlaying = true;
                _TimeSinceLastSound = TimeSpan.Zero;
            }

            _Sprite.Colour = new Vector4(1.5f, 1.5f, 1.5f, 1);
            _ColourCallback?.CancelAndComplete();
            _ColourCallback = new TimedCallback(TimeSpan.FromMilliseconds(50), () => _Sprite.Colour = new Vector4(1, 1, 1, 1));

            if (HitPoints > 0) return;

            Destroy();
        }

        /// <inheritdoc />
        public void Destroy()
        {
            IsDestroyed = true;
            PositionRelayer.Instance.RemoveRecipient(this);
            CollisionController.Instance.DeregisterEnemy(this);

            GameStats.Instance.EnemiesKilled++;

            foreach (IEnemyListener listener in _Listeners)
            {
                listener.OnEnemyDestroyed(this);
            }

            new AudioPlayer("Content/Audio/explode.wav", false, AudioManager.Category.EFFECT, 1);
            _Explosion.AddFrameAction((anim) =>
            {
                Dispose();
            }, 8);
            _Sprite.Visible = false;
            _Explosion.Visible = true;
            _Explosion.Playing = true;
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
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);
            if (_IsMoving)
            {
                Position = _MovementController.ApplyAcceleration(Position, timeTilUpdate);

                _Explosion.Position = Position;
                HitBox = GetRect();
            }

            if (IsDestroyed) return;

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

            if (OnScreen && !InPlay) // If first time on screen
            {
                InPlay = true;
                CollisionController.Instance.RegisterEnemy(this);
            }
            else if (OffScreen && InPlay) // If alive and offscreen
            {
                for (Int32 i = _Listeners.Count - 1; i >= 0; i--)
                {
                    IEnemyListener listener = _Listeners[i];
                    listener.OnEnemyOffscreen(this);
                }
            }
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
        public override void Dispose()
        {
            _ColourCallback?.CancelAndComplete();
            base.Dispose();
            if (!_Explosion.IsDisposed) _Explosion.Dispose();

            _Listeners.Clear();
            CollisionController.Instance.DeregisterEnemy(this);
            PositionRelayer.Instance.RemoveRecipient(this);
        }
    }
}
