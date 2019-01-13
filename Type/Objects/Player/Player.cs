using System;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Controllers;
using Type.Objects.Projectiles;
#if DESKTOP
using OpenTK.Input;
#endif

namespace Type.Objects.Player
{
    /// <summary>
    /// The player ship, can move and fire
    /// </summary>
    public class Player : GameObject
    {
        /// <summary> Default rate of fire </summary>
        private readonly TimeSpan _DefaultFireRate = TimeSpan.FromMilliseconds(100);
        /// <summary> Position on screen where the player is spawned </summary>
        private readonly Vector2 _SpawnPosition = new Vector2(-700, 0);
        /// <summary> Default movement speed </summary>
        private readonly Single _DefaultMovementSpeed = 500;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> The offset to the front of the ship, used for spawning bullets </summary>
        private Vector2 _BulletSpawnPos;

        private readonly Action OnDeath;

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
            _BulletSpawnPos = new Vector2(0, -GetSprite().Height / 4);
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
#if DESKTOP
            if (Keyboard.GetState().IsKeyDown(Key.Right))
            {
                if (Position.X <= (1920 / 2) - GetSprite().Width)
                {
                    Position += new Vector2(MovementSpeed * (Single)timeTilUpdate.TotalSeconds, 0);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Key.Left))
            {
                if (Position.X >= -(1920 / 2))
                {
                    Position -= new Vector2(MovementSpeed * (Single)timeTilUpdate.TotalSeconds, 0);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Key.Up))
            {
                if (Position.Y <= (1080 / 2) - GetSprite().Height)
                {
                    Position += new Vector2(0, MovementSpeed * (Single)timeTilUpdate.TotalSeconds);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Key.Down))
            {
                if (Position.Y >= -(1080 / 2))
                {
                    Position -= new Vector2(0, MovementSpeed * (Single)timeTilUpdate.TotalSeconds);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Key.Space))
            {
                if (_IsWeaponLocked) return;
                FireForward();
            }
#endif
        }
    }
}
