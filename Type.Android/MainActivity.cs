using AmosAndroid;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase.Analytics;

namespace Type.Android
{
    [Activity(Label = "C:Type", Name = "com.ctype.MainActivity", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : GameActivity
    {

        private FirebaseAnalytics _FirebaseAnalytics;

        public MainActivity() : base(new Game())
        {
        }


        #region Overrides of GameActivity

        /// <summary> Called when the activity is created - it creates the view and the game </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _FirebaseAnalytics = FirebaseAnalytics.GetInstance(this);
            Bundle eventBundle = new Bundle();
            eventBundle.PutString(FirebaseAnalytics.Param.ItemId, "TestID");
            eventBundle.PutString(FirebaseAnalytics.Param.ItemName, "GameLaunch");
            eventBundle.PutString(FirebaseAnalytics.Param.ContentType, "TestType");
            _FirebaseAnalytics.LogEvent(FirebaseAnalytics.Event.SelectContent, eventBundle);
        }

        #endregion

        #region Overrides of Activity

        /// <summary>
        /// Invoked when the user presses the back button, do not call base as no functionality is required from the press
        /// </summary>
        public override void OnBackPressed()
        {
        }

        #endregion
    }
}

