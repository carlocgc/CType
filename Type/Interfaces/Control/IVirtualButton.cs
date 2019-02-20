using Type.Data;

namespace Type.Interfaces.Control
{
    /// <summary> Interface for a virtual button </summary>
    public interface IVirtualButton
    {
        /// <summary> The current press state of the button </summary>
        VirtualButtonData.State State { get; set; }
    }
}
