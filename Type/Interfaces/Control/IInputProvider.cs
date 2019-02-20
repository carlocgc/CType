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
        /// <summary> Virtual nuke button </summary>
        NukeButton NukeButton { get; set; }
        /// <summary> Virtual firebutton </summary>
        FireButton FireButton { get; set; }
        /// <summary> Virtual pausebutton </summary>
        PauseButton PauseButton { get; set; }
        /// <summary> Virtual resumebutton </summary>
        ResumeButton ResumeButton { get; set; }

        /// <summary>
        /// Add a listener
        /// </summary>
        void RegisterListener(IInputListener listener);

        /// <summary>
        /// Remove a listener
        /// </summary>
        void DeregisterListener(IInputListener listener);
    }
}
