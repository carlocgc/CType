using System;
using System.Collections.Generic;
using System.Linq;
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
            using (GameWindow window = new GameWindow(game.InitialResolution, 0.7f, game, "Test Game"))
            {
                // Hand the window to the display provider before the loop starts, so the saved
                // display mode can be applied while content loads.
                DesktopDisplayProvider.Attach(window);

                window.Run();
                game.Dispose();
            }
        }
    }
}
