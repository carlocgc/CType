using System;
using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Controllers;
using Type.Interfaces.Control;
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
        /// <summary> Default rate of fire </summary>
        private readonly TimeSpan _DefaultFireRate = TimeSpan.FromMilliseconds(100);
        /// <summary> Position on screen where the player is spawned </summary>
        private readonly Vector2 _SpawnPosition = new Vector2(-700, 0);
        /// <summary> Default movement speed </summary>
        private readonly Single _DefaultMovementSpeed = 500;
        /// <summary> Action to be invoked when the player dies </summary>
        private readonly Action OnDeath;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Whether the player should move up </summary>
        private Boolean _MoveUp;
        /// <summary> Whether the player should move down </summary>
        private Boolean _MoveDown;
        /// <summary> Whether the player should move right </summary>
        private Boolean _MoveRight;
        /// <summary> Whether the player should move left </summary>
        private Boolean _MoveLeft;

        /// <summary> Whether the player should shoot </summary>
        public Boolean Shoot { get; set; }

        /// <summary> Amount of time between firing </summary>
        public TimeSpan FireRate { get; set; }
        /// <summary> How fast the player can move in any direction </summary>
        public Single MovementSpeed { get; set; }

        /// <summary> Position of the player </summary>
        public Player(Action onDeath)
        {
            AddSprite(new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/player.png"))
            {
                Visible = true,
            });
            Position = _SpawnPosition;
            MovementSpeed = _DefaultMovementSpeed;
            FireRate = _DefaultFireRate;

            OnDeath = onDeath;

            CollisionController.Instance.RegisterPlayer(this);
        }

        /// <summary>
        /// Resets the position of the player ship
        /// </summary>
        public void LoseLife()
        {
            Position = _SpawnPosition;
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Creates a new Bullet
        /// </summary>
        private void FireForward()
        {
            new Bullet("Content/Graphics/bullet.png", GetCenter(), new Vector2(1, 0), 1000, 0, true, new Vector4(1, 1, 1, 1));
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
            if (Shoot)
            {
                if (_IsWeaponLocked) return;
                FireForward();
            }
            if (_MoveRight)
            {
                if (Position.X <= (1920 / 2) - GetSprite().Width)
                {
                    Position += new Vector2(MovementSpeed * (Single)timeTilUpdate.TotalSeconds, 0);
                }
            }
            if (_MoveLeft)
            {
                if (Position.X >= -(1920 / 2))
                {
                    Position -= new Vector2(MovementSpeed * (Single)timeTilUpdate.TotalSeconds, 0);
                }
            }
            if (_MoveUp)
            {
                if (Position.Y <= (1080 / 2) - GetSprite().Height)
                {
                    Position += new Vector2(0, MovementSpeed * (Single)timeTilUpdate.TotalSeconds);
                }
            }
            if (_MoveDown)
            {
                if (Position.Y >= -(1080 / 2))
                {
                    Position -= new Vector2(0, MovementSpeed * (Single)timeTilUpdate.TotalSeconds);
                }
            }
        }

        public void UpdatePosition(Vector2 position)
        {
            _MoveUp = position.Y < -20;
            _MoveDown = position.Y > 20;
            _MoveRight = position.X < -20;
            _MoveLeft = position.X > 20;
        }
    }
}
