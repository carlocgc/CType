using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Enemies;
using static Type.Constants.Global;

namespace Type.Objects.Bosses
{
    /// <summary>
    /// A boss: a body that advances to a stop and a set of destroyable cannons. Which one it is
    /// comes from its <see cref="BossDefinition"/>.
    /// </summary>
    /// <remarks>
    /// This replaces <c>BossFighter</c>, <c>BossFighterStrong</c>, <c>BossStation</c> and
    /// <c>BossStationStrong</c>, which were 273 to 278 lines each. Stripped of whitespace they
    /// were the same file: the four differed in the body texture and the cannon list and in
    /// nothing else - see E1 in ROADMAP.md.
    /// <para>
    /// **Deliberately not merged into <see cref="Objects.Enemies.Enemy"/>.** A wave enemy is a
    /// sprite that moves and shoots; a boss is a hull that advances to a stop, carries guns that
    /// are hit instead of it, and comes apart over three seconds when the last one dies. Its own
    /// <see cref="Hit"/> does nothing and it has no hit points. Folding the two together would put
    /// two behaviours in one class to save a file.
    /// </para>
    /// <para>
    /// E6 is the item that gives bosses phases, per-phase attack patterns and telegraphed
    /// transitions. It is one class and one table to change now rather than four files.
    /// </para>
    /// </remarks>
    public sealed class Boss : GameObject, IBoss, IEnemyListener
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
        /// <summary> The blasts that take the boss apart once its guns are gone </summary>
        private readonly BossDeathSequence _DeathSequence;

        /// <summary> Whether the boss is autofiring </summary>
        private Boolean _AutoFire;
        /// <summary> Whether the boss is moving onto screen </summary>
        private Boolean _IsAdvancing;
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

        /// <summary>
        /// Creates a new <see cref="Boss"/>
        /// </summary>
        /// <param name="definition"> Which boss this is </param>
        public Boss(BossDefinition definition)
        {
            _Listeners = new List<IEnemyListener>();
            _Cannons = new List<BossCannon>();
            _DeathSequence = new BossDeathSequence();

            _Body = new Sprite(Game.MainCanvas, Constants.ZOrders.BOSS_BASE, Texture.GetTexture(definition.Texture))
            {
                Visible = true,
            };
            Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 + _Body.Width / 2, 0);
            _Body.Offset = _Body.Size / 2;

            foreach (BossCannonDefinition cannon in definition.Cannons)
            {
                _Cannons.Add(new BossCannon(cannon.HitPoints, cannon.FireRate) { Offset = cannon.Offset });
            }

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
            if (_DeathSequence.IsRunning)
            {
                _DeathSequence.Update(timeTilUpdate, Position, _Body.Size);

                if (_DeathSequence.IsComplete)
                {
                    _Body.Visible = false;
                    for (Int32 i = _Listeners.Count - 1; i >= 0; i--)
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
            _DeathSequence.Start();
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
