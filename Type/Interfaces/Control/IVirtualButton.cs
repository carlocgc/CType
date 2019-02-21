using System;
using Type.Data;

namespace Type.Interfaces.Control
{
    /// <summary> Interface for a virtual button </summary>
    public interface IVirtualButton
    {
        /// <summary> Unique id of the button </summary>
        ButtonData.Type ID { get; }

        /// <summary> The current press state of the button </summary>
        ButtonData.State State { get; set; }
    }
}
