namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// Object that implements this interface will be notified of enemy events
    /// </summary>
    public interface IEnemyListener
    {
        /// <summary>
        /// Invoked when an enemy is Destroyed
        /// </summary>
        /// <param name="enemy"> The enemy that has been destroyed </param>
        void OnEnemyDestroyed(IEnemy enemy);

        /// <summary>
        /// Invoked when an enemy leaves the screen
        /// </summary>
        /// <param name="enemy"> The enemy that has left the screen </param>
        void OnEnemyOffscreen(IEnemy enemy);
    }
}
