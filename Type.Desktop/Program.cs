using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmosDesktop;

namespace Type.Desktop
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            using (GameWindow window = new GameWindow(game.InitialResolution, 0.7f, game, "Test Game"))
            {
                window.Run();
                game.Dispose();
            }
        }
    }
}
