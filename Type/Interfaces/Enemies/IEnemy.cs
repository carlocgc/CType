using System;
using AmosShared.Interfaces;
using Java.Lang;
using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.GameData;
using Type.Interfaces.Weapons;
using Boolean = System.Boolean;

namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// Interface for enemies
    /// </summary>
    public interface IEnemy : ICollidable, IPositionRecipient, IProjectileShooter, IPoints, INotifier<IEnemyListener>, IUpdatable
    {
        /// <summary> Whether the enemy can be roadkilled </summary>
        Boolean CanBeRoadKilled { get; }
    }
}
