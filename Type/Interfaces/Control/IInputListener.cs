using System;
using OpenTK;
using Type.Controllers;
using Type.Data;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Interface for objects that listen to the <see cref="InputManager"/>
    /// </summary>
    public interface IInputListener
    {
        /// <summary> Informs the listener of input data </summary>
        /// <param name="data"> Data packet from the <see cref="InputManager"/> </param>
        void UpdateInputData(ButtonEventData data);

        /// <summary>
        /// Informs the listener of directional input data
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> The distance the stick is pushed </param>
        void UpdateDirectionData(Vector2 direction, Single strength);
    }
}
