using OpenTK;
using System;
using AmosShared.Audio;
using AmosShared.Base;
using Type.Controllers;
using Type.Data;
using Type.Objects.Enemies;
using Type.Objects.Projectiles;

namespace Type.Objects.Bosses
{
    public class BossA : BaseEnemy
    {
        /// <summary> Point on screen where the boss stops </summary>
        private Vector2 _StopPosition;

        /// <inheritdoc />
        public BossA(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate, Int32 hitPoints) : base(assetPath, spawnPos, rotation, direction, speed, fireRate, hitPoints)
        {
            PointValue = 5000;

            Position += new Vector2(_Sprite.Width / 3, 0);
            _StopPosition = new Vector2(Renderer.Instance.TargetDimensions.X / 4, 0);

            _Explosion.Scale = new Vector2(9,9);
            _Explosion.Offset = new Vector2( _Explosion.Size.X / 2 * _Explosion.Scale.X, _Explosion.Size.Y / 2 * _Explosion.Scale.Y);

            _IsBoss = true;
        }

        /// <inheritdoc />
        protected override void Fire()
        {
            if (!_IsHostile) return;

            Vector2 bulletDirection = _DirectionTowardsPlayer;
            if (bulletDirection != Vector2.Zero) bulletDirection.Normalize();

            new Bullet("Content/Graphics/enemybullet.png", Position, bulletDirection, 1050, Rotation, false, new Vector4(255, 0, 255, 1));
            new Bullet("Content/Graphics/enemybullet.png", new Vector2(Position.X, Position.Y - 100), bulletDirection, 1050, Rotation, false, new Vector4(255, 0, 255, 1));
            new Bullet("Content/Graphics/enemybullet.png", new Vector2(Position.X, Position.Y  + 100), bulletDirection, 1050, Rotation, false, new Vector4(255, 0, 255, 1));

            _IsWeaponLocked = true;
            new AudioPlayer("Content/Audio/laser4.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeTilUpdate)
        {
            if (_IsMoving)
            {
                _TimeSinceLastFired = TimeSpan.Zero;

                Position += _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

                if (Position.X <= _StopPosition.X)
                {
                    IsAlive = true;
                    _IsMoving = false;
                }
            }

            if (IsAlive)
            {
                if (_IsSoundPlaying) // TODO FIXME Work around to limit sounds created
                {
                    _TimeSinceLastSound += timeTilUpdate;
                    if (_TimeSinceLastSound >= _HitSoundInterval)
                    {
                        _IsSoundPlaying = false;
                        _TimeSinceLastSound = TimeSpan.Zero;
                    }
                }

                if (!_IsWeaponLocked)
                {
                    Fire();
                }
                else
                {
                    _TimeSinceLastFired += timeTilUpdate;
                    if (_TimeSinceLastFired >= FireRate)
                    {
                        _IsWeaponLocked = false;
                        _TimeSinceLastFired = TimeSpan.Zero;
                    }
                }
            }

            base.Update(timeTilUpdate);
        }

        /// <inheritdoc />
        public override void Destroy()
        {

            GameStats.Instance.EnemiesKilled++;

            IsAlive = false;

            if (_IsDestroyed)
            {
                _Explosion.AddFrameAction((anim) =>
                {
                    Dispose();
                    foreach (Action action in OnDestroyedByPlayer)
                    {
                        action?.Invoke();
                    }
                }, 8);
                _Sprite.Visible = false;
                _Explosion.Visible = true;
                _Explosion.Playing = true;
            }
            else
            {
                _Explosion.RemoveAllFrameActions();
                OnOutOfBounds?.Invoke();
                Dispose();
            }

            PositionRelayer.Instance.RemoveRecipient(this);
        }
    }
}
