using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.State;
using Engine.Shared.Graphics.Textures;
using OpenTK;
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
                new Shader("Graphics/Shader/DefaultVertexShader.txt", "Graphics/Shader/DefaultFragmentShader.txt"));
            UiCanvas = new Canvas(new Camera(Vector2.Zero, new Vector2(1920, 1080)), 1,
                new Shader("Graphics/Shader/DefaultVertexShader.txt", "Graphics/Shader/DefaultFragmentShader.txt"));

            SpritesheetLoader.LoadSheet("Content/Graphics/KenPixel/", "KenPixel.png", "KenPixel.json");

            StateManager.Instance.StartState(new MainMenuState());
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
