using System;
using System.Collections.Generic;
using System.Text;
using Type.Interfaces.Collisions;
using Type.Interfaces.Control;
using Type.Interfaces.Weapons;

namespace Type.Interfaces.Enemies
{
    /// <summary>
    /// Interface for enemies
    /// </summary>
    public interface IEnemy : IHitable, IDestroyable, ICollidable, IPositionRecipient, IProjectileShooter, INotifier<IEnemyListener>
    {
    }
}
