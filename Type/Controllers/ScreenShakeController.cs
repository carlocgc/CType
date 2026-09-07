using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Interfaces;
using OpenTK;
using System;

namespace Type.Controllers
{
    /// <summary>
    /// Shakes the world camera, and puts it back.
    /// </summary>
    /// <remarks>
    /// Only the main canvas is moved, so the HUD stays exactly where it is. That is the whole
    /// reason this offsets a camera rather than the objects in front of it: the game already
    /// draws the world and the interface through two cameras, and shaking one of them is free.
    /// <para>
    /// **Timed against the wall clock**, the same as <see cref="TimeScaleController"/> and for a
    /// related reason: shake and hit stop are usually asked for together, and a shake measured
    /// in game time would be stretched by the hit stop into something several times longer than
    /// it was asked for.
    /// </para>
    /// <para>
    /// Effects ask for shake through <see cref="Data.Shake"/> rather than calling this directly,
    /// so the numbers that decide how each event feels sit together in one place.
    /// </para>
    /// </remarks>
    public sealed class ScreenShakeController : IUpdatable
    {
        /// <summary>
        /// Longest step the shake will advance by, in seconds
        /// </summary>
        /// <remarks>
        /// A window that has been dragged, alt tabbed away from or stalled by a level load hands
        /// back a very large wall clock step. Without this the first frame afterwards consumes
        /// the whole shake, or worse, overshoots the decay and leaves the camera off centre.
        /// </remarks>
        private const Single MaxStep = 0.1f;

        /// <summary> The instance of the ScreenShakeController </summary>
        private static ScreenShakeController _Instance;

        /// <summary> The instance of the ScreenShakeController </summary>
        public static ScreenShakeController Instance => _Instance ?? (_Instance = new ScreenShakeController());

        /// <summary> Source of the per frame jitter </summary>
        private readonly Random _Random = new Random();

        /// <summary> The camera being shaken, null until the level starts </summary>
        private Camera _Camera;

        /// <summary> Where the camera sits when nothing is shaking it </summary>
        private Vector2 _Rest;

        /// <summary> How far the camera moves at full trauma, in world units </summary>
        private Single _Magnitude;

        /// <summary> How much trauma is shed per second </summary>
        private Single _Decay;

        /// <summary> How much shake is left, from 1 down to 0 </summary>
        private Single _Trauma;

        /// <summary> Wall clock time of the previous update </summary>
        private DateTime _Last;

        /// <summary> Whether play is paused, which holds the camera where it is </summary>
        public Boolean Paused { get; set; }

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        private ScreenShakeController()
        {
        }

        /// <summary>
        /// Takes hold of the world camera and starts advancing. Call once when a level starts.
        /// </summary>
        /// <remarks>
        /// The camera's current position is remembered rather than assumed to be the origin, so
        /// that putting it back is correct even if something else ever moves it.
        /// </remarks>
        public void Initialise()
        {
            if (IsDisposed) return;

            _Camera = Game.MainCanvas.Camera;
            _Rest = _Camera.Position;
            _Trauma = 0;
            _Last = DateTime.UtcNow;

            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Knocks the camera about for a moment
        /// </summary>
        /// <param name="magnitude"> How far it moves at the start, in world units </param>
        /// <param name="duration"> How long it takes to settle, in wall clock time </param>
        /// <remarks>
        /// The heavier request wins outright rather than adding to one already running. Two of
        /// these in the same frame is ordinary — a nuke that kills a boss — and summing them
        /// would throw the camera off the far side of the field.
        /// </remarks>
        public void Shake(Single magnitude, TimeSpan duration)
        {
            if (_Camera == null) return;
            if (magnitude <= 0 || duration <= TimeSpan.Zero) return;

            // Compared against what is actually left of the running shake, not against the
            // magnitude it started at, so a heavy shake that has nearly finished does not lock
            // out a lighter one that has just been asked for.
            if (magnitude <= _Magnitude * _Trauma) return;

            _Magnitude = magnitude;
            _Decay = 1f / (Single)duration.TotalSeconds;
            _Trauma = 1;
        }

        #region Implementation of IUpdatable

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            DateTime now = DateTime.UtcNow;
            Single seconds = (Single)(now - _Last).TotalSeconds;
            _Last = now;

            if (_Camera == null || _Trauma <= 0) return;

            // Held rather than cancelled: a shake interrupted by the pause menu carries on when
            // play does, which is less jarring than the camera snapping straight.
            if (Paused) return;

            if (seconds > MaxStep) seconds = MaxStep;

            _Trauma -= _Decay * seconds;

            if (_Trauma <= 0)
            {
                _Trauma = 0;
                _Camera.Position = _Rest;
                return;
            }

            // Squared so the shake falls away sharply and then settles, rather than grinding
            // linearly down to nothing. A linear decay reads as a rattle that will not stop.
            Single amount = _Magnitude * _Trauma * _Trauma;

            _Camera.Position = _Rest + new Vector2(Offset(amount), Offset(amount));
        }

        /// <summary>
        /// A random displacement in either direction, up to the amount given
        /// </summary>
        private Single Offset(Single amount)
        {
            return ((Single)_Random.NextDouble() * 2f - 1f) * amount;
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Stops advancing and returns the camera to where it was found
        /// </summary>
        /// <remarks>
        /// The camera outlives the level — it belongs to a canvas built once when the game
        /// starts — so a shake left running would follow the player into the menu.
        /// </remarks>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            UpdateManager.Instance.RemoveUpdatable(this);

            if (_Camera != null) _Camera.Position = _Rest;
            _Camera = null;
            _Trauma = 0;

            _Instance = null;
        }

        #endregion
    }
}
