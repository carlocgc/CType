using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.Weapons;

namespace Type.Interfaces.Player
{
    /// <summary>
    /// Interface for the player ship
    /// </summary>
    public interface IPlayer : ISpawnable, IHitable, IDestroyable, ICollidable, IProjectileShooter, IAnalogListener, INotifier<IPlayerListener>
    {
    }
}
