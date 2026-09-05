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
        /// <summary> Tells the player how to leave a sub screen that has no prompt of its own </summary>
        private readonly InputPrompt _BackPrompt;

        /// <summary> The commands shown, in the order they are navigated </summary>
        public List<MenuTextItem> Items { get; }

        /// <summary>
        /// Builds the overlay
        /// </summary>
        /// <param name="onResume"> Invoked to continue the run </param>
        /// <param name="onHelp"> Invoked to show what the pickups do </param>
        /// <param name="onOptions"> Invoked to open the settings over the paused game </param>
        /// <param name="onRestart"> Invoked to abandon the run and start again </param>
        /// <param name="onQuit"> Invoked to abandon the run and return to the menu </param>
        public PauseScene(Action onResume, Action onHelp, Action onOptions, Action onRestart, Action onQuit)
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
                Position = new Vector2(0, 380),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;

            Items = new List<MenuTextItem>
            {
                new MenuTextItem("RESUME", new Vector2(0, 180), onResume, centred: true),
                new MenuTextItem("HELP", new Vector2(0, 90), onHelp, centred: true),
                new MenuTextItem("OPTIONS", new Vector2(0, 0), onOptions, centred: true),
                new MenuTextItem("RESTART", new Vector2(0, -90), onRestart, centred: true),
                new MenuTextItem("QUIT", new Vector2(0, -180), onQuit, centred: true),
            };

            _ResumePrompt = new InputPrompt(ButtonData.Type.PAUSE, "RESUME", new Vector2(-880, -530));

            _BackPrompt = new InputPrompt(ButtonData.Type.CANCEL, "BACK", new Vector2(-880, -530))
            {
                Visible = false,
            };
        }

        /// <summary>
        /// Hides or shows the menu itself while a sub screen is open over it. The dark wash
        /// stays, so whatever opens on top does not need one of its own.
        /// </summary>
        /// <param name="visible"> Whether the menu is shown </param>
        public void SetMenuVisible(Boolean visible)
        {
            _Title.Visible = visible;
            _ResumePrompt.Visible = visible;
            foreach (MenuTextItem item in Items) item.SetVisible(visible);
        }

        /// <summary>
        /// Shows a way out for a sub screen that does not carry its own prompt, such as the
        /// pickup guide. The settings screen supplies its own, so it does not need this.
        /// </summary>
        /// <param name="visible"> Whether the prompt is shown </param>
        public void SetBackPromptVisible(Boolean visible)
        {
            _BackPrompt.Visible = visible;
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
            _BackPrompt.Dispose();
            _Title.Dispose();
            _Scrim.Dispose();
        }
    }
}
