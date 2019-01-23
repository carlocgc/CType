using AmosShared.Audio;
using AmosShared.Graphics;
using System;
using Type.Interfaces.Player;
using Type.Objects.Player;
using Type.Objects.World;

namespace Type.Scenes
{
    public class GameScene : Scene
    {
        /// <summary> Scrolling background </summary>
        private readonly ScrollingBackground _BackgroundFar;
        /// <summary> Scrolling background </summary>
        private readonly ScrollingBackground _BackgroundNear;
        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _PlanetsNear;
        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _PlanetsFar;
        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _Clusters;

        /// <summary> The players ship </summary>
        public IPlayer Player { get; }

        public GameScene()
        {
            _BackgroundFar = new ScrollingBackground(100, "Content/Graphics/stars-1.png");
            _BackgroundNear = new ScrollingBackground(200, "Content/Graphics/stars-2.png");
            _Clusters = new ScrollingObject(100, 200, "Content/Graphics/cluster-", 7, 20, 40, Constants.ZOrders.CLUSTERS);
            _PlanetsFar = new ScrollingObject(200, 250, "Content/Graphics/planet-far-", 9, 10, 20, Constants.ZOrders.PLANETS_FAR);
            _PlanetsNear = new ScrollingObject(250, 350, "Content/Graphics/planet-near-", 9, 10, 30, Constants.ZOrders.PLANETS_NEAR);

            Player = new PlayerAlpha();

            new AudioPlayer("Content/Audio/bgm-1.wav", true, AudioManager.Category.MUSIC, 1);
        }

        /// <summary>
        /// Starts the background moving
        /// </summary>
        public void StartBackgroundScroll()
        {
            _BackgroundNear.Start();
            _BackgroundFar.Start();
            _PlanetsFar.Start();
            _PlanetsNear.Start();
            _Clusters.Start();
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        public override void Dispose()
        {
            base.Dispose();

            Player.Dispose();

            _BackgroundNear.Dispose();
            _BackgroundFar.Dispose();
            _PlanetsNear.Dispose();
            _PlanetsFar.Dispose();
            _Clusters.Dispose();

            AudioManager.Instance.Dispose();
        }
    }
}
