using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    class Program {
        static void Main(string[] args) {
            Player[] player = new Player[] {
                new Player("ypf791", new TestDeck()),
                new Player("CPU", new TestDeck())
            };
            Game game = new Game(player[0], player[1]);

            game.Start(player[0].ID);
            while (!game.IsEnd()) {
                game.NextTurn();
            }

            Console.ReadKey();
        }
    }
}
