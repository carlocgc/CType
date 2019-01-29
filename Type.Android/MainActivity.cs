using AmosAndroid;
using Android.App;
using Android.Content.PM;

namespace Type.Android
{
    [Activity(Label = "C:Type", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : GameActivity
    {
        public MainActivity() : base(new Game())
        {
        }
    }
}

