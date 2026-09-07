using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;
using System;

namespace Type.Controllers
{
    /// <summary>
    /// Whites out the screen for a moment, and fades it back.
    /// </summary>
    /// <remarks>
    /// The sprite sits on the UI canvas rather than the world one, which matters because
    /// <see cref="ScreenShakeController"/> moves the world camera: a full screen overlay drawn
    /// through a camera that is being shaken slides off its own edges and lets the field show
    /// through the gap. Flash and shake are almost always asked for together, so that would be
    /// the normal case rather than an edge one. It draws under the HUD so the score and lives
    /// stay readable through it.
    /// <para>
    /// **Timed against the wall clock**, the same as <see cref="ScreenShakeController"/> and
    /// <see cref="TimeScaleController"/>, so a flash is the length it says it is even while a
    /// hit stop has the game clock slowed.
    /// </para>
    /// <para>
    /// Effects ask for a flash through <see cref="Data.Flash"/> rather than calling this
    /// directly, so the numbers that decide how each event reads sit together in one place.
    /// </para>
    /// </remarks>
    public sealed class ScreenFlashController : IUpdatable
    {
        /// <summary>
        /// Longest step the fade will advance by, in seconds. Same reasoning as
        /// <see cref="ScreenShakeController"/>: a stalled frame must not consume a whole flash.
        /// </summary>
        private const Single MaxStep = 0.1f;

        /// <summary> The instance of the ScreenFlashController </summary>
        private static ScreenFlashController _Instance;

        /// <summary> The instance of the ScreenFlashController </summary>
        public static ScreenFlashController Instance => _Instance ?? (_Instance = new ScreenFlashController());

        /// <summary> The full screen white sprite, null until the level starts </summary>
        private Sprite _Flash;

        /// <summary> How opaque the flash is at its brightest, from 0 to 1 </summary>
        private Single _Peak;

        /// <summary> How much of the flash is left, from 1 down to 0 </summary>
        private Single _Remaining;

        /// <summary> How much of the flash is shed per second </summary>
        private Single _Decay;

        /// <summary> Wall clock time of the previous update </summary>
        private DateTime _Last;

        /// <summary> Whether play is paused, which holds the flash where it is </summary>
        public Boolean Paused { get; set; }

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        private ScreenFlashController()
        {
        }

        /// <summary>
        /// Builds the overlay and starts advancing. Call once when a level starts.
        /// </summary>
        public void Initialise()
        {
            if (IsDisposed) return;

            _Flash = new Sprite(Game.UiCanvas, Constants.ZOrders.SCREEN_FLASH,
                Texture.GetTexture("Content/Graphics/Engine/engine_background.png"))
            {
                Position = new Vector2(0, 0),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0),
            };
            _Flash.Offset = _Flash.Size / 2;

            _Remaining = 0;
            _Last = DateTime.UtcNow;

            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Whites the screen out and fades it back
        /// </summary>
        /// <param name="intensity"> How opaque it goes at its brightest, from 0 to 1 </param>
        /// <param name="duration"> How long it takes to fade, in wall clock time </param>
        /// <remarks>
        /// The brighter request wins outright rather than adding to one already running, for the
        /// same reason <see cref="ScreenShakeController.Shake"/> does it: a chain of these is
        /// ordinary, and summing them would leave the screen solid white.
        /// </remarks>
        public void Flash(Single intensity, TimeSpan duration)
        {
            if (_Flash == null) return;
            if (intensity <= 0 || duration <= TimeSpan.Zero) return;
            if (intensity <= _Peak * _Remaining) return;

            _Peak = intensity > 1 ? 1 : intensity;
            _Decay = 1f / (Single)duration.TotalSeconds;
            _Remaining = 1;
        }

        #region Implementation of IUpdatable

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            DateTime now = DateTime.UtcNow;
            Single seconds = (Single)(now - _Last).TotalSeconds;
            _Last = now;

            if (_Flash == null || _Remaining <= 0) return;
            if (Paused) return;

            if (seconds > MaxStep) seconds = MaxStep;

            _Remaining -= _Decay * seconds;

            if (_Remaining <= 0)
            {
                _Remaining = 0;
                _Flash.Visible = false;
                return;
            }

            _Flash.Visible = true;
            _Flash.Colour = new Vector4(1, 1, 1, _Peak * _Remaining);
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Stops advancing and takes the overlay down
        /// </summary>
        /// <remarks>
        /// The canvas outlives the level, so an overlay left behind would sit over the menu.
        /// </remarks>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            UpdateManager.Instance.RemoveUpdatable(this);

            if (_Flash != null && !_Flash.IsDisposed) _Flash.Dispose();
            _Flash = null;
            _Remaining = 0;

            _Instance = null;
        }

        #endregion
    }
}
