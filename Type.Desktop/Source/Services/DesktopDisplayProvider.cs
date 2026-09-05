using OpenTK;
using System;
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
        }

        /// <inheritdoc />
        public void SetMode(DisplayMode mode)
        {
            if (_Window == null) return;

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
                        break;
                    }
                case DisplayMode.BORDERLESS:
                    {
                        _Window.WindowState = WindowState.Normal;
                        _Window.WindowBorder = WindowBorder.Hidden;
                        _Window.WindowState = WindowState.Maximized;
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
    }
}
