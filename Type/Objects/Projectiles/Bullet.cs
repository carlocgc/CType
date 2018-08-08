using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Controllers;

namespace Type.Objects.Projectiles
{
    /// <summary>
    /// Bullet object, once created it will move in the given direction at the given speed until out of screen bounds
    /// </summary>
    public class Bullet : GameObject
    {
        /// <summary> Positional offset compared to the object that created the bullet </summary>
        private Vector2 _PositionOffset;
        /// <summary> The speed of the bullet </summary>
        private Single _Velocity;
        /// <summary> Angle of the bullet graphic </summary>
        private Single _Rotation;
        /// <summary> Direction of the bullet </summary>
        private Vector2 _Direction;
        /// <summary> Whether this bullet was fired from the player ship </summary>
        private Boolean _IsPlayerBullet;

        /// <summary>
        /// Creates a bullet
        /// </summary>
        /// <param name="currentPosition"> Position of the creator </param>
        /// <param name="offset"> Correction to the position </param>
        /// <param name="direction"> Direction of the bullet </param>
        /// <param name="velocity"> SPeed multiplier </param>
        /// <param name="rotation"> Rotation of the bullet graphic </param>
        /// <param name="addbulletWidth"> Wether to add the bullet width to the offset, used for centering the bullet on certain ships </param>
        public Bullet(String assetPath, Vector2 currentPosition, Vector2 offset, Vector2 direction, Single velocity, Single rotation, Boolean isPlayerBullet, Boolean addbulletWidth = false)
        {
            _Rotation = rotation;
            _Direction = direction;
            _Velocity = velocity;

            AddSprite(new Sprite(Game.MainCanvas, Constants.ZOrders.BULLETS, Texture.GetTexture(assetPath))
            {
                Visible = true,
                Rotation = _Rotation,
            });

            _IsPlayerBullet = isPlayerBullet;

            _PositionOffset = offset;
            if (addbulletWidth)
            {
                _PositionOffset += new Vector2(0, +GetSprite().Width / 2);
            }
            else
            {
                _PositionOffset += new Vector2(0, -GetSprite().Width / 2);
            }
            Position = currentPosition - _PositionOffset;

            RegisterCollidable();
        }

        /// <summary>
        /// Registers this bullet with the collidable controller
        /// </summary>
        private void RegisterCollidable()
        {
            if (_IsPlayerBullet) CollisionController.Instance.RegisterPlayerBullet(this);
            else CollisionController.Instance.RegisterEnemyBullet(this);
        }

        /// <summary>
        /// Returns true if the bullets position is out of bounds of the screen
        /// </summary>
        /// <returns></returns>
        private Boolean CheckOutOfBounds()
        {
            return Position.X >= 2220 / 2 || Position.X + GetSprite().Width < -2220 / 2 ||
                   Position.Y + GetSprite().Width > 1280 / 2 || Position.Y < -1280 / 2;
        }

        /// <summary>
        /// Disposes the bullet
        /// </summary>
        private void Destroy()
        {
            CollisionController.Instance.DeregiterBullet(this);
            Dispose();
        }

        /// <summary>
        /// Updates the position every update and destroys itself if out of bounds of the screen
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            Position += _Velocity * _Direction * (Single)timeTilUpdate.TotalSeconds;

            if (CheckOutOfBounds()) Destroy();
        }
    }
}
