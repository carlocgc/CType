using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.GameData;
using Type.Interfaces.Weapons;

namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// Interface for enemies
    /// </summary>
    public interface IEnemy : ICollidable, IPositionRecipient, IProjectileShooter, IPoints, INotifier<IEnemyListener>
    {
    }
}
