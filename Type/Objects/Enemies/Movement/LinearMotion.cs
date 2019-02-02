using OpenTK;
using System;
using Type.Interfaces.Movement;

namespace Type.Objects.Enemies.Movement
{
    /// <summary>
    /// Increases X axis by speed and direction
    /// </summary>
    public class LinearMotion : IAccelerationProvider
    {
        /// <summary> Movement speed </summary>
        private readonly Single _Speed;
        /// <summary> Movement direction </summary>
        private Vector2 _Direction;

        public LinearMotion(Vector2 direction, Single speed)
        {
            _Direction = direction;
            _Speed = speed;
        }

        /// <summary>
        /// Updates a given vector using a modifier
        /// </summary>
        /// <param name="baseVector"></param>
        /// <param name="timeTilUpdate"></param>
        /// <returns> The modifed vector </returns>
        public Vector2 ApplyAcceleration(Vector2 baseVector, TimeSpan timeTilUpdate)
        {
            baseVector += new Vector2(_Direction.X * _Speed * (Single)timeTilUpdate.TotalSeconds, 0);
            return baseVector;
        }
    }
}
