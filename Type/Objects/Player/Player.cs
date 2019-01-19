﻿using System;
using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Controllers;
using Type.Interfaces.Control;
using Type.Interfaces.Probe;
using Type.Objects.Projectiles;
#if DESKTOP
using OpenTK.Input;
#endif

namespace Type.Objects.Player
{
    /// <summary>
    /// The player ship, can move and fire
    /// </summary>
    public class Player : GameObject, IAnalogListener
    {
        private readonly Single ScreenTop = Renderer.Instance.TargetDimensions.Y / 2;
        private readonly Single ScreenRight = Renderer.Instance.TargetDimensions.X / 2;
        private readonly Single ScreenLeft = -Renderer.Instance.TargetDimensions.X / 2;
        private readonly Single ScreenBottom = -Renderer.Instance.TargetDimensions.Y / 2;

        /// <summary> Single point of contact for probes attached to  the player </summary>
        private readonly IProbeController _ProbeController;

        /// <summary> Default rate of fire </summary>
        private readonly TimeSpan _DefaultFireRate = TimeSpan.FromMilliseconds(100);
        /// <summary> Position on screen where the player is spawned </summary>
        private readonly Vector2 _SpawnPosition = new Vector2(-540, 0);
        /// <summary> Default movement speed </summary>
        private readonly Single _DefaultMovementSpeed = 700;
        /// <summary> Action to be invoked when the player dies </summary>
        private readonly Action OnDeath;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;

        private Vector2 _Direction;

        private Single _MoveStrength;

        private Boolean _Shoot;

        /// <summary> Whether the player should shoot </summary>
        public Boolean Shoot
        {
            get => _Shoot;
            set
            {
                _Shoot = value;
                _ProbeController.Shoot = value;
            }
        }

        /// <summary> Amount of time between firing </summary>
        public TimeSpan FireRate { get; set; }
        /// <summary> How fast the player can move in any direction </summary>
        public Single MovementSpeed { get; set; }

        /// <summary> Position of the player </summary>
        public Player(Action onDeath)
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/player.png"))
            {
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);

            Position = _SpawnPosition;
            MovementSpeed = _DefaultMovementSpeed;
            FireRate = _DefaultFireRate;

            OnDeath = onDeath;

            _ProbeController = new ProbeController();
            _ProbeController.UpdatePosition(Position);

            //CreateProbes(5); // TODO : Remove this method TEST

            CollisionController.Instance.RegisterPlayer(this);
        }

        /// <summary>
        /// Creates the given number of probes around the player ship
        /// </summary>
        /// <param name="amount"> Amount of probes to create </param>
        /// <param name="id"> The type of probe to create </param>
        public void CreateProbes(Int32 amount, Int32 id)
        {
            for (Int32 i = 0; i < amount; i++)
            {
                _ProbeController.AddProbe(0);
            }
        }

        /// <summary>
        /// Resets the position of the player ship
        /// </summary>
        public void LoseLife()
        {
            _ProbeController.RemoveAll();
            Position = _SpawnPosition;
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Creates a new Bullet
        /// </summary>
        private void FireForward()
        {
            new Bullet("Content/Graphics/bullet.png", Position + new Vector2(_Sprite.Width / 2, 0), new Vector2(1, 0), 1000, 0, true, new Vector4(1, 1, 1, 1));
            _IsWeaponLocked = true;
            new AudioPlayer("Content/Audio/laser1.wav", false, AudioManager.Category.EFFECT, 0.5f);
        }

        /// <summary>
        /// Position is updated every update if movement key press is detected
        /// bullet is fired if fire key press is detected and parameters are met
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            if (_IsWeaponLocked)
            {
                _TimeSinceLastFired += timeTilUpdate;
                if (_TimeSinceLastFired >= FireRate)
                {
                    _IsWeaponLocked = false;
                    _TimeSinceLastFired = TimeSpan.Zero;
                }
            }

            if (Shoot && !_IsWeaponLocked)
            {
                FireForward();
            }

            Position += GetPositionModifier(timeTilUpdate);
            _ProbeController.UpdatePosition(Position);
        }

        ///// <summary>
        ///// Returns whether the player can move in the current direction any further
        ///// </summary>
        ///// <returns></returns>
        //private Boolean CanMove()
        //{
        //    return _Direction.X > 0 && Position.X <= ScreenRight - GetSprite().Width ||
        //        _Direction.X < 0 && Position.X >= ScreenLeft ||
        //        _Direction.Y > 0 && Position.Y <= ScreenTop - GetSprite().Height ||
        //        _Direction.Y < 0 && Position.Y >= ScreenBottom;
        //}

        /// <summary>
        /// Returns how much the player should move by
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        /// <returns></returns>
        private Vector2 GetPositionModifier(TimeSpan timeTilUpdate)
        {
            Single x, y;

            if (_Direction.X > 0 && Position.X >= ScreenRight - GetSprite().Width /2 || _Direction.X < 0 && Position.X <= ScreenLeft + GetSprite().Width /2) x = 0;
            else x = _Direction.X * _MoveStrength * MovementSpeed * (Single)timeTilUpdate.TotalSeconds;

            if (_Direction.Y > 0 && Position.Y >= ScreenTop - GetSprite().Height /2 || _Direction.Y < 0 && Position.Y <= ScreenBottom + GetSprite().Height /2) y = 0;
            else y = _Direction.Y * _MoveStrength * MovementSpeed * (Single)timeTilUpdate.TotalSeconds;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Updates the direction and move strength of the player ship, provided by the analog stick
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> How far the stick is pushed </param>
        public void UpdatePosition(Vector2 direction, Single strength)
        {
            _Direction = direction;
            _MoveStrength = strength;
        }
    }
}
