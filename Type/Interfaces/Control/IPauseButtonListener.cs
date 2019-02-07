namespace Type.Interfaces.Control
{
    /// <summary> Interface for objects that listen to pause button presses </summary>
    public interface IPauseButtonListener
    {
        /// <summary> Invoked when the pause button is pressed </summary>
        void OnPausedPressed();

        /// <summary> Invoked when the pause button is released </summary>
        void OnPauseReleased();
    }
}
