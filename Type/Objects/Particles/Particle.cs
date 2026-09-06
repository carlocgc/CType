using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;

namespace Type.Objects.Particles
{
    /// <summary>
    /// One particle: a sprite that is spawned, drifts, fades and goes back in the pool.
    /// </summary>
    /// <remarks>
    /// **Pooled, never created per effect.** The canvas rebuilds its whole vertex buffer
    /// whenever its drawable list changes, so creating and disposing sprites at the rate an
    /// explosion wants them would rebuild it several times a frame. A particle registers its
    /// sprite once and afterwards only ever changes position, colour and visibility, none of
    /// which touches the list.
    /// </remarks>
    public sealed class Particle
    {
        /// <summary> The sprite drawn for this particle </summary>
        private readonly Sprite _Sprite;

        /// <summary> Current velocity, in world units per second </summary>
        private Vector2 _Velocity;

        /// <summary> Colour at full life; the alpha is scaled down as the particle fades </summary>
        private Vector4 _Colour;

        /// <summary> Scale at full life, shrunk towards nothing as the particle fades </summary>
        private Single _Scale;

        /// <summary> How much of a second's velocity is shed each second </summary>
        private Single _Drag;

        /// <summary> Seconds of life remaining </summary>
        private Single _Life;

        /// <summary> Seconds of life the particle was spawned with </summary>
        private Single _TotalLife;

        /// <summary> Whether the particle is in use rather than waiting in the pool </summary>
        public Boolean Alive { get; private set; }

        /// <summary>
        /// Creates a particle and registers its sprite. Done once per pool slot, at level start.
        /// </summary>
        public Particle()
        {
            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.PARTICLES,
                Texture.GetTexture("Content/Graphics/Particles/particle.png"))
            {
                Visible = false,
            };

            // Drawn from the centre, so a particle spins out from the point it was spawned at
            // rather than from its top-left corner.
            _Sprite.Offset = new Vector2(_Sprite.Width, _Sprite.Height) / 2;
        }

        /// <summary>
        /// Puts the particle into play
        /// </summary>
        /// <param name="position"> Where it starts </param>
        /// <param name="velocity"> How fast and which way, in world units per second </param>
        /// <param name="colour"> Colour at full life </param>
        /// <param name="life"> How long it lasts, in seconds </param>
        /// <param name="scale"> Size at full life </param>
        /// <param name="drag"> How much of its speed it sheds per second, 0 for none </param>
        public void Spawn(Vector2 position, Vector2 velocity, Vector4 colour, Single life, Single scale, Single drag)
        {
            _Velocity = velocity;
            _Colour = colour;
            _Scale = scale;
            _Drag = drag;
            _Life = life;
            _TotalLife = life;

            _Sprite.Position = position;
            _Sprite.Colour = colour;
            _Sprite.Scale = new Vector2(scale, scale);
            _Sprite.Visible = true;

            Alive = true;
        }

        /// <summary>
        /// Advances the particle, and returns it to the pool once its life runs out
        /// </summary>
        /// <param name="seconds"> Time since the last update </param>
        public void Update(Single seconds)
        {
            if (!Alive) return;

            _Life -= seconds;
            if (_Life <= 0)
            {
                Kill();
                return;
            }

            _Velocity -= _Velocity * _Drag * seconds;
            _Sprite.Position += _Velocity * seconds;

            // Fading on both alpha and size, because alpha alone leaves a particle its full
            // size right up to the frame it disappears, which reads as a pop rather than a fade.
            Single remaining = _Life / _TotalLife;

            _Sprite.Colour = new Vector4(_Colour.X, _Colour.Y, _Colour.Z, _Colour.W * remaining);
            _Sprite.Scale = new Vector2(_Scale * remaining, _Scale * remaining);
        }

        /// <summary>
        /// Returns the particle to the pool
        /// </summary>
        public void Kill()
        {
            Alive = false;
            _Sprite.Visible = false;
        }

        /// <summary>
        /// Disposes the sprite. The pool is torn down with the level that built it.
        /// </summary>
        public void Dispose()
        {
            Alive = false;
            _Sprite.Dispose();
        }
    }
}
