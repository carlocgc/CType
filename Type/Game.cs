using AmosShared.Base;
#if __ANDROID__
using AmosShared.Competitive;
#endif // #if __ANDROID__
using AmosShared.Graphics;
using AmosShared.State;
using Engine.Shared.Graphics.Textures;
using OpenTK;
using Type.Data;
using Type.Services;
using Type.States;

namespace Type
{
    public class Game : BaseGame
    {
        /// <summary> Main canvas for the game graphics </summary>
        public static Canvas MainCanvas;

        /// <summary> Main canvas for the UI elements </summary>
        public static Canvas UiCanvas;

        /// <remarks>
        /// The name reaches the engine only as the directory its key value store lives in, so it
        /// is the filesystem-safe form rather than the one shown to the player. Renaming it from
        /// "Test Game" moves the engine's achievement and leaderboard store, which is inert on
        /// desktop because <c>CompetitiveManager</c> only loads under Android. The game's own
        /// save is unaffected: it moved out of that store in S11, and the migration that reads
        /// the old one looks for the previous name by literal.
        /// </remarks>
        public Game() : base(Constants.Global.STORE_NAME, 60)
        {

        }

        public override Vector2 InitialResolution => new Vector2(1920, 1080);

        public override void LoadContent()
        {
            MainCanvas = new Canvas(new Camera(Vector2.Zero, new Vector2(1920, 1080)), 0,
                new Shader());
            UiCanvas = new Canvas(new Camera(Vector2.Zero, new Vector2(1920, 1080)), 1,
                new Shader());

#if __ANDROID__
            AdService.Instance.Initialise(Constants.Global.ADMOB_APP_ID);
#endif // #if __ANDROID__

            // Must come first: both of the below read through it.
            StorageService.Instance.Load();
            Settings.Load();
            ControlSettings.Load();
            Progress.Load();

            SpritesheetLoader.LoadSheet("Content/Graphics/KenPixel/", "KenPixel.png", "KenPixel.json");
            SpritesheetLoader.LoadSheet("Content/Graphics/Background/Planets/", "planets.png", "planets.json");

#if __ANDROID__
            CompetitiveManager.Instance.LoadData(Constants.GameAchievements.GetAll(), Constants.Leaderboards.GetAll());
#endif // #if __ANDROID__

            StateManager.Instance.StartState(new EngineSplashState());
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
