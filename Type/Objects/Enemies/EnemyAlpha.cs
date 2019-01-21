using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Interfaces.Enemies;

namespace Type.Objects.Enemies
{
    public class EnemyAlpha : GameObject, IEnemy
    {
        /// <summary> How long to wait before playing the hit sound</summary>
        private readonly TimeSpan _HitSoundInterval = TimeSpan.FromSeconds(0.2f); // TODO FIXME Work around to stop so many sounds playing
        /// <summary> How long since the last hit occured </summary>
        private TimeSpan _TimeSinceLastSound; // TODO FIXME Work around to stop so many sounds playing
        /// <summary> Whether a sound is playing </summary>
        private Boolean _IsSoundPlaying; // TODO FIXME Work around to stop so many sounds playing

        /// <summary> Animation of an explosion, played on death </summary>
        private readonly AnimatedSprite _Explosion;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Firerate of the enemy </summary>
        private TimeSpan _FireRate;
        /// <summary> Whether the enemy is moving </summary>
        private Boolean _IsMoving;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Direction the enemy is moving </summary>
        private Vector2 _MoveDirection;
        /// <summary> The players current position </summary>
        private Vector2 _PlayerPosition;
        /// <summary> Relative direction to the player from this enemy </summary>
        private Vector2 _DirectionTowardsPlayer;
        /// <summary> movement speed of the enemy </summary>
        private Single _Speed;

        /// <summary> Whether the hitbox is a circular </summary>
        public Boolean IsCircleHitBox { get; private set; }
        /// <summary> List of actions called when the ship is destroyed by the player </summary>
        public List<Action> OnDestroyedByPlayer { get; set; }
        /// <summary> Called when the ship goes out of screen bounds </summary>
        public Action OnOutOfBounds { get; set; }
        /// <summary> Whether the enemy is on screen and can be hit </summary>
        public Boolean IsAlive { get; set; }
        /// <summary> Point valuie for this enemy </summary>
        public Int32 PointValue { get; set; }

        /// <inheritdoc />
        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                _Explosion.Position = value;
            }
        }

        
        /// <inheritdoc />
        public Boolean AutoFire { get; set; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <inheritdoc />
        public Int32 HitPoints { get; }

        public EnemyAlpha(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate, Int32 hitPoints)
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, Texture.GetTexture(assetPath))
            {
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
            };
            _Explosion.Offset = _Explosion.Size / 2;

            _IsWeaponLocked = true;
            _MoveDirection = direction;
            _Speed = speed;
            _FireRate = fireRate;
            HitPoints = hitPoints;
            Position = spawnPos;
        }

        /// <inheritdoc />
        public void UpdatePositionData(Vector2 position)
        {

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
        public void Hit()
        {

        }

        /// <inheritdoc />
        public void Destroy()
        {

        }

        /// <inheritdoc />
        public void Shoot()
        {

        }
    }
}
