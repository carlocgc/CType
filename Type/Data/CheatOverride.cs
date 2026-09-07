namespace Type.Data
{
    /// <summary>
    /// How a cheat treats something the game normally decides for itself.
    /// </summary>
    /// <remarks>
    /// Three states rather than a switch, because a switch can only ever add. The Omega ship is
    /// unlocked by finishing the campaign, and once that is recorded there is no way back:
    /// <see cref="Progress.SetGameCompleted"/> has no inverse. A tester who has finished the game
    /// once - or whose save was finished for them by an automated run - could otherwise never see
    /// the locked state again.
    /// <para>
    /// <see cref="DEFAULT"/> leaves the game's own answer alone, so the real progression is what
    /// is being tested unless someone deliberately says otherwise. Neither of the other two
    /// writes to <see cref="Progress"/>: forcing a state for a test must not rewrite what the
    /// player has actually earned.
    /// </para>
    /// </remarks>
    public enum CheatOverride
    {
        /// <summary> Whatever the game would decide on its own </summary>
        DEFAULT,

        /// <summary> Forced on, whatever the game would decide </summary>
        FORCED_ON,

        /// <summary> Forced off, whatever the game would decide </summary>
        FORCED_OFF,
    }
}
