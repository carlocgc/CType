using AmosShared.Base;
using AmosShared.Interfaces;
using System;

namespace Type.Controllers
{
    /// <summary>
    /// The single owner of the game clock's multiplier, so that pausing and hit stop cannot
    /// fight over it.
    /// </summary>
    /// <remarks>
    /// Both effects slow the clock and both would otherwise write
    /// <c>Game.GameTime.Multiplier</c> directly, which stops working the moment there is more
    /// than one of them: a hit stop that ended while the game was paused would have unpaused
    /// the game. Here the multiplier is **derived** from every reason to change it rather than
    /// assigned by whichever ran last, and pause always wins.
    /// <para>
    /// **Timed against the wall clock**, for the same reason the rumble is. The update this
    /// receives has already been scaled by the very multiplier it is setting, so a hit stop at
    /// a fifth speed would take five times as long to end itself, and one that started just
    /// before a pause would never end at all.
    /// </para>
    /// <para>
    /// Effects ask for hit stop through <see cref="Data.HitStop"/> rather than calling this
    /// directly, so the numbers that decide how each event feels sit together in one place.
    /// </para>
    /// </remarks>
    public sealed class TimeScaleController : IUpdatable
    {
        /// <summary> The instance of the TimeScaleController </summary>
        private static TimeScaleController _Instance;

        /// <summary> The instance of the TimeScaleController </summary>
        public static TimeScaleController Instance => _Instance ?? (_Instance = new TimeScaleController());

        /// <summary> How far the clock is slowed by the hit stop in progress, 1 when there is none </summary>
        private Single _HitStopScale = 1;

        /// <summary> Wall clock time the hit stop in progress should end at </summary>
        private DateTime _HitStopUntil;

        /// <summary> Whether play is paused </summary>
        private Boolean _Paused;

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        /// <summary>
        /// Whether play is paused, which stops the clock outright
        /// </summary>
        public Boolean Paused
        {
            get => _Paused;
            set
            {
                _Paused = value;
                Apply();
            }
        }

        private TimeScaleController()
        {
        }

        /// <summary>
        /// Starts advancing the controller. Call once when a level starts.
        /// </summary>
        public void Initialise()
        {
            if (IsDisposed) return;

            Reset();
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Slows the clock briefly, to give an impact weight
        /// </summary>
        /// <param name="scale"> What to multiply the clock by, between 0 and 1 </param>
        /// <param name="duration"> How long to hold it there, in wall clock time </param>
        /// <remarks>
        /// The heavier request wins outright rather than compounding with one already running.
        /// Two of these in the same frame is ordinary — a nuke that kills a boss — and
        /// multiplying them together would stop the game dead.
        /// </remarks>
        public void HitStop(Single scale, TimeSpan duration)
        {
            if (scale >= 1 || duration <= TimeSpan.Zero) return;

            DateTime until = DateTime.UtcNow + duration;

            if (scale < _HitStopScale) _HitStopScale = scale;
            if (until > _HitStopUntil) _HitStopUntil = until;

            Apply();
        }

        /// <summary>
        /// Returns the clock to normal speed and forgets both reasons it might not be
        /// </summary>
        /// <remarks>
        /// For leaving a run rather than for ending a hit stop. Whatever comes next must not
        /// inherit a slowed or stopped clock, which is what made abandoning from the pause menu
        /// freeze the menu it returned to.
        /// </remarks>
        public void Reset()
        {
            _Paused = false;
            _HitStopScale = 1;
            _HitStopUntil = DateTime.MinValue;

            Apply();
        }

        /// <summary>
        /// Works out what the clock should be running at and sets it
        /// </summary>
        private void Apply()
        {
            Game.GameTime.Multiplier = _Paused ? 0 : _HitStopScale;
        }

        #region Implementation of IUpdatable

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            if (_HitStopScale >= 1) return;
            if (DateTime.UtcNow < _HitStopUntil) return;

            _HitStopScale = 1;
            Apply();
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Stops advancing the controller, leaving the clock at normal speed
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            UpdateManager.Instance.RemoveUpdatable(this);
            Reset();

            _Instance = null;
        }

        #endregion
    }
}
