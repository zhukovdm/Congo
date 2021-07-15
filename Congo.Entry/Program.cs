using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Core;
using Congo.Def;

namespace Congo.Entry {
	class Program {
		static void Main(string[] args) {

            IGame game;
            do {
                game = CongoGame.GetStandard(UICode.CommandLineInterface);
                while (game.InProgress()) {
                    game = game.MakeProgress();
                    game.Show();
                }
                game.ReportResult();
            } while (game.Repeat());

        }
	}
}
