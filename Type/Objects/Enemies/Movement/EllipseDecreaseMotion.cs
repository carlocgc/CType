using OpenTK;
using System;
using Type.Interfaces.Movement;

namespace Type.Objects.Enemies.Movement
{
    /// <summary>
    /// Creates movement in an elipse while decreasing Y axis
    /// </summary>
    public class EllipseDecreaseMotion : IAccelerationProvider
    {
        /// <summary> Movement speed </summary>
        private readonly Single _Speed;
        /// <summary> Movement direction </summary>
        private Vector2 _Direction;

        public EllipseDecreaseMotion(Vector2 direction, Single speed)
        {
            _Direction = direction;
            _Speed = speed;
        }

        /// <summary>
        /// Updates a given vector using a modifier
        /// </summary>
        /// <param name="timeTilUpdate"></param>
        /// <returns> The modifed vector </returns>
        public Vector2 ApplyAcceleration(Vector2 baseVector, TimeSpan timeTilUpdate)
        {
            baseVector.Y -= _Speed * (Single) timeTilUpdate.TotalSeconds;
            baseVector.X = 1f/200f * (baseVector.Y * baseVector.Y);

            return baseVector;
        }
    }
}
