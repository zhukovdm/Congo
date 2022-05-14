using Congo.Core;
using System;

namespace Congo.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            CongoGame.Initialize();

            CongoGame game;
            CongoMove move = null;
            CongoCommandLine ui = null;
            var ap = new ArgumentParser();
            
            try {
                ap.Parse(args);

                ui = CongoCommandLine.SetCommandLine(args);
                game = ui.SetGame();

                while (!game.HasEnded()) {
                    do {

                        move = game.ActivePlayer.Accept(move);
                    } while (move == null);

                    game = game.Transition(move);
                    ui.ShowBoard(game);
                    game = ui.WaitResponse(game);
                }

                ui.ReportResult(game);
            }

            catch (ArgumentException ex) {
                Console.WriteLine(ex.Message);
            }

            finally {
                ui?.Dispose(); // network socket, database, etc.
            }
        }
    }
}
