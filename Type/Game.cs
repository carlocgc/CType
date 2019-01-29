using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.State;
using Engine.Shared.Graphics.Textures;
using OpenTK;
using Type.Ads;
using Type.States;

namespace Type
{
    public class Game : BaseGame
    {
        /// <summary> Main canvas for the game graphics </summary>
        public static Canvas MainCanvas;

        /// <summary> Main canvas for the UI elements </summary>
        public static Canvas UiCanvas;

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

            AdService.Instance.InitialiseInterstitial();

            SpritesheetLoader.LoadSheet("Content/Graphics/KenPixel/", "KenPixel.png", "KenPixel.json");

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
