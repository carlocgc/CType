namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// An enemy that is a boss rather than one of a wave.
    /// </summary>
    /// <remarks>
    /// A marker with no members of its own. Before it, the only thing saying which enemies were
    /// bosses was the folder they sat in, which nothing at runtime can read, so anything wanting
    /// to treat a boss differently had to name all five classes.
    /// <para>
    /// **It was written as a stopgap and is not one any more.** The note here used to say E1
    /// would collapse everything into one data-driven type and make being a boss a field in the
    /// data, at which point this would go away. E1 did the collapse and reached the opposite
    /// conclusion: a boss is a different object from a wave enemy - a hull that advances to a
    /// stop, carries guns that are hit instead of it, and has no hit points of its own - so it is
    /// <see cref="Objects.Bosses.Boss"/> and not an <see cref="Objects.Enemies.Enemy"/> with a
    /// flag. Two classes implement <see cref="IEnemy"/> now, and this says which is which
    /// honestly rather than provisionally.
    /// </para>
    /// </remarks>
    public interface IBoss : IEnemy
    {
    }
}
