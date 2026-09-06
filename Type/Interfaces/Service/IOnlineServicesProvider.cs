using System;

namespace Type.Interfaces.Service
{
    /// <summary>
    /// Interface for a platform's online services: the storefront's own SDK, which on desktop is
    /// Steam and on Android is Google Play.
    /// </summary>
    /// <remarks>
    /// Lifecycle only — starting the SDK, giving it the per frame tick most of them need, and
    /// shutting it down. What is built on top of it, achievements today and rich presence and
    /// matchmaking later, goes through its own facade rather than being piled in here.
    /// <para>
    /// "Online" rather than "platform" deliberately, because this codebase now uses platform to
    /// mean the CPU architecture in several places and the two would read as the same word.
    /// </para>
    /// </remarks>
    public interface IOnlineServicesProvider : IDisposable
    {
        /// <summary>
        /// Whether the SDK started and can be used. False is normal and not an error: the player
        /// may have launched the game without the client running, or without it installed.
        /// </summary>
        Boolean Available { get; }

        /// <summary>
        /// Starts the SDK. Must not throw and must not prevent the game from starting when the
        /// platform is absent.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Gives the SDK its per frame tick, which is how it delivers callbacks
        /// </summary>
        void Update();
    }
}
