using OpenTK;
using System;
using System.Drawing;
using Type.Data;
using Type.Interfaces.Service;

namespace Type.Desktop.Source.Services
{
    /// <summary>
    /// Applies display modes to the OpenTK window
    /// </summary>
    /// <remarks>
    /// The window is created by the entry point, before any shared code runs, so it is handed
    /// here through <see cref="Attach"/> rather than resolved. Until that happens every request
    /// is ignored, which keeps a provider constructed early from throwing.
    /// </remarks>
    public sealed class DesktopDisplayProvider : IDisplayProvider
    {
        /// <summary> The window the game is drawn in, or null before the entry point attaches it </summary>
        private static GameWindow _Window;

        /// <summary> Surface width used for windowed mode, matching the size the game starts at </summary>
        private static Int32 _WindowedWidth;
        /// <summary> Surface height used for windowed mode </summary>
        private static Int32 _WindowedHeight;

        /// <summary> Where the window sat when it was last windowed </summary>
        private static Int32 _WindowedX;
        /// <summary> Where the window sat when it was last windowed </summary>
        private static Int32 _WindowedY;

        /// <summary>
        /// The monitor slots OpenTK exposes, searched in order when placing a borderless window
        /// </summary>
        /// <remarks>
        /// Enumerated by slot rather than through <c>AvailableDisplays</c>, which is obsolete.
        /// <c>GetDisplay</c> returns null for a slot no monitor occupies.
        /// </remarks>
        private static readonly DisplayIndex[] DisplayIndices =
        {
            DisplayIndex.First, DisplayIndex.Second, DisplayIndex.Third,
            DisplayIndex.Fourth, DisplayIndex.Fifth, DisplayIndex.Sixth,
        };

        /// <inheritdoc />
        public Boolean CanChangeMode => true;

        /// <summary>
        /// Gives the provider the window to act on. Called by the entry point once, before the
        /// game loop starts.
        /// </summary>
        /// <param name="window"> The game window </param>
        public static void Attach(GameWindow window)
        {
            _Window = window;
            if (window == null) return;

            _WindowedWidth = window.Width;
            _WindowedHeight = window.Height;
            _WindowedX = window.X;
            _WindowedY = window.Y;
        }

        /// <inheritdoc />
        public void SetMode(DisplayMode mode)
        {
            if (_Window == null) return;

            // Remember where a windowed window sits before leaving it. Borderless used to be a
            // maximise, and coming back out of one restored the position for free; sizing to the
            // display instead does not, so it is tracked rather than lost.
            if (_Window.WindowBorder != WindowBorder.Hidden)
            {
                _WindowedX = _Window.X;
                _WindowedY = _Window.Y;
            }

            switch (mode)
            {
                case DisplayMode.WINDOWED:
                    {
                        // Leave fullscreen before restoring the border, or the border is applied
                        // to a window still sized to the whole display.
                        _Window.WindowState = WindowState.Normal;
                        _Window.WindowBorder = WindowBorder.Resizable;
                        _Window.Width = _WindowedWidth;
                        _Window.Height = _WindowedHeight;
                        _Window.X = _WindowedX;
                        _Window.Y = _WindowedY;
                        break;
                    }
                case DisplayMode.BORDERLESS:
                    {
                        // Sized to the display rather than maximised. Maximising respects the
                        // desktop work area, so the taskbar stayed drawn over the bottom of what
                        // is meant to be a fullscreen game.
                        _Window.WindowState = WindowState.Normal;
                        _Window.WindowBorder = WindowBorder.Hidden;
                        _Window.Bounds = DisplayFor(_Window).Bounds;
                        break;
                    }
                case DisplayMode.FULLSCREEN:
                    {
                        _Window.WindowBorder = WindowBorder.Hidden;
                        _Window.WindowState = WindowState.Fullscreen;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Display mode does not exist");
            }
        }

        /// <summary>
        /// The display a window is sitting on, judged by where its centre falls
        /// </summary>
        /// <param name="window"> The window to place </param>
        /// <returns> The display containing the window's centre, or the primary one if no
        /// display does </returns>
        /// <remarks>
        /// Maximising used to pick the right display on its own. Sizing to a display explicitly
        /// means choosing it explicitly too, or going borderless would drag the window back to
        /// the primary monitor from whichever one the player had put it on.
        /// </remarks>
        private static DisplayDevice DisplayFor(INativeWindow window)
        {
            Point centre = new Point(window.Bounds.Left + window.Bounds.Width / 2,
                                     window.Bounds.Top + window.Bounds.Height / 2);

            for (Int32 index = 0; index < DisplayIndices.Length; index++)
            {
                DisplayDevice display = DisplayDevice.GetDisplay(DisplayIndices[index]);
                if (display != null && display.Bounds.Contains(centre)) return display;
            }

            return DisplayDevice.Default;
        }
    }
}
