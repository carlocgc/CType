using AmosAndroid;
using Android.App;
using Android.Content.PM;

namespace Type.Android
{
    [Activity(Label = "C:Type", Name = "com.ctype.MainActivity", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : GameActivity
    {
        public MainActivity() : base(new Game())
        {
        }

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

