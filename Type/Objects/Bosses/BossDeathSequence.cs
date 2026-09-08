using OpenTK;
using System;
using Type.Data;

namespace Type.Objects.Bosses
{
    /// <summary>
    /// A boss coming apart: blasts walking across the hull for a few seconds, then one that
    /// finishes it.
    /// </summary>
    /// <remarks>
    /// **Blasts are particles rather than sprites.** They were a pooled nine frame animation, the
    /// same sheet ordinary enemies used, and it read as disconnected from the debris thrown at the
    /// same moment. With that gone this owns no drawables at all: it picks points on the hull and
    /// asks <see cref="Data.Particles.BossDeathBlast"/> for each one, so the whole sequence is timing,
    /// and the look lives with every other effect in <see cref="Data.Particles"/>.
    /// <para>
    /// Driven by game time rather than the wall clock, unlike the shake and the flash it asks
    /// for. That is deliberate: pausing mid-sequence should hold the whole thing still rather
    /// than let it run on behind the menu.
    /// </para>
    /// </remarks>
    public sealed class BossDeathSequence
    {
        /// <summary> Every this many blasts, one also flashes the screen </summary>
        private const Int32 BLASTS_PER_FLASH = 5;

        /// <summary> How long the blasts keep coming before the last one </summary>
        private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(3120);

        /// <summary> How long between one blast and the next </summary>
        private static readonly TimeSpan BlastInterval = TimeSpan.FromMilliseconds(120);

        /// <summary> Source of the scatter across the hull </summary>
        private readonly Random _Random = new Random();

        /// <summary> How far through the sequence we are </summary>
        private TimeSpan _Elapsed;

        /// <summary> How long since the last blast </summary>
        private TimeSpan _SinceBlast;

        /// <summary> How many blasts have gone off, which decides when one also flashes </summary>
        private Int32 _BlastCount;

        /// <summary> Whether the sequence is running </summary>
        public Boolean IsRunning { get; private set; }

        /// <summary> Whether the sequence has finished and the boss should now be gone </summary>
        public Boolean IsComplete { get; private set; }

        /// <summary> Starts the sequence. Does nothing if it is already running </summary>
        public void Start()
        {
            if (IsRunning || IsComplete) return;

            IsRunning = true;
            _Elapsed = TimeSpan.Zero;

            // A whole interval, so the first blast lands on the frame the boss dies rather than
            // an interval after it.
            _SinceBlast = BlastInterval;
            _BlastCount = 0;
        }

        /// <summary>
        /// Advances the sequence, scattering blasts across the area given
        /// </summary>
        /// <param name="timeTilUpdate"> Time since the last update </param>
        /// <param name="centre"> The middle of the boss </param>
        /// <param name="size"> How big the boss is, in world units </param>
        public void Update(TimeSpan timeTilUpdate, Vector2 centre, Vector2 size)
        {
            if (!IsRunning) return;

            _Elapsed += timeTilUpdate;
            _SinceBlast += timeTilUpdate;

            if (_SinceBlast >= BlastInterval && _Elapsed < Duration)
            {
                _SinceBlast = TimeSpan.Zero;
                Blast(centre, size);
            }

            if (_Elapsed < Duration) return;

            IsRunning = false;
            IsComplete = true;
            Flash.BossDeathFinal();
        }

        /// <summary>
        /// Puts one blast somewhere on the hull, with the noise and the knock that go with it
        /// </summary>
        /// <param name="centre"> The middle of the boss </param>
        /// <param name="size"> How big the boss is, in world units </param>
        private void Blast(Vector2 centre, Vector2 size)
        {
            // Kept inside the middle of the hull rather than its full width, so a blast reads as
            // being on the boss instead of alongside it.
            Vector2 at = new Vector2(centre.X + Spread(size.X * 0.35f), centre.Y + Spread(size.Y * 0.35f));

            Data.Particles.BossDeathBlast(at);

            _BlastCount++;

            Sounds.Destroyed();
            Shake.BossDeathBlast();

            if (_BlastCount % BLASTS_PER_FLASH == 0) Flash.BossDeathBlast();
        }

        /// <summary> A random displacement either side of centre, up to the amount given </summary>
        private Single Spread(Single amount)
        {
            return ((Single)_Random.NextDouble() * 2f - 1f) * amount;
        }
    }
}
