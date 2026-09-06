using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Data;
using Type.Input;
using Type.UI;
using Type.UI.Navigation;

namespace Type.Scenes
{
    /// <summary>
    /// The controls screen, listing every rebindable action against the inputs bound to it.
    /// Opened from the options screen, on the main menu and over a paused game alike.
    /// </summary>
    public class ControlsScene : Scene
    {
        /// <summary> Left edge of every row </summary>
        private const Single RowLeft = -820;
        /// <summary> Height of the first row </summary>
        private const Single FirstRow = 280;
        /// <summary> Vertical distance between rows </summary>
        private const Single RowStep = 80;
        /// <summary> Distance from the keyboard column to the left edge of a row </summary>
        private const Single KeyColumn = 520;
        /// <summary> Distance from the gamepad column to the left edge of a row </summary>
        private const Single PadColumn = 1180;

        /// <summary> Sprite for the background </summary>
        private readonly Sprite _Background;
        /// <summary> Darkens the background so the bindings stay readable over it </summary>
        private readonly Sprite _Scrim;
        /// <summary> The screen title </summary>
        private readonly TextDisplay _Title;
        /// <summary> Names the three columns </summary>
        private readonly List<TextDisplay> _Headings;
        /// <summary> Tells the player how to leave the screen </summary>
        private readonly InputPrompt _BackPrompt;
        /// <summary> Tells the player how to rebind the focused action </summary>
        private readonly InputPrompt _BindPrompt;

        /// <summary> One row per rebindable action, in the order they are navigated </summary>
        public List<BindingRow> Rows { get; }

        /// <summary> Puts every binding back to the shipped default </summary>
        public MenuTextItem ResetItem { get; }

        /// <summary>
        /// Builds the screen
        /// </summary>
        /// <param name="overlay">
        /// True when shown over something already on screen, such as the paused game, in which
        /// case the menu art is omitted and only the dark wash is drawn.
        /// </param>
        public ControlsScene(Boolean overlay = false)
        {
            if (!overlay)
            {
                _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND,
                    Texture.GetTexture("Content/Graphics/Background/MainMenuBG-2.png"))
                {
                    Position = new Vector2(-960, -540),
                    Visible = true,
                };

                // Same reason as the options screen: the menu art has a bright star behind
                // exactly where this text sits, and a flat wash is cheaper than new art. As an
                // overlay the caller has already darkened the screen.
                _Scrim = new Sprite(Game.MainCanvas, Constants.ZOrders.MENU_SCRIM,
                    Texture.GetTexture("Content/Graphics/Engine/engine_background.png"))
                {
                    Position = new Vector2(-960, -600),
                    Colour = new Vector4(0, 0, 0, 0.65f),
                    Visible = true,
                };
            }

            _Title = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "CONTROLS",
                Position = new Vector2(0, 450),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;
            AddDrawable(_Title);

            _Headings = new List<TextDisplay>
            {
                CreateHeading("ACTION", new Vector2(RowLeft, 360)),
                CreateHeading("KEYBOARD", new Vector2(RowLeft + KeyColumn, 360)),
                CreateHeading("GAMEPAD", new Vector2(RowLeft + PadColumn, 360)),
            };
            foreach (TextDisplay heading in _Headings) AddDrawable(heading);

            Rows = new List<BindingRow>();
            for (Int32 index = 0; index < InputBindings.Rebindable.Length; index++)
            {
                Rows.Add(new BindingRow(InputBindings.Rebindable[index],
                    new Vector2(RowLeft, FirstRow - index * RowStep), RefreshRows));
            }

            ResetItem = new MenuTextItem("RESET DEFAULTS",
                new Vector2(RowLeft, FirstRow - InputBindings.Rebindable.Length * RowStep - 40),
                ResetBindings, false, 2);

            _BindPrompt = new InputPrompt(ButtonData.Type.CONFIRM, "REBIND", new Vector2(-880, -430));
            _BackPrompt = new InputPrompt(ButtonData.Type.CANCEL, "BACK", new Vector2(-880, -480));
        }

        /// <summary>
        /// Creates one column heading
        /// </summary>
        private static TextDisplay CreateHeading(String text, Vector2 position)
        {
            return new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = text,
                Position = position,
                Visible = true,
                Scale = new Vector2(1.5f, 1.5f),
                Colour = new Vector4(0.45f, 0.45f, 0.45f, 1),
            };
        }

        /// <summary>
        /// Puts every binding back to the shipped default and shows the result
        /// </summary>
        private void ResetBindings()
        {
            ControlSettings.ResetToDefaults();
            RefreshRows();
        }

        /// <summary>
        /// Re-reads every row. One rebind can move an input off another action, so a change to
        /// any row can change any other.
        /// </summary>
        private void RefreshRows()
        {
            foreach (BindingRow row in Rows) row.Refresh();
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// The title and the headings went in through <see cref="Scene.AddDrawable"/>, so the
        /// base disposes them. Everything else was built straight onto a canvas and has to be
        /// disposed here.
        /// </remarks>
        public override void Dispose()
        {
            base.Dispose();
            foreach (BindingRow row in Rows) row.Dispose();
            Rows.Clear();
            ResetItem.Dispose();
            _BindPrompt.Dispose();
            _BackPrompt.Dispose();
            _Headings.Clear();
            _Scrim?.Dispose();
            _Background?.Dispose();
        }
    }
}
