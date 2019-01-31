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
        /// <summary> Movement speed </summary>
        private readonly Single _Speed;
        /// <summary> Movement direction </summary>
        private Vector2 _Direction;
        /// <summary> Angle used to oscillate the y axis when moving the enemy </summary>
        private Single _Yoscillation;
        /// <summary> How much to increment the oscilation every update </summary>
        private Single _Yincrement = 0.05f;

        public WaveMotion(Vector2 direction, Single speed)
        {
            _Direction = direction;
            _Speed = speed;
        }

        /// <summary>
        /// Updates a given vector using a modifier
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        /// <returns> The modifed vector </returns>
        public Vector2 GetAcceleration(TimeSpan timeTilUpdate)
        {
            Single x = _Direction.X * _Speed * (Single) timeTilUpdate.TotalSeconds;
            Single y = (Single) Math.Sin(_Yoscillation) * _Speed * (Single) timeTilUpdate.TotalSeconds;

            _Yoscillation += _Yincrement;
            if (_Yoscillation > 360f) _Yoscillation = 0;

            return new Vector2(x, y);
        }
    }
}
