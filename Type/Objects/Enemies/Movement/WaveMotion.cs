using OpenTK;
using System;
using Type.Interfaces.Movement;

namespace Type.Objects.Enemies.Movement
{
    /// <summary>
    /// Produces wave motion
    /// </summary>
    public class WaveMotion : IAccelerationProvider
    {
        /// <summary> Initial Y position </summary>
        private readonly Single _SpawnY;
        /// <summary> Movement direction </summary>
        private Vector2 _Direction;
        /// <summary> Movement speed </summary>
        private Single _Speed;
        /// <summary> Angle used to oscillate the y axis when moving the enemy </summary>
        private Single _Yoscillation;
        /// <summary> How much to increment the oscilation every update </summary>
        private Single _Yincrement = 0.05f;

        public WaveMotion(Single initialY, Vector2 direction, Single speed)
        {
            _SpawnY = initialY;
            _Direction = direction;
            _Speed = speed;
        }

        /// <summary>
        /// Updates a given vector using a modifier
        /// </summary>
        /// <param name="position"> The vector to be modified </param>
        /// <param name="timeTilUpdate"></param>
        /// <returns> The modifed vector </returns>
        public Vector2 UpdatePosition(TimeSpan timeTilUpdate)
        {
            Single x = _Direction.X * _Speed * (Single) timeTilUpdate.TotalSeconds;
            Single y = _SpawnY + (Single) Math.Sin(_Yoscillation) * _Speed * (Single) timeTilUpdate.TotalSeconds;

            _Yoscillation += _Yincrement;
            if (_Yoscillation > 360f) _Yoscillation = 0;
            return new Vector2(x, y);
        }
    }
}
