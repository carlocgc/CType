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
using Type.Services;
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
        /// <summary> How long the player is invincible when spawned </summary>
        private readonly TimeSpan _InvincibilityDuration = TimeSpan.FromSeconds(5);

        private TimedCallback _InvincibleCallback;
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
        /// <summary> Whether the player is invincible </summary>
        private Boolean _Invincible;
        /// <summary> Whether the sprite is dimmed, used during invincibility to set the correct colour </summary>
        private Boolean _IsDimmed;
        /// <summary> Callback to reset colour while flashing </summary>
        private TimedCallback _InvincibleColourCallback;

        /// <summary> Current amount of probes the player has </summary>
        public Int32 CurrentProbes => _ProbeController.CurrentProbes;
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
                HitBox = GetRect();
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

            _MovementSpeed = 775;
            _FireRate = TimeSpan.FromMilliseconds(85);
            HitPoints = 1;

            _ProbeController = new ProbeController();
            _Shield = new Shield();
            _Shield.UpdatePosition(Position);

            InputService.Instance.RegisterListener(this);
        }

        /// <inheritdoc />
        public void Spawn()
        {
            Position = _SpawnPosition;
            HitPoints = 1;
            StartInvincible();
        }

        /// <summary>
        /// Makes the player invincible
        /// </summary>
        private void StartInvincible()
        {
            _Invincible = true;
            FlashSprite();
            _InvincibleCallback?.Dispose();
            _InvincibleCallback = new TimedCallback(_InvincibilityDuration, () =>
            {
                _Invincible = false;
                _InvincibleColourCallback?.Dispose();
                _Sprite.Colour = new Vector4(1, 1, 1, 1);
                _IsDimmed = false;
            });
        }

        /// <summary>
        /// Flashes the player sprite, used while invincible
        /// </summary>
        private void FlashSprite()
        {
            if (_IsDimmed)
            {
                _Sprite.Colour = new Vector4(1, 1, 1, 1);
                _IsDimmed = false;
            }
            else
            {
                _Sprite.Colour = new Vector4(1, 1, 1, 0.3f);
                _IsDimmed = true;
            }
            _InvincibleColourCallback = new TimedCallback(TimeSpan.FromMilliseconds(100), FlashSprite);
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

            _ProbeController.UpdatePosition(Position, (Single)timeTilUpdate.TotalSeconds);
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

            if (_Invincible || Constants.Global.INVINCIBLE) return;

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

        /// <summary>
        /// Whether the enemy is destroyed
        /// </summary>
        public Boolean IsDestroyed { get; set; }

        /// <inheritdoc />
        public void Destroy()
        {
            IsDestroyed = true;
            Int32 probeCount = _ProbeController.CurrentProbes;

            HitPoints = 0;
            _ProbeController.RemoveAll();

            GameStats.Instance.Deaths++;

            foreach (IPlayerListener listener in _Listeners)
            {
                listener.OnPlayerDeath(this, probeCount, Position);
            }
        }

        #region Implementation of IInputListener

        /// <inheritdoc />
        public void UpdateDirectionData(Vector2 direction, Single strength)
        {
            _Direction = direction;
            _MoveStrength = strength;

            foreach (Sprite effect in _EngineEffects) effect.Visible = direction.X > 0;
        }

        /// <summary> Informs the listener of input events </summary>
        /// <param name="data"> Data packet from the <see cref="InputManager"/> </param>
        public void UpdateInputData(ButtonEventData data)
        {
            switch (data.ID)
            {
                case ButtonData.Type.FIRE:
                    {
                        AutoFire = data.State == ButtonData.State.PRESSED || data.State == ButtonData.State.HELD;
                        break;
                    }
            }
        }

        #endregion

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

        /// <summary>
        /// Adds a nuke to the player, informs the listeners a nuke was added
        /// </summary>
        /// <param name="points"></param>
        private void AddNuke(Int32 points)
        {
            foreach (IPlayerListener listener in _Listeners)
            {
                listener.OnNukeAdded(points);
            }
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
                case 4:
                    {
                        AddNuke(powerup.PointValue);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("Powerup does not exist");
            }
        }

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
            InputService.Instance.DeregisterListener(this);
            _InvincibleCallback?.Dispose();
            _InvincibleColourCallback?.Dispose();
            foreach (Sprite effect in _EngineEffects) effect.Dispose();
            _Shield.Dispose();
            _ProbeController.Dispose();
        }
    }
}
