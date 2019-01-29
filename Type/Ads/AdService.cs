using System;
using System.Collections.Generic;
using System.Text;
using Android.Gms.Ads;
using Type.Android;

namespace Type.Ads
{
    public class AdService
    {
        /// <summary> The instance of the AdService </summary>
        private static AdService _Instance;
        /// <summary> The instance of the AdService </summary>
        public static AdService Instance => _Instance ?? (_Instance = new AdService());

        public InterstitialAd MInterstitialAd { get; private set; }

        private AdService()
        {

        }

        public void InitialiseInterstitial()
        {
            MobileAds.Initialize(MainActivity.Instance, "ca-app-pub-4204969324853965~4341189590"); // My Admob ID
            MInterstitialAd = new InterstitialAd(MainActivity.Instance);
            MInterstitialAd.AdUnitId = "ca-app-pub-3940256099942544/1033173712"; // TODO FIX TEST AD Replace with ad unit id from AdMob
            LoadInterstitial();
        }

        public void LoadInterstitial()
        {
            AdRequest request = new AdRequest.Builder().AddTestDevice("7DBD856302197638").Build(); // TODO FIX TEST AD Remove '.AddTestDevice(XXXXXXX)'
            MInterstitialAd.LoadAd(request);
        }

        /// <summary>
        /// Show the interstitial ad
        /// </summary>
        /// <param name="onClicked">Action to invoke when the ad has been clicked</param>
        /// <param name="onLeaveApplication">Action to invoke when the the user has left the application </param>
        /// <param name="onClosed"> Action to invoke when the ad has closed </param>
        public void ShowInterstitial(Action onClicked = null, Action onLeaveApplication = null, Action onClosed = null)
        {
            if (!MInterstitialAd.IsLoaded)
            {
                onClosed?.Invoke();
                return;
            }

            CustomAdListener cadl = new CustomAdListener
            {
                OnAdClickedAction = () =>
                {
                    onClicked?.Invoke();
                },
                OnAdLeftApplicationAction = () =>
                {
                    onLeaveApplication?.Invoke();
                },
                OnAdClosedAction = () =>
                {
                    onClosed?.Invoke();
                    AdService.Instance.LoadInterstitial();
                }
            };
            AdService.Instance.MInterstitialAd.AdListener = cadl;
            AdService.Instance.MInterstitialAd.Show();
        }
    }
}
