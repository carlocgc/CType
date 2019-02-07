namespace Type.Interfaces.Control
{
    /// <summary> Interface for objects that listen to the resume button </summary>
    public interface IResumeButtonListener
    {
        /// <summary> Invoked when the pause button is pressed </summary>
        void OnResumePressed();

        /// <summary> Invoked when the pause button is released </summary>
        void OnResumeReleased();
    }
}
