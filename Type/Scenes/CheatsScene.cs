#if CTYPE_CHEATS
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
    /// The cheats screen, reached from the options screen and present only in a build that
    /// defines <c>CTYPE_CHEATS</c>.
    /// </summary>
    /// <remarks>
    /// The whole file is inside the symbol rather than only the values it changes, so a build
    /// without cheats has no screen to reach, nothing to navigate to it, and no code that could
    /// set a cheat even if a save claimed one was on.
    /// <para>
    /// Shown as an overlay over the options screen, the same way the controls screen is: the
    /// menu art is already drawn, and the caller hides its own settings first.
    /// </para>
    /// </remarks>
    public class CheatsScene : Scene
    {
        /// <summary> The screen title </summary>
        private readonly TextDisplay _Title;
        /// <summary> Warns that this screen does not exist in a shipped build </summary>
        private readonly TextDisplay _Note;
        /// <summary> Tells the player how to leave the screen </summary>
        private readonly InputPrompt _BackPrompt;

        /// <summary> The cheats shown, in the order they are navigated </summary>
        public List<OptionRow> Rows { get; }

        /// <summary> Builds the screen </summary>
        public CheatsScene()
        {
            _Title = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "CHEATS",
                Position = new Vector2(0, 450),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;
            AddDrawable(_Title);

            _Note = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "TEST BUILD ONLY",
                Position = new Vector2(0, 330),
                Scale = new Vector2(1.5f, 1.5f),
                Colour = new Vector4(1, 0.75f, 0.3f, 1),
                Visible = true,
            };
            _Note.Offset = new Vector2(_Note.Size.X * _Note.Scale.X, _Note.Size.Y * _Note.Scale.Y) / 2;
            AddDrawable(_Note);

            Rows = new List<OptionRow>
            {
                new OptionRow("INVINCIBLE", new Vector2(-700, 100),
                    () => Cheats.Invincible ? "ON" : "OFF",
                    step => Cheats.SetInvincible(!Cheats.Invincible)),

                new OptionRow("START LEVEL", new Vector2(-700, 0),
                    () => Cheats.StartLevel.ToString(),
                    step => Cheats.SetStartLevel(Cheats.StartLevel + step)),

                new OptionRow("OMEGA UNLOCKED", new Vector2(-700, -100),
                    () => Cheats.OmegaUnlocked ? "ON" : "OFF",
                    step => Cheats.SetOmegaUnlocked(!Cheats.OmegaUnlocked)),

                new OptionRow("INFINITE BOMBS", new Vector2(-700, -200),
                    () => Cheats.InfiniteBombs ? "ON" : "OFF",
                    step => Cheats.SetInfiniteBombs(!Cheats.InfiniteBombs)),
            };

            _BackPrompt = new InputPrompt(ButtonData.Type.CANCEL, "BACK", new Vector2(-880, -480));
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            foreach (OptionRow row in Rows) row.Dispose();
            Rows.Clear();
            _BackPrompt.Dispose();
            _Note.Dispose();
            _Title.Dispose();
        }
    }
}
#endif // #if CTYPE_CHEATS
