using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Interfaces;
using Type.Interfaces.Enemies;
using static Type.Constants.Global;

namespace Type.Objects.Bosses
{
    /// <summary>
    /// Boss that has three destroyable cannons
    /// </summary>
    public sealed class BossFighterStrong : GameObject, IEnemy, IEnemyListener
    {
        /// <summary> List of <see cref="IEnemyListener"/>'s </summary>
        private readonly List<IEnemyListener> _Listeners;
        /// <summary> List of the destroyable cannons on the boss </summary>
        private readonly List<BossCannon> _Cannons;
        /// <summary> Movement speed </summary>
        private readonly Single _Speed;
        /// <summary> Move direction </summary>
        private readonly Vector2 _MoveDirection;
        /// <summary> Sprite for the boss body </summary>
        private readonly Sprite _Body;

        /// <summary> Whether the boss is autofiring </summary>
        private Boolean _AutoFire;
        /// <summary> Whether the boss is moving onto screen </summary>
        private Boolean _IsAdvancing;
        /// <summary> Whether the boss is moving off the screen </summary>
        private Boolean _IsRetreating;
        /// <summary> Where the boss should stop when moving onto screen</summary>
        private Vector2 _StopPosition;
        /// <summary> The players current position </summary>
        private Vector2 _PlayerPosition;

        /// <summary> Whether the enemy is on screen </summary>
        public Boolean OnScreen =>
            Position.X - _Sprite.Offset.X >= ScreenLeft &&
            Position.X + _Sprite.Offset.X <= ScreenRight &&
            Position.Y - _Sprite.Offset.Y >= ScreenBottom &&
            Position.Y + _Sprite.Offset.Y <= ScreenTop;

        /// <summary> The position of the object </summary>
        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                _Body.Position = value;
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

        /// <inheritdoc />
        public Boolean AutoFire
        {
            get => _AutoFire;
            set
            {
                _AutoFire = value;
                foreach (BossCannon cannon in _Cannons)
                {
                    cannon.AutoFire = _AutoFire;
                }
            }
        }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <summary> Whether the enemy can be roadkilled </summary>
        public Boolean CanBeRoadKilled { get; }

        /// <summary> The hitpoints of the <see cref="IHitable"/> </summary>
        public Int32 HitPoints { get; }

        /// <summary> Amount of points this object is worth </summary>
        public Int32 Points { get; }

        public BossFighterStrong()
        {
            _Listeners = new List<IEnemyListener>();
            _Cannons = new List<BossCannon>();

            _Body = new Sprite(Game.MainCanvas, Constants.ZOrders.BOSS_BASE, Texture.GetTexture("Content/Graphics/Bosses/boss03.png"))
            {
                Visible = true,
            };
            Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 + _Body.Width / 2, 0);
            _Body.Offset = _Body.Size / 2;

            _Cannons.Add(new BossCannon(75, TimeSpan.FromMilliseconds(1400)) { Offset = new Vector2(113, -200) });
            _Cannons.Add(new BossCannon(100, TimeSpan.FromMilliseconds(1100)) { Offset = new Vector2(102, -130) });
            _Cannons.Add(new BossCannon(125, TimeSpan.FromMilliseconds(1000)) { Offset = new Vector2(-149, 0) });
            _Cannons.Add(new BossCannon(100, TimeSpan.FromMilliseconds(1100)) { Offset = new Vector2(102, 130) });
            _Cannons.Add(new BossCannon(75, TimeSpan.FromMilliseconds(1400)) { Offset = new Vector2(113, 200) });

            foreach (BossCannon cannon in _Cannons)
            {
                cannon.RegisterListener(this);
                cannon.Position = Position;
                cannon.Visible = true;
            }

            Points = 20000;
            CanBeRoadKilled = false;
            _Speed = 250f;
            _IsAdvancing = true;
            _MoveDirection = new Vector2(-1, 0);
            _StopPosition = new Vector2(Renderer.Instance.TargetDimensions.X / 4, 0);

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

        /// <summary>
        /// Updates the ship rotation so it is facing the players poistion
        /// </summary>
        private void UpdateRotation()
        {
            foreach (BossCannon cannon in _Cannons)
            {
                cannon.DirectionTowardsPlayer = _PlayerPosition - cannon.Position;
                cannon.Rotation = (Single)Math.Atan2(cannon.DirectionTowardsPlayer.Y, cannon.DirectionTowardsPlayer.X);
            }
        }

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            if (_IsAdvancing)
            {
                Position += _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

                if (Position.X <= _StopPosition.X)
                {
                    _IsAdvancing = false;
                    AutoFire = true;
                    foreach (BossCannon cannon in _Cannons)
                    {
                        CollisionController.Instance.RegisterEnemy(cannon);
                    }
                }
            }
            if (_IsRetreating)
            {
                Position -= _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

                if (Position.X - _Body.Width / 2 > Renderer.Instance.TargetDimensions.X / 2 && _IsRetreating)
                {
                    _IsRetreating = false;
                    for (var i = _Listeners.Count - 1; i >= 0; i--)
                    {
                        IEnemyListener listener = _Listeners[i];
                        listener.OnEnemyDestroyed(this);
                    }
                    Dispose();
                }
            }
        }

        /// <summary>
        /// Invoked when an enemy is Destroyed
        /// </summary>
        /// <param name="enemy"> The enemy that has been destroyed </param>
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            _Cannons.Remove(enemy as BossCannon);
            if (_Cannons.Count != 0) return;
            _IsRetreating = true;
        }

        #region Unusued Interfaces

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
        /// Whether the enemy is destroyed
        /// </summary>
        public Boolean IsDestroyed { get; set; }

        /// <summary>
        /// Destroy the object
        /// </summary>
        public void Destroy()
        {
            IsDestroyed = true;
        }

        /// <summary>
        /// Shoot a projectile
        /// </summary>
        public void Shoot()
        {
        }

        #endregion

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
            _Listeners.Clear();
            foreach (BossCannon cannon in _Cannons) cannon.Dispose();
            _Cannons.Clear();
            _Body.Dispose();
            PositionRelayer.Instance.RemoveRecipient(this);
        }
    }
}
