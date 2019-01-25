using OpenTK;
using System.Collections.Generic;
using Type.Interfaces.Control;

namespace Type.Controllers
{
    public sealed class PositionRelayer
    {
        /// <summary> The instance of the PositionRelayer</summary>
        private static PositionRelayer _Instance;
        /// <summary> The instance of the PositionRelayer</summary>
        public static PositionRelayer Instance => _Instance ?? (_Instance = new PositionRelayer());

        /// <summary> List of all the <see cref="IPositionRecipient"/>'s </summary>
        private readonly List<IPositionRecipient> _Recipients;

        /// <summary> Vector2 to relay to recipients </summary>
        private Vector2 _PositionToRelay;

        /// <summary>
        /// Relays the given position to all registered <see cref="IPositionRecipient"/>'s
        /// </summary>
        private PositionRelayer()
        {
            _Recipients = new List<IPositionRecipient>();
        }

        /// <summary>
        /// Add the given <see cref="IPositionRecipient"/> to list of recipients
        /// </summary>
        /// <param name="recipient"></param>
        public void AddRecipient(IPositionRecipient recipient)
        {
            if (_Recipients.Contains(recipient)) return;
            _Recipients.Add(recipient);
            RelayPosition(_PositionToRelay);
        }

        /// <summary>
        /// Provides the position to relay to the <see cref="PositionRelayer"/>
        /// </summary>
        /// <param name="position"></param>
        public void ProvidePosition(Vector2 position)
        {
            _PositionToRelay = position;
            RelayPosition(_PositionToRelay);
        }

        /// <summary>
        /// Send the position to all <see cref="IPositionRecipient"/>'s
        /// </summary>
        /// <param name="position"></param>
        private void RelayPosition(Vector2 position)
        {
            foreach (IPositionRecipient recipient in _Recipients)
            {
                recipient.UpdatePositionData(position);
            }
        }

        /// <summary>
        /// Remove the given <see cref="IPositionRecipient"/> from list of recipients
        /// </summary>
        /// <param name="recipient"></param>
        public void RemoveRecipient(IPositionRecipient recipient)
        {
           if (_Recipients.Contains(recipient)) _Recipients.Remove(recipient);
        }
    }
}
