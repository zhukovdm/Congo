using Congo.Core;

namespace Congo.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			CongoGame game;
			CongoMove move;
			CongoCommandLine ui = null;
			
			try {
				ui = CongoCommandLine.SetCommandLine();
				game = ui.SetGame();
				while (!game.HasEnded()) {
					ui.ShowBoard(game);
					move = game.ActivePlayer.GetValidMove(ui, game);
					game = game.Transition(move);
					game = ui.WaitResponse(game);
				}
				ui.ReportResult(game);
			} catch (ExitCommandException) {

			} finally {
				ui?.Dispose(); /* network socket, database, etc. */
			}
		}
	}
}
