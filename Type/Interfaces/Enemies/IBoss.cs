namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// An enemy that is a boss rather than one of a wave.
    /// </summary>
    /// <remarks>
    /// A marker with no members of its own. Until now the only thing saying which enemies were
    /// bosses was the folder they sat in, which nothing at runtime can read, so anything wanting
    /// to treat a boss differently had to name all five classes.
    /// <para>
    /// A stopgap, and deliberately a small one. ROADMAP item E1 collapses the enemy classes into
    /// one data-driven type, at which point being a boss becomes a field in the data and this
    /// goes away. Adding a property to <see cref="IEnemy"/> instead would have meant answering
    /// it in all eleven enemy classes rather than the five it is true of.
    /// </para>
    /// </remarks>
    public interface IBoss : IEnemy
    {
    }
}
