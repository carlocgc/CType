using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Objects.Particles;

namespace Type.Controllers
{
    /// <summary>
    /// Owns the particle pool, spawns bursts into it and advances what is alive.
    /// </summary>
    /// <remarks>
    /// A fixed pool built once when a level starts and torn down with it. Nothing is allocated
    /// while playing, which is the point: the canvas rebuilds its vertex buffer whenever its
    /// drawable list changes, so a system that created sprites on demand would rebuild it
    /// several times a frame during a firefight.
    /// <para>
    /// Effects ask for particles through <see cref="Data.Particles"/> rather than calling this
    /// directly, so the numbers that decide how each event looks sit together in one place.
    /// </para>
    /// </remarks>
    public sealed class ParticleController : IUpdatable
    {
        /// <summary>
        /// How many particles can be alive at once.
        /// </summary>
        /// <remarks>
        /// Sized against the canvas rather than guessed. A sprite is four vertices of eight
        /// floats and six indices; the buffers hold 900,000 and 6,000,000, so this costs well
        /// under a hundredth of either. The real cost is that every pooled sprite is walked
        /// each frame whether it is alive or not, which is why this is a few hundred and not a
        /// few thousand.
        /// </remarks>
        private const Int32 Capacity = 256;

        /// <summary> The instance of the ParticleController </summary>
        private static ParticleController _Instance;

        /// <summary> The instance of the ParticleController </summary>
        public static ParticleController Instance => _Instance ?? (_Instance = new ParticleController());

        /// <summary> Every particle, alive or waiting </summary>
        private readonly List<Particle> _Pool = new List<Particle>(Capacity);

        /// <summary> Source of the spread and speed variation within a burst </summary>
        private readonly Random _Random = new Random();

        /// <summary> Where the search for a free particle left off, so bursts do not all scan from zero </summary>
        private Int32 _Cursor;

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        private ParticleController()
        {
        }

        /// <summary>
        /// Builds the pool and starts advancing it. Call once when a level starts.
        /// </summary>
        /// <remarks>
        /// Deferred rather than done in the constructor because every particle registers a
        /// sprite with the canvas, and the canvas has to exist first.
        /// </remarks>
        public void Initialise()
        {
            if (_Pool.Count > 0) return;

            for (Int32 i = 0; i < Capacity; i++) _Pool.Add(new Particle());

            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Throws a ring of particles out from a point
        /// </summary>
        /// <param name="position"> Where the burst starts </param>
        /// <param name="count"> How many to try for </param>
        /// <param name="minSpeed"> Slowest particle, in world units per second </param>
        /// <param name="maxSpeed"> Fastest particle, in world units per second </param>
        /// <param name="colour"> Colour at full life </param>
        /// <param name="life"> How long each lasts, in seconds </param>
        /// <param name="scale"> Size at full life </param>
        /// <param name="drag"> How much speed each sheds per second </param>
        /// <remarks>
        /// A burst that cannot be filled is simply smaller. Dropping the particles that do not
        /// fit is better than recycling the oldest live ones, which would cut short an
        /// explosion that is still on screen to start one that has only just begun.
        /// </remarks>
        public void Burst(Vector2 position, Int32 count, Single minSpeed, Single maxSpeed,
            Vector4 colour, Single life, Single scale, Single drag)
        {
            if (_Pool.Count == 0) return;

            for (Int32 i = 0; i < count; i++)
            {
                Particle particle = Take();
                if (particle == null) return;

                Double angle = _Random.NextDouble() * Math.PI * 2;
                Single speed = minSpeed + (Single)_Random.NextDouble() * (maxSpeed - minSpeed);

                Vector2 velocity = new Vector2((Single)Math.Cos(angle), (Single)Math.Sin(angle)) * speed;

                // Varied per particle so a burst does not read as one object breaking into
                // identical pieces, which is what a fixed life and size look like.
                Single spread = 0.6f + (Single)_Random.NextDouble() * 0.8f;

                particle.Spawn(position, velocity, colour, life * spread, scale * spread, drag);
            }
        }

        /// <summary>
        /// Finds a particle that is not in use, or null when they are all busy
        /// </summary>
        private Particle Take()
        {
            for (Int32 i = 0; i < _Pool.Count; i++)
            {
                _Cursor++;
                if (_Cursor >= _Pool.Count) _Cursor = 0;

                if (!_Pool[_Cursor].Alive) return _Pool[_Cursor];
            }

            return null;
        }

        #region Implementation of IUpdatable

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            Single seconds = (Single)timeTilUpdate.TotalSeconds;

            for (Int32 i = 0; i < _Pool.Count; i++) _Pool[i].Update(seconds);
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Kills everything alive without tearing the pool down, for a level change
        /// </summary>
        public void Clear()
        {
            for (Int32 i = 0; i < _Pool.Count; i++) _Pool[i].Kill();
        }

        /// <summary>
        /// Disposes every pooled sprite and drops the instance
        /// </summary>
        /// <remarks>
        /// The sprites are registered with a canvas, so leaving them behind is exactly the leak
        /// S9 went looking for. The instance goes with them because the next level builds its
        /// pool against a new canvas.
        /// </remarks>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            UpdateManager.Instance.RemoveUpdatable(this);

            for (Int32 i = 0; i < _Pool.Count; i++) _Pool[i].Dispose();
            _Pool.Clear();

            _Instance = null;
        }
    }
}
