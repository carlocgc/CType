using Type.Buttons;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Interface for a platform specific input provider
    /// </summary>
    public interface IInputProvider
    {
        /// <summary> Virtual analog stick </summary>
        VirtualAnalogStick VirtualAnalogStick { get; set; }

        /// <summary>
        /// Add a listener
        /// </summary>
        void RegisterListener(IInputListener listener);

        /// <summary>
        /// Remove a listener
        /// </summary>
        void DeregisterListener(IInputListener listener);

        /// <summary>
        /// Registers a <see cref="IVirtualButton"/> with the Input provider
        /// </summary>
        /// <param name="button"></param>
        void RegisterButton(IVirtualButton button);

        /// <summary>
        /// Deregisters a <see cref="IVirtualButton"/> from the Input provider
        /// </summary>
        /// <param name="button"></param>
        void DeregisterButton(IVirtualButton button);
    }
}
