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
    /// <summary>
    /// A wave enemy. What kind of one it is comes from its <see cref="EnemyDefinition"/>.
    /// </summary>
    /// <remarks>
    /// This replaces <c>SmallEnemyWeak</c>, <c>SmallEnemyStrong</c>, <c>MediumEnemyWeak</c>,
    /// <c>MediumEnemyStrong</c>, <c>LargeEnemyWeak</c> and <c>LargeEnemyStrong</c>, which were
    /// 228 lines each and differed only in the seven values now held by
    /// <see cref="EnemyDefinition"/> - see E1 in ROADMAP.md.
    /// <para>
    /// Every enemy in the game still shares one behaviour: turn to face the player, fire a plasma
    /// ball on a timer. That was true of the six classes too. E2 and E3 are what change it, and
    /// they are much cheaper to do here than they were across six copies.
    /// </para>
    /// </remarks>
    public class Enemy : GameObject, IEnemy
    {
        /// <summary> What kind of enemy this is </summary>
        private readonly EnemyDefinition _Definition;
        /// <summary> Provider of this enemys motion, null if it does not move </summary>
        private readonly IAccelerationProvider _MovementController;
        /// <summary> List of <see cref="IEnemyListener"/>'s </summary>
        private readonly List<IEnemyListener> _Listeners;

        /// <summary> Callback used to change the colour back after being hit by a projectile </summary>
        private TimedCallback _ColourCallback;
        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> The players current position </summary>
        private Vector2 _PlayerPosition;
        /// <summary> Relative direction to the player from this enemy </summary>
        private Vector2 _DirectionTowardsPlayer;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Whether the enemy is moving </summary>
        private Boolean _IsMoving;

        /// <summary> Whether the enemy has been destroyed  </summary>
        public Boolean IsDestroyed { get; set; }

        /// <inheritdoc />
        public Int32 HitPoints { get; private set; }

        /// <summary> Point value for this enemy </summary>
        public Int32 Points { get; private set; }

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
        private Boolean OffScreen => Position.X + _Sprite.Offset.X <= ScreenLeft || Position.X - _Sprite.Offset.X >= ScreenRight;

        /// <summary>
        /// Creates a new <see cref="Enemy"/>
        /// </summary>
        /// <param name="definition"> What kind of enemy this is </param>
        /// <param name="yPos"> Y position the enemy enters the screen at </param>
        /// <param name="moveController"> Provider of this enemys motion, null if it does not move </param>
        public Enemy(EnemyDefinition definition, Single yPos, IAccelerationProvider moveController)
        {
            _Definition = definition;
            _Listeners = new List<IEnemyListener>();

            _IsMoving = true;
            _IsWeaponLocked = true;

            HitPoints = _Definition.HitPoints;
            Points = _Definition.Points;
            CanBeRoadKilled = true;

            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ENEMIES, Texture.GetTexture(_Definition.Texture))
            {
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            _Sprite.RotationOrigin = _Sprite.Size / 2;
            AddSprite(_Sprite);

            HitBox = GetRect();

            Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 + _Sprite.Offset.X / 2 - 1, yPos);

            _MovementController = moveController;

            PositionRelayer.Instance.AddRecipient(this);
        }

        /// <inheritdoc />
        public void Shoot()
        {
            Vector2 bulletDirection = _DirectionTowardsPlayer;
            if (bulletDirection != Vector2.Zero) bulletDirection.Normalize();
            new PlasmaBall(Position, bulletDirection, _Definition.ProjectileSpeed, _Definition.ProjectileColour);

            _IsWeaponLocked = true;
            _Definition.ShotSound();
        }

        /// <inheritdoc />
        public void Hit(Int32 damage)
        {
            HitPoints -= damage;

            Sounds.Hit();

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

            Sounds.Destroyed();
            _Sprite.Visible = false;
            Dispose();
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

                HitBox = GetRect();
            }

            if (IsDestroyed) return;

            if (!_IsWeaponLocked)
            {
                Shoot();
            }
            else
            {
                _TimeSinceLastFired += timeTilUpdate;
                if (_TimeSinceLastFired >= _Definition.FireRate)
                {
                    _IsWeaponLocked = false;
                    _TimeSinceLastFired = TimeSpan.Zero;
                }
            }

            if (OnScreen && !CollisionController.Instance.Enemies.Contains(this))
            {
                CollisionController.Instance.RegisterEnemy(this);
            }
            if (OffScreen)
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

            _Listeners.Clear();
            CollisionController.Instance.DeregisterEnemy(this);
            PositionRelayer.Instance.RemoveRecipient(this);
        }
    }
}
