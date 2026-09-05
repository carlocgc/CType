using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Data;
using Type.UI;
using Type.UI.Navigation;

namespace Type.Scenes
{
    /// <summary>
    /// The pause overlay: a dark wash over the frozen game, a title, and the commands the player
    /// can reach without leaving the run.
    /// </summary>
    /// <remarks>
    /// Laid out to the right of centre because the powerup help the pause screen already showed
    /// occupies the left edge, and that help is worth keeping.
    /// </remarks>
    public class PauseScene : Scene
    {
        /// <summary> Darkens the frozen game behind the menu </summary>
        private readonly Sprite _Scrim;
        /// <summary> The overlay title </summary>
        private readonly TextDisplay _Title;
        /// <summary> Tells the player how to resume without using the menu </summary>
        private readonly InputPrompt _ResumePrompt;

        /// <summary> The commands shown, in the order they are navigated </summary>
        public List<MenuTextItem> Items { get; }

        /// <summary>
        /// Builds the overlay
        /// </summary>
        /// <param name="onResume"> Invoked to continue the run </param>
        /// <param name="onOptions"> Invoked to open the settings over the paused game </param>
        /// <param name="onRestart"> Invoked to abandon the run and start again </param>
        /// <param name="onQuit"> Invoked to abandon the run and return to the menu </param>
        public PauseScene(Action onResume, Action onOptions, Action onRestart, Action onQuit)
        {
            _Scrim = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME,
                Texture.GetTexture("Content/Graphics/Engine/engine_background.png"))
            {
                Position = new Vector2(-960, -600),
                Colour = new Vector4(0, 0, 0, 0.6f),
                Visible = true,
            };

            _Title = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "PAUSED",
                Position = new Vector2(350, 400),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;

            Items = new List<MenuTextItem>
            {
                new MenuTextItem("RESUME", new Vector2(150, 200), onResume),
                new MenuTextItem("OPTIONS", new Vector2(150, 100), onOptions),
                new MenuTextItem("RESTART", new Vector2(150, 0), onRestart),
                new MenuTextItem("QUIT", new Vector2(150, -100), onQuit),
            };

            _ResumePrompt = new InputPrompt(ButtonData.Type.START, "RESUME", new Vector2(-880, -530));
        }

        /// <summary>
        /// Hides or shows the overlay, used while the settings are open over the top of it
        /// </summary>
        /// <param name="visible"> Whether the overlay is shown </param>
        public void SetVisible(Boolean visible)
        {
            _Scrim.Visible = visible;
            _Title.Visible = visible;
            _ResumePrompt.Visible = visible;
            foreach (MenuTextItem item in Items) item.SetVisible(visible);
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            foreach (MenuTextItem item in Items) item.Dispose();
            Items.Clear();
            _ResumePrompt.Dispose();
            _Title.Dispose();
            _Scrim.Dispose();
        }
    }
}
