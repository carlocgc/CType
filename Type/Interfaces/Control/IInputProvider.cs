using System;
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
        /// Vibrates a controller
        /// </summary>
        /// <param name="index"> Index of the controller to vibrate </param>
        /// <param name="strong"> Whether to use strong vibration </param>
        /// <param name="duration"> How long the vbration should last </param>
        void Vibrate(Int32 index, Boolean strong, TimeSpan duration);

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
