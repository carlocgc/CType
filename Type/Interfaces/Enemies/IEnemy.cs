using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.Weapons;

namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// Interface for enemies
    /// </summary>
    public interface IEnemy : ICollidable, IPositionRecipient, IProjectileShooter, INotifier<IEnemyListener>
    {
    }
}
