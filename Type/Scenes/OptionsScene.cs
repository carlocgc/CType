using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Data;
using Type.UI;
using Type.UI.Navigation;
using Type.Services;

namespace Type.Scenes
{
    /// <summary>
    /// The options screen: the audio levels, how the window fills the display, and the way in to
    /// the controls screen.
    /// </summary>
    public class OptionsScene : Scene
    {
        /// <summary> How much one press changes a volume </summary>
        private const Int32 VolumeStep = 10;

        /// <summary> Sprite for the background </summary>
        private readonly Sprite _Background;
        /// <summary> Darkens the background so the settings stay readable over it </summary>
        private readonly Sprite _Scrim;
        /// <summary> The screen title </summary>
        private readonly TextDisplay _Title;
        /// <summary> Tells the player how to leave the screen </summary>
        private readonly InputPrompt _BackPrompt;

        /// <summary> The settings shown, in the order they are navigated </summary>
        public List<OptionRow> Rows { get; }

        /// <summary>
        /// Opens the controls screen, or null on a platform with nothing to rebind
        /// </summary>
        public MenuTextItem ControlsItem { get; }

        /// <summary>
        /// Builds the screen
        /// </summary>
        /// <param name="onControls">
        /// Invoked when the player asks for the controls screen. The caller owns that screen,
        /// because where it goes differs between the main menu and a paused game.
        /// </param>
        /// <param name="overlay">
        /// True when shown over something already on screen, such as the paused game, in which
        /// case the menu art is omitted and only the dark wash is drawn.
        /// </param>
        public OptionsScene(Action onControls, Boolean overlay = false)
        {
            if (!overlay)
            {
                _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND,
                    Texture.GetTexture("Content/Graphics/Background/MainMenuBG-2.png"))
                {
                    Position = new Vector2(-960, -540),
                    Visible = true,
                };
            }

            // The menu art has a bright star at the top centre, exactly where the title and
            // the values sit. A flat dark wash over it keeps the text readable without
            // needing new art or moving the layout off centre.
            // As an overlay the caller already darkened the screen, so a second wash would
            // only make the settings harder to read.
            if (!overlay)
            {
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
                Text = "OPTIONS",
                Position = new Vector2(0, 450),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;
            AddDrawable(_Title);

            Rows = new List<OptionRow>
            {
                new OptionRow("MASTER VOLUME", new Vector2(-700, 150),
                    () => Settings.MasterVolume.ToString(),
                    step => Settings.SetMasterVolume(Settings.MasterVolume + step * VolumeStep)),

                new OptionRow("MUSIC VOLUME", new Vector2(-700, 50),
                    () => Settings.MusicVolume.ToString(),
                    step => Settings.SetMusicVolume(Settings.MusicVolume + step * VolumeStep)),

                new OptionRow("EFFECT VOLUME", new Vector2(-700, -50),
                    () => Settings.EffectVolume.ToString(),
                    step => Settings.SetEffectVolume(Settings.EffectVolume + step * VolumeStep)),
            };

            // Platforms that are always fullscreen have nothing to offer here, so the row is
            // omitted rather than shown doing nothing.
            if (DisplayService.Instance.CanChangeMode)
            {
                Rows.Add(new OptionRow("DISPLAY MODE", new Vector2(-700, -150),
                    () => Settings.DisplayMode.ToString(),
                    CycleDisplayMode));
            }

            Rows.Add(new OptionRow("RUMBLE", new Vector2(-700, -250),
                () => Settings.RumbleIntensity.ToString(),
                step => Settings.SetRumbleIntensity(Settings.RumbleIntensity + step * VolumeStep)));

            // A screen of its own rather than a row, because a binding is a list of inputs per
            // device and there is one per action. A platform with nothing to rebind says so by
            // having no bindings at all, and does not get the entry.
            if (onControls != null && InputService.Instance.Bindings != null)
            {
                ControlsItem = new MenuTextItem("CONTROLS", new Vector2(-700, -350), onControls, false, 2);
            }

            _BackPrompt = new InputPrompt(ButtonData.Type.CANCEL, "BACK", new Vector2(-880, -480));
        }

        /// <summary>
        /// Hides or shows the settings, so a screen opened from here can have the space to
        /// itself without the caller tearing this one down and rebuilding it
        /// </summary>
        /// <param name="visible"> Whether the settings are shown </param>
        public void SetSettingsVisible(Boolean visible)
        {
            _Title.Visible = visible;
            _BackPrompt.Visible = visible;
            foreach (OptionRow row in Rows) row.SetVisible(visible);
            ControlsItem?.SetVisible(visible);
        }

        /// <summary>
        /// Steps the display mode through the available modes, wrapping at both ends
        /// </summary>
        /// <param name="step"> -1 for the previous mode, 1 for the next </param>
        private static void CycleDisplayMode(Int32 step)
        {
            Array modes = Enum.GetValues(typeof(DisplayMode));
            Int32 index = Array.IndexOf(modes, Settings.DisplayMode);
            Int32 next = ((index + step) % modes.Length + modes.Length) % modes.Length;

            Settings.SetDisplayMode((DisplayMode)modes.GetValue(next));
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
            ControlsItem?.Dispose();
            Rows.Clear();
            _BackPrompt.Dispose();
            _Scrim?.Dispose();
            _Title.Dispose();
            _Background?.Dispose();
        }
    }
}
