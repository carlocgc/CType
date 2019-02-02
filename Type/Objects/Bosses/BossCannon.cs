using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Collisions;
using Type.Interfaces.Enemies;
using Type.Objects.Projectiles;

namespace Type.Objects.Bosses
{
    public class BossCannon : GameObject, IEnemy
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
        /// <summary> Sprite for the gun base </summary>
        private Sprite _Base;
        /// <summary> Sprite for the cannon </summary>
        private Sprite _Gun;

        /// <summary> Whether the cannon is visible </summary>
        private Boolean _Visible;

        /// <summary> Whether the cannon is visible </summary>
        public Boolean Visible
        {
            get => _Visible;
            set
            {
                _Visible = value;
                _Gun.Visible = _Visible;
                _Base.Visible = _Visible;
            }
        }

        /// <summary> The position of the object </summary>
        public Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value + Offset;
                _Gun.Position = value + Offset;
                _Base.Position = value + Offset;
                _Explosion.Position = value + Offset;
            }
        }

        /// <summary> The rotation of the object </summary>
        public override Double Rotation
        {
            get => base.Rotation;
            set
            {
                base.Rotation = value;
                _Gun.Rotation = value;
            }
        }

        /// <summary> The direction towards the player, used to point the cannon in that direction </summary>
        public Vector2 DirectionTowardsPlayer
        {
            get => _DirectionTowardsPlayer;
            set => _DirectionTowardsPlayer = value;
        }

        /// <summary>
        /// The hitbox of the <see cref="ICollidable"/>
        /// </summary>
        public Vector4 HitBox
        {
            get => new Vector4(_Base.Position.X - _Base.Offset.X * _Base.Scale.X, _Base.Position.Y - _Base.Offset.Y * _Base.Scale.Y,
                _Base.Width * _Base.Scale.X, _Base.Height * _Base.Scale.Y);
            set { }
        }

        /// <summary> Whether the enemy has been destroyed  </summary>
        public Boolean IsDestroyed { get; private set; }

        /// <summary>
        /// The hitpoints of the <see cref="IHitable"/>
        /// </summary>
        public Int32 HitPoints { get; private set; }

        /// <summary> Offset of the cannon </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Whether this object is Autofiring
        /// </summary>
        public Boolean AutoFire { get; set; }

        /// <summary> Amount of points this object is worth </summary>
        public Int32 Points { get; }

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        public BossCannon(Int32 hitPoints, TimeSpan fireRate)
        {
            _Listeners = new List<IEnemyListener>();

            HitPoints = hitPoints;
            _FireRate = fireRate;

            _Gun = new Sprite(Game.MainCanvas, Constants.ZOrders.BOSS_UPPER, Texture.GetTexture("Content/Graphics/Bosses/boss-cannon.png"));
            _Gun.Offset = _Gun.Size / 2;
            _Gun.RotationOrigin = _Gun.Size / 2;

            _Base = new Sprite(Game.MainCanvas, Constants.ZOrders.BOSS_LOWER, Texture.GetTexture("Content/Graphics/Bosses/boss01-gun-base.png"));
            _Base.Offset = _Base.Size / 2;

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
            }, 8)
            {
                Visible = false,
                Playing = false,
                AnimEndBehaviour = AnimatedSprite.EndBehaviour.STOP,
                CurrentFrame = 0,
            };
            _Explosion.Scale = new Vector2(2, 2);
            _Explosion.Offset = new Vector2(_Explosion.Size.X / 2 * _Explosion.Scale.X, _Explosion.Size.Y / 2 * _Explosion.Scale.Y);
        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <summary>
        /// Shoot a projectile
        /// </summary>
        public void Shoot()
        {
            Vector2 bulletDirection = _DirectionTowardsPlayer;
            if (bulletDirection != Vector2.Zero) bulletDirection.Normalize();
            new PlasmaBall(Position, bulletDirection, 1000, new Vector4(100, 0, 0, 1));
            _IsWeaponLocked = true;
            new AudioPlayer("Content/Audio/laser2.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
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

            if (AutoFire)
            {
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
        }

        /// <summary>
        /// Hit the <see cref="IHitable"/>
        /// </summary>
        public void Hit(Int32 damage)
        {
            if (IsDestroyed) return;

            HitPoints -= damage;

            if (!_IsSoundPlaying)
            {
                new AudioPlayer("Content/Audio/hurt3.wav", false, AudioManager.Category.EFFECT, 1);
                _IsSoundPlaying = true;
                _TimeSinceLastSound = TimeSpan.Zero;
            }

            _Gun.Colour = new Vector4(1.5f, 1.5f, 1.5f, 1);
            _ColourCallback?.CancelAndComplete();
            _ColourCallback = new TimedCallback(TimeSpan.FromMilliseconds(50), () => _Gun.Colour = new Vector4(1, 1, 1, 1));

            if (HitPoints > 0) return;

            Destroy();
        }

        /// <summary>
        /// Destroy the object
        /// </summary>
        public void Destroy()
        {
            IsDestroyed = true;
            AutoFire = false;

            _Gun.Visible = false;

            _Explosion.AddFrameAction((anim) =>
            {
                for (var i = _Listeners.Count - 1; i >= 0; i--)
                {
                    IEnemyListener listener = _Listeners[i];
                    listener.OnEnemyDestroyed(this);
                }
                Dispose();
            }, 8);
            _Explosion.Visible = true;
            _Explosion.Playing = true;
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

        /// <summary>
        /// Update position data
        /// </summary>
        /// <param name="position"> The received position data </param>
        public void UpdatePositionData(Vector2 position)
        {
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            _ColourCallback?.CancelAndComplete();
            base.Dispose();

            _Gun.Dispose();
            _Base.Dispose();
            if (!_Explosion.IsDisposed) _Explosion.Dispose();
            CollisionController.Instance.DeregisterEnemy(this);
            _Listeners.Clear();
        }
    }
}
