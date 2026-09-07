using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Data;

namespace Type.Objects.Bosses
{
    /// <summary>
    /// A boss coming apart: blasts walking across the hull for a couple of seconds, then one
    /// that finishes it.
    /// </summary>
    /// <remarks>
    /// **Many small explosions rather than one big one, because there is no big one to draw.**
    /// The only explosion art in the game is a nine frame pixel sheet sized for a fighter.
    /// Scaling it to the width of a boss magnifies every pixel with it and reads as a smear, so
    /// the size of the event comes from the number of blasts, the noise and the shaking instead
    /// of from the size of any one sprite.
    /// <para>
    /// **Pooled, for the reason G2 recorded.** <see cref="Canvas"/> rebuilds its entire vertex
    /// buffer whenever its drawable list changes, and this puts a blast on screen every sixth of
    /// a second. A pooled sprite registers once and afterwards only moves, recolours and hides.
    /// </para>
    /// <para>
    /// Driven by game time rather than the wall clock, unlike the shake and the flash it asks
    /// for. That is deliberate: this drives sprite animation, which the engine advances on game
    /// time, and pausing mid-sequence should hold the whole thing still rather than let it run
    /// on behind the menu.
    /// </para>
    /// </remarks>
    public sealed class BossDeathSequence : IDisposable
    {
        /// <summary> How many blast sprites are kept. Enough for the overlap that the interval
        /// and the blast length imply, with room to spare </summary>
        private const Int32 POOL_SIZE = 10;

        /// <summary> How fast a single blast plays, in frames per second. Faster than the
        /// enemy explosion, so a blast reads as a crack rather than a bloom </summary>
        private const Single BLAST_FPS = 18f;

        /// <summary> Every this many blasts, one also flashes the screen </summary>
        private const Int32 BLASTS_PER_FLASH = 4;

        /// <summary> How long the blasts keep coming before the last one </summary>
        private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(2400);

        /// <summary> How long between one blast and the next </summary>
        private static readonly TimeSpan BlastInterval = TimeSpan.FromMilliseconds(160);

        /// <summary> The blast sprites, reused in turn </summary>
        private readonly AnimatedSprite[] _Blasts;

        /// <summary> Source of the scatter across the hull </summary>
        private readonly Random _Random = new Random();

        /// <summary> Which blast sprite is used next </summary>
        private Int32 _Next;

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

        /// <summary> Whether this has been disposed </summary>
        public Boolean IsDisposed { get; private set; }

        /// <summary> Builds the blast pool, hidden until the boss dies </summary>
        public BossDeathSequence()
        {
            _Blasts = new AnimatedSprite[POOL_SIZE];

            for (Int32 i = 0; i < POOL_SIZE; i++)
            {
                AnimatedSprite blast = new AnimatedSprite(Game.MainCanvas, Constants.ZOrders.BOSS_DEATH_BLAST, new[]
                {
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion00.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion01.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion02.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion03.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion04.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion05.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion06.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion07.png"),
                    Texture.GetTexture("Content/Graphics/Explosion2/pixelExplosion08.png"),
                }, BLAST_FPS)
                {
                    Visible = false,
                    Playing = false,
                    AnimEndBehaviour = AnimatedSprite.EndBehaviour.STOP,
                    CurrentFrame = 0,
                };

                // Added once and left in place: a repeat count of zero never runs out, so this
                // runs at the end of every replay rather than only the first. It stops as well as
                // hides: EndBehaviour.STOP would now do that too, since AmosEngine !31, but a
                // pooled sprite that is replayed rather than disposed is clearer left in a state
                // it put itself in. See G6 in ROADMAP.md.
                blast.AddFrameAction(anim =>
                {
                    anim.Playing = false;
                    anim.Visible = false;
                }, 8);

                _Blasts[i] = blast;
            }
        }

        /// <summary> Starts the sequence. Does nothing if it is already running </summary>
        public void Start()
        {
            if (IsRunning || IsComplete || IsDisposed) return;

            IsRunning = true;
            _Elapsed = TimeSpan.Zero;

            // Zero rather than the interval, so the first blast lands on the frame the boss
            // dies rather than a sixth of a second after it.
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
            if (!IsRunning || IsDisposed) return;

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
            AnimatedSprite blast = _Blasts[_Next];
            _Next = (_Next + 1) % POOL_SIZE;

            // Kept inside the middle of the hull rather than its full width, so a blast reads as
            // being on the boss instead of alongside it.
            Single x = centre.X + Spread(size.X * 0.35f);
            Single y = centre.Y + Spread(size.Y * 0.35f);

            Single scale = 1.1f + (Single)_Random.NextDouble() * 0.9f;

            blast.Scale = new Vector2(scale, scale);
            blast.Offset = new Vector2(blast.Size.X / 2 * scale, blast.Size.Y / 2 * scale);
            blast.Position = new Vector2(x, y);
            blast.CurrentFrame = 0;
            blast.Visible = true;
            blast.Playing = true;

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

        /// <summary> Releases the blast sprites </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            IsRunning = false;

            foreach (AnimatedSprite blast in _Blasts)
            {
                if (blast != null && !blast.IsDisposed) blast.Dispose();
            }
        }
    }
}
