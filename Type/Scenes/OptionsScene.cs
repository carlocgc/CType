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
    /// The options screen. Currently the audio levels; display mode joins it once the game can
    /// change resolution, and rebinding once there is a binding editor.
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

        public OptionsScene()
        {
            _Background = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND,
                Texture.GetTexture("Content/Graphics/Background/MainMenuBG-2.png"))
            {
                Position = new Vector2(-960, -540),
                Visible = true,
            };

            // The menu art has a bright star at the top centre, exactly where the title and
            // the values sit. A flat dark wash over it keeps the text readable without
            // needing new art or moving the layout off centre.
            _Scrim = new Sprite(Game.MainCanvas, Constants.ZOrders.MENU_SCRIM,
                Texture.GetTexture("Content/Graphics/Engine/engine_background.png"))
            {
                Position = new Vector2(-960, -600),
                Colour = new Vector4(0, 0, 0, 0.65f),
                Visible = true,
            };

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

            _BackPrompt = new InputPrompt(ButtonData.Type.CANCEL, "BACK", new Vector2(-880, -480));
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
            Rows.Clear();
            _BackPrompt.Dispose();
            _Scrim.Dispose();
            _Title.Dispose();
            _Background.Dispose();
        }
    }
}
