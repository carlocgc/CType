namespace Type.Input
{
    /// <summary>
    /// Platform agnostic gamepad button identifiers. Each platform input provider maps these
    /// onto its own gamepad API, so bindings can be declared once in shared code.
    /// </summary>
    public enum GamepadButton
    {
        NONE,
        A,
        B,
        X,
        Y,
        LEFT_SHOULDER,
        RIGHT_SHOULDER,
        LEFT_TRIGGER,
        RIGHT_TRIGGER,
        LEFT_STICK,
        RIGHT_STICK,
        START,
        BACK,
        DPAD_UP,
        DPAD_DOWN,
        DPAD_LEFT,
        DPAD_RIGHT,
    }
}
