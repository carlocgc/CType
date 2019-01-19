using System;
using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Controllers;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Control;
using Type.Interfaces.Probe;
using Type.Objects.Projectiles;

namespace Type.Objects.Player
{
    /// <summary>
    /// The player ship, can move and fire
    /// </summary>
    public class Player : GameObject, IAnalogListener
    {
        /// <summary> The top of the screen </summary>
        private readonly Single ScreenTop = Renderer.Instance.TargetDimensions.Y / 2;
        /// <summary> The right of the screen </summary>
        private readonly Single ScreenRight = Renderer.Instance.TargetDimensions.X / 2;
        /// <summary> The left of the screen </summary>
        private readonly Single ScreenLeft = -Renderer.Instance.TargetDimensions.X / 2;
        /// <summary> The bottom of the screen </summary>
        private readonly Single ScreenBottom = -Renderer.Instance.TargetDimensions.Y / 2;

        /// <summary> Single point of contact for probes attached to  the player </summary>
        private readonly IProbeController _ProbeController;
        /// <summary> The players shield </summary>
        private readonly IShield _Shield;
        /// <summary> Position on screen where the player is spawned </summary>
        private readonly Vector2 _SpawnPosition = new Vector2(-540, 0);
        /// <summary> Amount of time between firing </summary>
        private readonly TimeSpan _FireRate;
        /// <summary> How fast the player can move in any direction </summary>
        private readonly Single _MovementSpeed;
        /// <summary> Action to be invoked when the player dies </summary>
        private readonly Action OnDeath;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> The direction to be applied to the position of the player </summary>
        private Vector2 _Direction;
        /// <summary> Movement speed modifier </summary>
        private Single _MoveStrength;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Whether the player should shoot </summary>
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

        /// <summary>
        /// The player ship
        /// </summary>
        /// <param name="onDeath"> Action to be invoked when the player is killed </param>
        public Player(Action onDeath)
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/player.png"))
            {
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);

            Position = _SpawnPosition;
            _MovementSpeed = 700;
            _FireRate = TimeSpan.FromMilliseconds(100);

            OnDeath = onDeath;

            _ProbeController = new ProbeController();
            _ProbeController.UpdatePosition(Position);

            _Shield = new Shield();
            _Shield.UpdatePosition(Position);

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
            new AudioPlayer("Content/Audio/upgrade1.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <summary>
        /// Increases the shield level
        /// </summary>
        public void AddShield()
        {
            _Shield.Increase();
        }

        /// <summary>
        /// Creates a new Bullet
        /// </summary>
        private void FireForward()
        {
            new Bullet("Content/Graphics/bullet.png", Position + new Vector2(_Sprite.Width / 2, 0), new Vector2(1, 0), 1000, 0, true, new Vector4(1, 1, 1, 1));
            _IsWeaponLocked = true;
            GameStats.Instance.BulletsFired++;
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
                if (_TimeSinceLastFired >= _FireRate)
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
            _Shield.UpdatePosition(Position);
        }

        /// <summary>
        /// Returns how much the player should move by
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        /// <returns></returns>
        private Vector2 GetPositionModifier(TimeSpan timeTilUpdate)
        {
            Single x, y;

            if (_Direction.X > 0 && Position.X >= ScreenRight - GetSprite().Width / 2 || _Direction.X < 0 && Position.X <= ScreenLeft + GetSprite().Width / 2) x = 0;
            else x = _Direction.X * _MoveStrength * _MovementSpeed * (Single)timeTilUpdate.TotalSeconds;

            if (_Direction.Y > 0 && Position.Y >= ScreenTop - GetSprite().Height / 2 || _Direction.Y < 0 && Position.Y <= ScreenBottom + GetSprite().Height / 2) y = 0;
            else y = _Direction.Y * _MoveStrength * _MovementSpeed * (Single)timeTilUpdate.TotalSeconds;

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

        public void Hit(Action onDeath)
        {
            if (_Shield.IsActive)
            {
                _Shield.Decrease();
                return;
            }
            LoseLife(onDeath);
        }

        /// <summary>
        /// Resets the position of the player ship
        /// </summary>
        private void LoseLife(Action onDeath)
        {
            _ProbeController.RemoveAll();
            Position = _SpawnPosition;
            GameStats.Instance.Deaths++;

            onDeath.Invoke();
            OnDeath?.Invoke();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _Shield.Dispose();
            _ProbeController.Dispose();
        }
    }
}
