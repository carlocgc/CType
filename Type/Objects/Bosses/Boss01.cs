using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using AmosShared.Base;
using Type.Base;
using Type.Controllers;
using Type.Interfaces;
using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.Enemies;

namespace Type.Objects.Bosses
{
    /// <summary>
    /// Boss that has three destroyable cannons
    /// </summary>
    public class Boss01 : GameObject, IEnemy, IPositionRecipient, IEnemyListener
    {
        private readonly List<BossCannon> _Cannons;

        private readonly Single _Speed;

        private readonly Vector2 _MoveDirection;

        /// <summary> List of <see cref="IEnemyListener"/>'s </summary>
        private readonly List<IEnemyListener> _Listeners;

        private BossCannon _TopCannon;

        private BossCannon _MiddleCannon;

        private BossCannon _BottomCannon;

        /// <summary> The players current position </summary>
        private Vector2 _PlayerPosition;
        /// <summary> Relative direction to the player from this enemy </summary>
        private Vector2 _DirectionTowardsPlayer;

        private Sprite _Sprite;

        private Boolean _IsMoving;

        private Vector2 _StopPosition;

        private Int32 _DestroyedCannons;

        /// <summary>
        /// The hitbox of the <see cref="ICollidable"/>
        /// </summary>
        public Vector4 HitBox { get; set; }

        /// <summary>
        /// The hitpoints of the <see cref="IHitable"/>
        /// </summary>
        public Int32 HitPoints { get; }

        /// <summary>
        /// Whether this object is Autofiring
        /// </summary>
        public Boolean AutoFire { get; set; }

        /// <summary> Amount of points this object is worth </summary>
        public Int32 Points { get; }

        /// <summary> The position of the object </summary>
        public Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;

                _Sprite.Position = value;
                foreach (BossCannon cannon in _Cannons)
                {
                    cannon.Position = value;
                }
            }
        }

        /// <summary> The rotation of the object </summary>
        public override Double Rotation
        {
            get => base.Rotation;
            set
            {
                base.Rotation = value;
                foreach (BossCannon cannon in _Cannons)
                {
                    cannon.Rotation = value;
                }
            }
        }

        public Boss01()
        {
            _Listeners = new List<IEnemyListener>();
            _Cannons = new List<BossCannon>();

            _MoveDirection = new Vector2(-1, 0);

            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.BOSS_BASE, Texture.GetTexture("Content/Graphics/Bosses/boss01.png"))
            {
                Visible = true,
            };
            Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 + _Sprite.Width / 2, 0);
            _Sprite.Offset = _Sprite.Size / 2;

            _TopCannon = new BossCannon(100, TimeSpan.FromMilliseconds(1500));
            _TopCannon.Offset = new Vector2(102, -130);
            _MiddleCannon = new BossCannon(100, TimeSpan.FromMilliseconds(1000));
            _MiddleCannon.Offset = new Vector2(-149, 0);
            _BottomCannon = new BossCannon(100, TimeSpan.FromMilliseconds(1500));
            _BottomCannon.Offset = new Vector2(102, 130);

            _Cannons.Add(_TopCannon);
            _Cannons.Add(_MiddleCannon);
            _Cannons.Add(_BottomCannon);

            foreach (BossCannon cannon in _Cannons)
            {
                cannon.RegisterListener(this);
                cannon.Position = Position;
                cannon.Visible = true;
            }

            _Speed = 250f;
            _StopPosition = new Vector2(Renderer.Instance.TargetDimensions.X / 4, 0);
            _IsMoving = true;

            PositionRelayer.Instance.AddRecipient(this);
        }

        /// <summary>
        /// Update position data
        /// </summary>
        /// <param name="position"> The received position data </param>
        public void UpdatePositionData(Vector2 position)
        {
            _PlayerPosition = position;
            UpdateRotation();
        }

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);
            if (_IsMoving)
            {
                Position += _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

                if (Position.X <= _StopPosition.X)
                {
                    _IsMoving = false;
                    foreach (BossCannon cannon in _Cannons)
                    {
                        //cannon.AutoFire = true;
                        CollisionController.Instance.RegisterEnemy(cannon);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the ship rotation so it is facing the players poistion
        /// </summary>
        private void UpdateRotation()
        {
            _DirectionTowardsPlayer = _PlayerPosition - Position;
            foreach (BossCannon cannon in _Cannons)
            {
                cannon.DirectionTowardsPlayer = _DirectionTowardsPlayer;
            }
            Rotation = (Single)Math.Atan2(_DirectionTowardsPlayer.Y, _DirectionTowardsPlayer.X);
        }

        /// <summary>
        /// Invoked when an enemy is Destroyed
        /// </summary>
        /// <param name="enemy"> The enemy that has been destroyed </param>
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            _DestroyedCannons++;
            if (_DestroyedCannons < _Cannons.Count) return;

            foreach (BossCannon cannon in _Cannons)
            {
                CollisionController.Instance.DeregisterEnemy(cannon);
                cannon.Dispose();
            }
        }

        /// <summary>
        /// Invoked when an enemy leaves the screen
        /// </summary>
        /// <param name="enemy"> The enemy that has left the screen </param>
        public void OnEnemyOffscreen(IEnemy enemy)
        {
        }

        /// <summary>
        /// Hit the <see cref="IHitable"/>
        /// </summary>
        public void Hit(Int32 damage)
        {
        }

        /// <summary>
        /// Destroy the object
        /// </summary>
        public void Destroy()
        {
        }

        /// <summary>
        /// Shoot a projectile
        /// </summary>
        public void Shoot()
        {
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

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            foreach (BossCannon cannon in _Cannons)
            {
                CollisionController.Instance.DeregisterEnemy(cannon);
                cannon.Dispose();
            }
            _Sprite.Dispose();
            PositionRelayer.Instance.RemoveRecipient(this);
        }
    }
}
