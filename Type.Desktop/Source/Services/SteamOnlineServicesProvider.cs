using Steamworks;
using System;
using Type.Interfaces.Service;

namespace Type.Desktop.Source.Services
{
    /// <summary>
    /// Starts, ticks and shuts down the Steam client SDK through Facepunch.Steamworks.
    /// </summary>
    /// <remarks>
    /// Facepunch rather than Steamworks.NET because every published Steamworks.NET package is
    /// `netstandard2.1`, which .NET Framework 4.8 cannot consume. Facepunch ships an x64 native
    /// and nothing else, which is why the desktop build is x64. See ROADMAP S6 for the spike.
    /// </remarks>
    public sealed class SteamOnlineServicesProvider : IOnlineServicesProvider
    {
        /// <summary>
        /// Environment variable that suppresses Steam entirely
        /// </summary>
        /// <remarks>
        /// Exists so a gamepad can be tested while the game still borrows an app id. Steam takes
        /// the controller away from a game whose id is configured for Steam Input, and app 480 —
        /// the SDK's own Steam Input sample — is. See ROADMAP S6.
        /// </remarks>
        private const String DisableVariable = "CTYPE_NO_STEAM";

        /// <inheritdoc />
        public Boolean Available { get; private set; }

        /// <inheritdoc />
        /// <remarks>
        /// **Failing here is normal and must stay silent to the player.** Steam not running, not
        /// installed, or a build launched straight from the output directory all land here, and
        /// none of them is a reason a single player game cannot start. Everything built on top
        /// asks <see cref="Available"/> rather than assuming.
        /// </remarks>
        public void Initialise()
        {
            // Deliberately before the try: an unavailable Steam is a state the rest of the game
            // already handles, so opting out lands on the path Steam not running lands on.
            if (Environment.GetEnvironmentVariable(DisableVariable) != null)
            {
                Available = false;
                return;
            }

            try
            {
                // Callbacks are pumped from the game loop rather than by a thread Facepunch
                // starts for itself, so that they arrive in step with everything else.
                SteamClient.Init(Constants.Global.STEAM_APP_ID, false);
                Available = true;
            }
            catch (Exception)
            {
                Available = false;
            }
        }

        /// <inheritdoc />
        public void Update()
        {
            if (!Available) return;

            SteamClient.RunCallbacks();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!Available) return;

            Available = false;
            SteamClient.Shutdown();
        }
    }
}
