using System;
using Type.Buttons;
using Type.Input;

namespace Type.Interfaces.Control
{
    /// <summary>
    /// Interface for a platform specific input provider
    /// </summary>
    public interface IInputProvider
    {
        /// <summary> Whether the input provider is paused</summary>
        Boolean Paused { get; set; }

        /// <summary> Virtual analog stick </summary>
        VirtualAnalogStick VirtualAnalogStick { get; set; }

        /// <summary>
        /// The active action to input mapping, used to label on screen prompts. Null on
        /// platforms that do not map physical inputs to actions.
        /// </summary>
        InputBindings Bindings { get; }

        /// <summary>
        /// Whether the provider is waiting for the player to press an input to bind. While it
        /// is, nothing is reported to listeners, so the press that chooses a binding cannot also
        /// act on the menu that asked for it.
        /// </summary>
        Boolean Capturing { get; }

        /// <summary>
        /// Waits for the player to press one input and reports it, for the rebinding screen
        /// </summary>
        /// <param name="onCaptured">
        /// Invoked once with the input pressed, or with null if the player backed out. Capture
        /// ends either way.
        /// </param>
        /// <remarks>
        /// Nothing is captured until every input is released, so the press that opened the
        /// capture is never the one taken. Platforms with no rebindable inputs report null
        /// immediately.
        /// </remarks>
        void BeginCapture(Action<InputSource> onCaptured);

        /// <summary>
        /// Abandons a capture in progress without reporting an input. Does nothing if none is
        /// running.
        /// </summary>
        void CancelCapture();

        /// <summary>
        /// Re-reads <see cref="Bindings"/> after it has been changed, so that any per platform
        /// form of it the provider holds is rebuilt
        /// </summary>
        void ReloadBindings();

        /// <summary>
        /// Whether a gamepad is currently driving input, so the interface can show the right
        /// prompts. False on platforms with no gamepad support.
        /// </summary>
        Boolean GamepadActive { get; }

        /// <summary>
        /// Invoked when the input device currently driving the game becomes unavailable, so that
        /// play can be suspended rather than continue with no way to control it. Platforms with
        /// no detachable input device never invoke it.
        /// </summary>
        Action OnInputDeviceLost { get; set; }

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
