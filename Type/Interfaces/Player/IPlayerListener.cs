using System;
using OpenTK;

namespace Type.Interfaces.Player
{
    /// <summary>
    /// Object that listens to the player
    /// </summary>
    public interface IPlayerListener
    {
        /// <summary>
        /// Invoked when a life is added
        /// </summary>
        /// <param name="player"></param>
        /// <param name="points"></param>
        void OnLifeAdded(IPlayer player, Int32 points);

        /// <summary>
        /// Invoked when points are picked up
        /// </summary>
        void OnPointPickup(Int32 value);

        /// <summary>
        /// Invoked when the player is hit
        /// </summary>
        void OnPlayerHit(IPlayer player);

        /// <summary>
        /// Invoked when the player has died
        /// </summary>
        void OnPlayerDeath(IPlayer player, Int32 probeCount, Vector2 position);
    }
}
