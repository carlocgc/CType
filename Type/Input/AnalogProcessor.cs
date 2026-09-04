using OpenTK;
using System;

namespace Type.Input
{
    /// <summary>
    /// Converts a raw analog stick reading into a unit direction and a movement strength.
    /// </summary>
    /// <remarks>
    /// The deadzone is radial rather than per axis. The desktop provider previously tested each
    /// axis independently, which let a diagonal push clear the deadzone at a lower deflection
    /// than a cardinal one and made the dead area a square rather than a circle.
    /// </remarks>
    public sealed class AnalogProcessor
    {
        /// <summary> Deflection at or below which the stick is treated as centred </summary>
        public Single InnerDeadzone { get; set; }

        /// <summary> Deflection at or above which the stick is treated as fully pushed </summary>
        public Single OuterDeadzone { get; set; }

        /// <summary>
        /// Exponent applied to the normalised deflection. 1 is linear; higher values give finer
        /// control near the centre at the cost of needing more travel for full speed.
        /// </summary>
        public Single ResponseCurve { get; set; }

        public AnalogProcessor()
        {
            InnerDeadzone = 0.2f;
            OuterDeadzone = 0.95f;
            ResponseCurve = 1f;
        }

        /// <summary>
        /// Processes a raw stick reading into a direction and a strength
        /// </summary>
        /// <param name="raw"> Raw stick reading, each axis nominally -1 to 1 </param>
        /// <param name="direction"> Unit vector in the direction of the push, or zero inside the deadzone </param>
        /// <param name="strength"> How far the stick is pushed, 0 to 1, after the deadzone and curve </param>
        public void Process(Vector2 raw, out Vector2 direction, out Single strength)
        {
            Single deflection = raw.Length;

            if (deflection <= InnerDeadzone)
            {
                direction = Vector2.Zero;
                strength = 0;
                return;
            }

            direction = raw / deflection;

            Single range = OuterDeadzone - InnerDeadzone;
            Single normalised = range <= 0 ? 1 : (deflection - InnerDeadzone) / range;
            if (normalised > 1) normalised = 1;

            strength = (Single)Math.Pow(normalised, ResponseCurve);
        }
    }
}
