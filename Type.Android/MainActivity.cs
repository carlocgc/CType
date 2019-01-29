using AmosAndroid;
using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;

namespace Type.Android
{
    [Activity(Label = "C:Type", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : GameActivity
    {
        public MainActivity() : base(new Game())
        {
        }

        /// <inheritdoc />
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            MobileAds.Initialize(this, "ca-app-pub-4204969324853965~4341189590"); // My Admob ID
            MInterstitialAd = new InterstitialAd(this);
            MInterstitialAd.AdUnitId = "ca-app-pub-3940256099942544/1033173712";                   // Test ad unit
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            AdRequest request = new AdRequest.Builder().AddTestDevice("7DBD856302197638").Build(); // Ad request
            MInterstitialAd.LoadAd(request);
        }
    }
}

