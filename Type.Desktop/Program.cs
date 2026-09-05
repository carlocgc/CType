using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AmosDesktop;
using Type.Desktop.Source.Services;

namespace Type.Desktop
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            using (GameWindow window = new GameWindow(game.InitialResolution, 0.7f, game, Constants.Global.STORE_NAME))
            {
                // The engine hardcodes "Game" as the window title in its own constructor, so the
                // real one is set here rather than passed in. The constructor argument above is
                // the engine's AssemblyName property, which nothing reads.
                window.Title = Constants.Global.TITLE;
                window.Icon = LoadIcon();

                // Hand the window to the display provider before the loop starts, so the saved
                // display mode can be applied while content loads.
                DesktopDisplayProvider.Attach(window);

                window.Run();
                game.Dispose();
            }
        }

        /// <summary>
        /// The icon compiled into this executable, for the window and the taskbar
        /// </summary>
        /// <returns> The icon, or null if it cannot be read </returns>
        /// <remarks>
        /// Taken from the executable rather than shipped alongside it as content, so there is one
        /// icon to keep current rather than two copies that can disagree.
        /// </remarks>
        private static Icon LoadIcon()
        {
            try
            {
                return Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            }
            catch (Exception)
            {
                // A missing icon is a cosmetic problem, not a reason to refuse to start.
                return null;
            }
        }
    }
}
