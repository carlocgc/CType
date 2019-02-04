using System;
using Type.Interfaces.Service;

namespace Type.Desktop.Services
{
    /// <summary>
    /// AdService for Desktop build
    /// </summary>
    public sealed class DesktopAdService : IAdService
    {
        /// <summary>
        /// Whether an ad is loaded
        /// </summary>
        public Boolean IsLoaded { get; private set; }

        /// <summary>
        /// Invoked when an ad is closed by the user
        /// </summary>
        public Action OnAddClosed { get; set; }

        /// <summary>
        /// Invoked when an ad is clicked by the user
        /// </summary>
        public Action OnAddClicked { get; set; }

        /// <summary>
        /// Invoked when the application has been left, usually by clicking an ad
        /// </summary>
        public Action OnLeaveApplication { get; set; }

        public DesktopAdService()
        {
        }

        /// <summary>
        /// Initialises the service by setting the service unique id
        /// </summary>
        /// <param name="serviceId"> Service id, AdMob/Google ads etc. </param>
        public void Initialise(String serviceId)
        {
        }

        /// <summary>
        /// Load the next ad, should be invoked as soon as possible so ad is ready to display when nessesary, usually straight after an ad is shown
        /// </summary>
        public void LoadInterstitial()
        {
            IsLoaded = true;
        }

        /// <summary>
        /// Show the interstitial ad
        /// </summary>
        public void ShowInterstitial()
        {
            OnAddClosed?.Invoke();
        }
    }
}
