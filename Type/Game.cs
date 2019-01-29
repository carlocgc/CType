using System;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.State;
using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Engine.Shared.Graphics.Textures;
using OpenTK;
using Type.States;
using Android.Gms.Ads;
using Android.Icu.Text;
using Android.Telephony;
using Type.Ads;

namespace Type
{
    public class Game : BaseGame
    {
        /// <summary> Main canvas for the game graphics </summary>
        public static Canvas MainCanvas;

        /// <summary> Main canvas for the UI elements </summary>
        public static Canvas UiCanvas;

        public static InterstitialAd MInterstitialAd;

        public Action OnAdClosed;

        public Game() : base("Test Game", 60)
        {

        }

        public override Vector2 InitialResolution => new Vector2(1920, 1080);

        public override void LoadContent()
        {
            MainCanvas = new Canvas(new Camera(Vector2.Zero, new Vector2(1920, 1080)), 0,
                new Shader());
            UiCanvas = new Canvas(new Camera(Vector2.Zero, new Vector2(1920, 1080)), 1,
                new Shader());

            SpritesheetLoader.LoadSheet("Content/Graphics/KenPixel/", "KenPixel.png", "KenPixel.json");

            StateManager.Instance.StartState(new EngineSplashState());

            Context context = Android.MainActivity.Instance;
            MInterstitialAd = new InterstitialAd(context);
            MInterstitialAd.AdUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ad unit
            MobileAds.Initialize(context, "ca-app-pub-4204969324853965~4341189590"); // My Admob ID
            AdRequest request = new AdRequest.Builder().AddTestDevice("7DBD856302197638").Build(); // Ad request
            MInterstitialAd.LoadAd(request);
            MInterstitialAd.AdListener = new CustomAdListener();
        }

        protected override Vector2 CalculateExtraOffset(float heightDifference)
        {
            return Vector2.Zero;
        }

        protected override void DisposeGameElements()
        {

        }
    }
}
