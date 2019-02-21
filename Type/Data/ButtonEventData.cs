using System;
using Type.Controllers;
using Type.Interfaces.Control;
using Type.Services;

namespace Type.Data
{
    /// <summary>
    /// Data sent from the <see cref="InputService"/> to a <see cref="IInputListener"/> containing data about a buttons state
    /// </summary>
    public struct ButtonEventData
    {
        /// <summary> The <see cref="ButtonData.Type"/> of the button this data belongs to</summary>
        public ButtonData.Type ID { get; set; }

        /// <summary> The <see cref="ButtonData.State"/> of the button </summary>
        public ButtonData.State State { get; set; }

        public ButtonEventData(ButtonData.Type id, ButtonData.State state)
        {
            ID = id;
            State = state;
        }
    }
}
