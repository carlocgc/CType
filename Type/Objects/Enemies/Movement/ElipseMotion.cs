using OpenTK;
using System;
using Type.Interfaces.Movement;

namespace Type.Objects.Enemies.Movement
{
    public class ElipseMotion : IAccelerationProvider
    {
        /// <summary> Movement speed </summary>
        private readonly Single _Speed;
        /// <summary> Movement direction </summary>
        private Vector2 _Direction;

        public ElipseMotion(Vector2 direction, Single speed)
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
            if (baseVector.Y >= 0) baseVector.Y -= _Speed * (Single) timeTilUpdate.TotalSeconds;
            if (baseVector.Y < 0) baseVector.Y += _Speed * (Single) timeTilUpdate.TotalSeconds;
            baseVector.X = 1f/200f * (baseVector.Y * 2);

            return baseVector;
        }
    }
}
