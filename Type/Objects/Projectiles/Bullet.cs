using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Base;
using Type.Controllers;
using Type.Interfaces;

namespace Type.Objects.Projectiles
{
    /// <summary>
    /// Bullet object, once created it will move in the given direction at the given speed until out of screen bounds
    /// </summary>
    public class Bullet : GameObject, IDestroyable
    {
        /// <summary> The speed of the bullet </summary>
        private readonly Single _Velocity;
        /// <summary> Direction of the bullet </summary>
        private readonly Vector2 _Direction;
        /// <summary> Whether this bullet was fired from the player ship </summary>
        private readonly Boolean _IsPlayerBullet;

        public Bullet(String assetPath, Vector2 spawnPos, Vector2 direction, Single velocity, Double rotation, Boolean isPlayerBullet, Vector4 colour)
        {
            AddSprite(new Sprite(Game.MainCanvas, Constants.ZOrders.BULLETS, Texture.GetTexture(assetPath))
            {
                Visible = true,
                Colour = colour,
            });
            _Direction = direction;
            _Velocity = velocity;
            _IsPlayerBullet = isPlayerBullet;
            Position = spawnPos;
            Rotation = rotation;
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
        public void Destroy()
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
