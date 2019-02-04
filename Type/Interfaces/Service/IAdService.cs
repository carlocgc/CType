using System;

namespace Type.Interfaces.Service
{
    /// <summary>
    /// Interface for the advertisement service
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// Whether an ad is loaded
        /// </summary>
        Boolean IsLoaded { get; }

        /// <summary>
        /// Invoked when an ad is closed by the user
        /// </summary>
        Action OnAddClosed { get; set; }

        /// <summary>
        /// Invoked when an ad is clicked by the user
        /// </summary>
        Action OnAddClicked { get; set; }

        /// <summary>
        /// Invoked when the application has been left, usually by clicking an ad
        /// </summary>
        Action OnLeaveApplication { get; set; }

        /// <summary>
        /// Initialises the service by setting the service unique id
        /// </summary>
        /// <param name="serviceId"> Service id, AdMob/Google ads etc. </param>
        void Initialise(String serviceId);

        /// <summary>
        /// Load the next ad, should be invoked as soon as possible so ad is ready to display when nessesary, usually straight after an ad is shown
        /// </summary>
        void LoadInterstitial();

        /// <summary>
        /// Show the interstitial ad
        /// </summary>
        void ShowInterstitial();
    }
}
