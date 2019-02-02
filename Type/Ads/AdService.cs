using Android.Gms.Ads;
using System;
using Android.Views;
using Android.Widget;
using Type.Android;

namespace Type.Ads
{
    public class AdService
    {
        /// <summary> The instance of the AdService </summary>
        private static AdService _Instance;
        /// <summary> The instance of the AdService </summary>
        public static AdService Instance => _Instance ?? (_Instance = new AdService());

        public Action OnAddClosed { get; set; }

        public Action OnAddClicked { get; set; }

        public Action OnLeaveApplication { get; set; }

        private AdService()
        {

        }

        public void Initialise()
        {
            MobileAds.Initialize(MainActivity.Instance, "ca-app-pub-4204969324853965~4341189590"); // My Admob ID
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
                    AdService.Instance.LoadInterstitial();
                },
                OnAdLeftApplicationAction = () =>
                {
                    OnLeaveApplication?.Invoke();
                    AdService.Instance.LoadInterstitial();
                },
                OnAdClosedAction = () =>
                {
                    OnAddClosed?.Invoke();
                    AdService.Instance.LoadInterstitial();
                }
            };
            AdService.Instance.MInterstitialAd.AdListener = cadl;
        }

        #region Interstitial

        /// <summary>
        /// Interstitial ad object
        /// </summary>
        public InterstitialAd MInterstitialAd { get; private set; }

        /// <summary>
        /// Creates interstitial ad object, sets the AdUnit id and preloads an ad request
        /// </summary>
        private void CreateInterstitial()
        {
            MInterstitialAd = new InterstitialAd(MainActivity.Instance);
            MInterstitialAd.AdUnitId = "ca-app-pub-3940256099942544/1033173712"; // TODO FIX TEST AD Replace with ad unit id from AdMob
            LoadInterstitial();
        }

        /// <summary>
        /// Load the next ad, should be invoked as soon as possible so ad is ready to display when nessesary, usually straight after an ad is shown
        /// </summary>
        public void LoadInterstitial()
        {
            AdRequest request = new AdRequest.Builder().AddTestDevice("7DBD856302197638").Build(); // TODO FIX TEST AD Remove '.AddTestDevice(XXXXXXX)'
            MInterstitialAd.LoadAd(request);
        }


        /// <summary>
        /// Show the interstitial ad
        /// </summary>
        public void ShowInterstitial()
        {
            if (!MInterstitialAd.IsLoaded)
            {
                OnAddClosed?.Invoke();
                return;
            }

            AttachListener();
            AdService.Instance.MInterstitialAd.Show();
        }

        #endregion

        #region  Banner

        /// <summary>
        /// AdView object
        /// </summary>
        private AdView MAdView { get; set; }

        /// <summary>
        /// WIP Not implemented
        /// </summary>
        private void CreateAdView()
        {
            throw new NotImplementedException();

            AdView adView = new AdView(MainActivity.Instance);
            adView.AdSize = AdSize.Banner;
            adView.AdUnitId = "ca-app-pub-3940256099942544/6300978111"; // TODO FIX TEST AD Replace with ad unit id from AdMob
            LoadAdView();
        }

        /// <summary>
        /// WIP Not implemented
        /// </summary>
        public void LoadAdView()
        {
            throw new NotImplementedException();

            AdRequest adRequest = new AdRequest.Builder().AddTestDevice("7DBD856302197638").Build();
            MAdView.LoadAd(adRequest);
        }


        /// <summary>
        /// WIP not implemented
        /// </summary>
        public void ShowAdView()
        {
            throw new NotImplementedException("AdViews are not implemented");
        }

        #endregion
    }
}
