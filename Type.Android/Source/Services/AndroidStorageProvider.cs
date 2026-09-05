using AmosShared.Base;
using System;
using Type.Interfaces.Service;

namespace Type.Android.Source.Services
{
    /// <summary>
    /// Keeps the game's settings and progress in the engine's key value store
    /// </summary>
    /// <remarks>
    /// Android keeps using <c>DataLoader</c>. The problem that moved desktop off it — isolated
    /// storage scoping the save to the executable's path, ROADMAP item S11 — does not arise
    /// here, because an installed package's storage is keyed to the application rather than to
    /// a directory the player can move. Behaviour is therefore unchanged from before the
    /// provider existed.
    /// </remarks>
    public sealed class AndroidStorageProvider : IStorageProvider
    {
        /// <inheritdoc />
        /// <remarks>
        /// Nothing to do: the engine initialises its store in <c>BaseGame.Init</c>, before
        /// content loading begins.
        /// </remarks>
        public void Load()
        {
        }

        /// <inheritdoc />
        public Object GetValue(String key)
        {
            return DataLoader.GetValue(key);
        }

        /// <inheritdoc />
        public void SetValue(String key, Object value)
        {
            DataLoader.SetValue(key, value);
        }
    }
}
