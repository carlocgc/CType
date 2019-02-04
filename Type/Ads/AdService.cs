#if __ANDROID__
using Type.Android.Services;
#elif DESKTOP
using Type.Desktop.Services;
#endif
using System;
using Type.Interfaces.Service;

namespace Type.Ads
{
    /// <summary>
    /// Advertisement Service
    /// </summary>
    public sealed class AdService
    {
        /// <summary> The instance of the AdService </summary>
        private static AdService _Instance;

        /// <summary> The instance of the PlayStoreAdService </summary>
        public static AdService Instance => _Instance ?? (_Instance = new AdService());

        /// <summary> Platform specific ad service </summary>
        private readonly IAdService _AdServiceProvider;

        /// <summary> Whether an ad is loaded </summary>
        public Boolean IsLoaded => _AdServiceProvider.IsLoaded;

        /// <summary>
        /// Invoked when an ad is closed by the user
        /// </summary>
        public Action OnAddClosed
        {
            get => _AdServiceProvider.OnAddClosed;
            set => _AdServiceProvider.OnAddClosed = value;
        }

        /// <summary>
        /// Invoked when an ad is clicked by the user
        /// </summary>
        public Action OnAddClicked
        {
            get => _AdServiceProvider.OnAddClicked;
            set => _AdServiceProvider.OnAddClicked = value;
        }

        /// <summary>
        /// Invoked when the application has been left, usually by clicking an ad
        /// </summary>
        public Action OnLeaveApplication
        {
            get => _AdServiceProvider.OnLeaveApplication;
            set => _AdServiceProvider.OnLeaveApplication = value;
        }

        private AdService()
        {
#if __ANDROID__
            _AdServiceProvider = new PlayStoreAdService();
#elif DESKTOP
            _AdServiceProvider = new DesktopAdService();
#endif
        }

        /// <summary>
        /// Initialises the service by setting the service unique id
        /// </summary>
        /// <param name="serviceId"> Service id, AdMob/Google ads etc. </param>
        public void Initialise(String serviceId)
        {
            _AdServiceProvider.Initialise(serviceId);
        }

        #region Interstitial

        /// <summary>
        /// Load the next ad, should be invoked as soon as possible so ad is ready to display when nessesary, usually straight after an ad is shown
        /// </summary>
        public void LoadInterstitial()
        {
            _AdServiceProvider.LoadInterstitial();
        }

        /// <summary>
        /// Show the interstitial ad
        /// </summary>
        public void ShowInterstitial()
        {
            _AdServiceProvider.ShowInterstitial();
        }

        #endregion
    }
}