using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Data;
using Type.Interfaces;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using Type.Interfaces.Probe;
using Type.Objects.Projectiles;
using static Type.Constants.Global;

namespace Type.Objects.Player
{
    /// <summary>
    /// Alpha type player craft
    /// </summary>
    public class PlayerAlpha : GameObject, IPlayer
    {
        /// <summary> Single point of contact for probes attached to  the player </summary>
        private readonly IProbeController _ProbeController;
        /// <summary> The players shield </summary>
        private readonly IShield _Shield;
        /// <summary> Position on screen where the player is spawned </summary>
        private readonly Vector2 _SpawnPosition = new Vector2(-540, 0);
        /// <summary> How fast the player can move in any direction </summary>
        private readonly Single _MovementSpeed;
        /// <summary> Amount of time between firing </summary>
        private readonly TimeSpan _FireRate;
        /// <summary> List of the engine effect sprites </summary>
        private readonly Sprite[] _EngineEffects;

        /// <summary> Time since the last bullet was fired </summary>
        private TimeSpan _TimeSinceLastFired;
        /// <summary> The direction to be applied to the position of the player </summary>
        private Vector2 _Direction;
        /// <summary> Movement speed modifier </summary>
        private Single _MoveStrength;
        /// <summary> Whether firing is allowed </summary>
        private Boolean _IsWeaponLocked;
        /// <summary> Whether the ship is autofiring </summary>
        private Boolean _AutoFire;

        /// <inheritdoc />
        public Int32 HitPoints { get; private set; }
        /// <inheritdoc />
        public Vector4 HitBox { get; set; }
        /// <inheritdoc />
        public Boolean AutoFire
        {
            get => _AutoFire;

            set
            {
                _AutoFire = value;
                _ProbeController.Shoot = _AutoFire;
            }
        }

        /// <inheritdoc />
        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                foreach (Sprite effect in _EngineEffects)
                {
                    effect.Position = value;
                }
            }
        }

        public PlayerAlpha()
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/Player/player-alpha.png"))
            {
                Visible = true,
            };
            _Sprite.Offset = _Sprite.Size / 2;
            AddSprite(_Sprite);

            _EngineEffects = new Sprite[2];
            _EngineEffects[0] = new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/Player/engine_effect_large.png"));
            _EngineEffects[0].Offset = new Vector2(25 + _EngineEffects[0].Width, -23);
            _EngineEffects[1] = new Sprite(Game.MainCanvas, Constants.ZOrders.PLAYER, Texture.GetTexture("Content/Graphics/Player/engine_effect_large.png"));
            _EngineEffects[1].Offset = new Vector2(25 + _EngineEffects[1].Width, 23 + _EngineEffects[1].Height);

            Position = _SpawnPosition;

            _MovementSpeed = 700;
            _FireRate = TimeSpan.FromMilliseconds(100);
            HitPoints = 1;

            _ProbeController = new ProbeController();
            _ProbeController.UpdatePosition(Position);

            _Shield = new Shield();
            _Shield.UpdatePosition(Position);

            HitBox = GetRect();

            Spawn();
        }

        /// <inheritdoc />
        public void Spawn()
        {
            Position = _SpawnPosition;
        }

        /// <inheritdoc />
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

            if (AutoFire && !_IsWeaponLocked)
            {
                Shoot();
            }

            Position += GetPositionModifier(timeTilUpdate);
            HitBox = GetRect();

            _ProbeController.UpdatePosition(Position);
            _Shield.UpdatePosition(Position);
            PositionRelayer.Instance.ProvidePosition(Position);
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

        /// <inheritdoc />
        public void Shoot()
        {
            new Laser(Position + new Vector2(_Sprite.Width / 2, 0), new Vector2(1, 0), 1000, 0);
            _IsWeaponLocked = true;
            GameStats.Instance.BulletsFired++;
            new AudioPlayer("Content/Audio/laser1.wav", false, AudioManager.Category.EFFECT, 0.5f);
        }

        /// <inheritdoc />
        public void Hit(Int32 damage)
        {
            if (_Shield.IsActive)
            {
                _Shield.Decrease(damage);
                return;
            }

            HitPoints -= damage;

            foreach (IPlayerListener listener in _Listeners)
            {
                listener.OnPlayerHit(this);
            }

            if (HitPoints <= 0)
            {
                Destroy();
            }
        }

        /// <inheritdoc />
        public void Destroy()
        {
            HitPoints = 0;
            _ProbeController.RemoveAll();

            GameStats.Instance.Deaths++;

            foreach (IPlayerListener listener in _Listeners)
            {
                listener.OnPlayerDeath(this);
            }
        }

        #region  UI_Listener

        /// <inheritdoc />
        public void UpdateAnalogData(Vector2 direction, Single strength)
        {
            _Direction = direction;
            _MoveStrength = strength;

            foreach (Sprite effect in _EngineEffects) effect.Visible = direction.X > 0;
        }

        /// <inheritdoc />
        public void FireButtonPressed()
        {
            AutoFire = true;
        }

        /// <inheritdoc />
        public void FireButtonReleased()
        {
            AutoFire = false;
        }

        /// <inheritdoc />
        private void AddShield(Int32 points)
        {
            if (_Shield.IsMaxLevel)
            {
                foreach (IPlayerListener listener in _Listeners)
                {
                    listener.OnPointPickup(points);
                }
                new AudioPlayer("Content/Audio/points_instead.wav", false, AudioManager.Category.EFFECT, 1);
                return;
            }

            _Shield.Increase();
        }

        /// <inheritdoc />
        private void AddProbe(Int32 id, Int32 points)
        {
            if (_ProbeController.WeaponsAtMax)
            {
                foreach (IPlayerListener listener in _Listeners)
                {
                    listener.OnPointPickup(points);
                }
                new AudioPlayer("Content/Audio/points_instead.wav", false, AudioManager.Category.EFFECT, 1);
                return;
            }

            _ProbeController.AddProbe(id);
            _ProbeController.Shoot = AutoFire;
        }

        /// <summary>
        /// Adds a life to the player
        /// </summary>
        private void AddLife(Int32 points)
        {
            foreach (IPlayerListener listener in _Listeners)
            {
                listener.OnLifeAdded(this, points);
            }
        }

        /// <summary>
        /// Add points
        /// </summary>
        private void AddPoints(Int32 value)
        {
            foreach (IPlayerListener listener in _Listeners)
            {
                listener.OnPointPickup(value);
            }

            new AudioPlayer("Content/Audio/points_pickup.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <inheritdoc />
        public void ApplyPowerup(IPowerup powerup)
        {
            switch (powerup.ID)
            {
                case 0:
                    {
                        AddLife(powerup.PointValue);
                        return;
                    }
                case 1:
                    {
                        AddShield(powerup.PointValue);
                        break;
                    }
                case 2:
                    {
                        AddProbe(0, powerup.PointValue);
                        break;
                    }
                case 3:
                    {
                        AddPoints(powerup.PointValue);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("Powerup does not exist");
            }
        }

        #endregion

        #region Listener

        /// <summary> List of listeners </summary>
        private readonly List<IPlayerListener> _Listeners = new List<IPlayerListener>();

        /// <inheritdoc />
        public void RegisterListener(IPlayerListener listener)
        {
            _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IPlayerListener listener)
        {
            _Listeners.Add(listener);
        }

        #endregion

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            foreach (Sprite effect in _EngineEffects) effect.Dispose();
            _Shield.Dispose();
            _ProbeController.Dispose();
        }
    }
}
