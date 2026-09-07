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

                new OptionRow("OMEGA SHIP", new Vector2(-700, -100),
                    () => Describe(Cheats.OmegaUnlock),
                    step => Cheats.SetOmegaUnlock(Cycle(Cheats.OmegaUnlock, step))),

                new OptionRow("INFINITE BOMBS", new Vector2(-700, -200),
                    () => Cheats.InfiniteBombs ? "ON" : "OFF",
                    step => Cheats.SetInfiniteBombs(!Cheats.InfiniteBombs)),
            };

            _BackPrompt = new InputPrompt(ButtonData.Type.CANCEL, "BACK", new Vector2(-880, -480));
        }

        /// <summary>
        /// How an override reads on screen. "CAMPAIGN" rather than "DEFAULT" because what it
        /// means here is that the ship is unlocked by finishing the game, as it is in a shipped
        /// build, which is the thing a tester most needs to be able to get back to.
        /// </summary>
        /// <param name="state"> The override to describe </param>
        /// <returns> The text for the row </returns>
        private static String Describe(CheatOverride state)
        {
            switch (state)
            {
                case CheatOverride.FORCED_ON:
                    {
                        return "UNLOCKED";
                    }
                case CheatOverride.FORCED_OFF:
                    {
                        return "LOCKED";
                    }
                default:
                    {
                        return "CAMPAIGN";
                    }
            }
        }

        /// <summary>
        /// Steps an override to the next state, wrapping at both ends
        /// </summary>
        /// <param name="state"> The override now </param>
        /// <param name="step"> -1 for the previous state, 1 for the next </param>
        /// <returns> The override to move to </returns>
        private static CheatOverride Cycle(CheatOverride state, Int32 step)
        {
            Array states = Enum.GetValues(typeof(CheatOverride));
            Int32 index = Array.IndexOf(states, state);
            Int32 next = ((index + step) % states.Length + states.Length) % states.Length;

            return (CheatOverride)states.GetValue(next);
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
