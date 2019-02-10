using Android.Gms.Ads;
using System;
using Type.Ads;
using Type.Interfaces.Service;

namespace Type.Android.Services
{
    /// <summary>
    /// Adsevice for Google Play Store platform
    /// </summary>
    public sealed class PlayStoreAdService : IAdService
    {
        /// <summary>
        /// Whether an ad is loaded
        /// </summary>
        public Boolean IsLoaded => _MInterstitialAd.IsLoaded;

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

        public PlayStoreAdService()
        {
        }

        /// <summary>
        /// Initialises the service by setting the service unique id
        /// </summary>
        /// <param name="serviceId"> Service id, AdMob/Google ads etc. </param>
        public void Initialise(String serviceId)
        {
            MobileAds.Initialize(MainActivity.Instance, serviceId);
            CreateInterstitial();
        }

        /// <summary>
        /// Adds the assignable actions as a listener to the ad events
        /// </summary>
        private void AttachListener()
        {
            CustomAdListener cadl = new CustomAdListener
            {
                OnAdClickedAction = () =>
                {
                    OnAddClicked?.Invoke();
                    LoadInterstitial();
                },
                OnAdLeftApplicationAction = () =>
                {
                    OnLeaveApplication?.Invoke();
                    LoadInterstitial();
                },
                OnAdClosedAction = () =>
                {
                    OnAddClosed?.Invoke();
                    LoadInterstitial();
                }
            };
            _MInterstitialAd.AdListener = cadl;
        }

        #region Interstitial

        /// <summary>
        /// Interstitial ad object
        /// </summary>
        private InterstitialAd _MInterstitialAd { get; set; }

        /// <summary>
        /// Creates interstitial ad object, sets the AdUnit id and preloads an ad request
        /// </summary>
        private void CreateInterstitial()
        {
            _MInterstitialAd = new InterstitialAd(MainActivity.Instance)
            {
                AdUnitId = "ca-app-pub-4204969324853965/8810416639" // TODO FIX TEST AD Replace with ad unit id from AdMob
            };

            LoadInterstitial();
        }

        /// <summary>
        /// Load the next ad, should be invoked as soon as possible so ad is ready to display when nessesary, usually straight after an ad is shown
        /// </summary>
        public void LoadInterstitial()
        {
            AdRequest request = new AdRequest.Builder().Build(); // TODO FIX TEST AD Remove '.AddTestDevice(XXXXXXX)'
            _MInterstitialAd.LoadAd(request);
        }

        /// <summary>
        /// Show the interstitial ad
        /// </summary>
        public void ShowInterstitial()
        {
            if (!_MInterstitialAd.IsLoaded)
            {
                OnAddClosed?.Invoke();
                return;
            }

            AttachListener();
            _MInterstitialAd.Show();
        }

        #endregion
    }
}
