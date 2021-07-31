using Congo.Core;

namespace Congo.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			CongoGame.Initialize();

			CongoGame game;
			CongoMove move;
			CongoCommandLine ui = null;
			
			try {
				ui = CongoCommandLine.SetCommandLine();
				game = ui.SetGame();
				while (!game.HasEnded()) {
					move = game.ActivePlayer.GetValidMove(ui, game);
					game = game.Transition(move);
					ui.ShowBoard(game);
					game = ui.WaitResponse(game);
				}
				ui.ReportResult(game);
			} catch (ExitCommandException) {

			} finally {
				ui?.Dispose(); // network socket, database, etc.
			}
		}
	}
}
