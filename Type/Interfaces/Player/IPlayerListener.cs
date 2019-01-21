using System;
using System.Collections.Generic;
using System.Text;

namespace Type.Interfaces.Player
{
    /// <summary>
    /// Object that listens to the player
    /// </summary>
    public interface IPlayerListener
    {
        /// <summary>
        /// Invoked when the player is hit
        /// </summary>
        void OnPlayerHit(IPlayer player);

        /// <summary>
        /// Invoked when the player has died
        /// </summary>
        void OnPlayerDeath(IPlayer player);
    }
}
