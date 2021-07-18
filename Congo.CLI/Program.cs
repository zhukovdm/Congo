using System;

using Congo.Core;

namespace Congo.CLI
{
	class Program
    {
        static void Main(string[] args)
        {
            CongoCommandLine ui = null;
            CongoGame game;
            try {
                ui = new CongoCommandLine();
                game = CongoGame.New(ui);
                while (!game.HasEnded()) {
                    game = game.Proceed();
                }
                game.ReportResult();
            } catch (Exception e) {
                ui?.ReportExit(e.Message);
			}
        }
    }
}
