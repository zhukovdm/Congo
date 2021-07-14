using System.Collections.Immutable;

using Congo.Core;
using Congo.Def;

namespace Congo.Entry {
	class Program {
		static void Main(string[] args) {
			IGame game = CongoGame.GetStandard(UICode.CommandLineInterface);
			var board = CongoBoard.Empty;
			var leaps = board.LeapsAsCrocodile((int)SquareCode.D4);
			/*/
			bool last;
			do {
				IGame game = CongoGame.GetStandard(UIType.CommandLineInterface);
				game.Show();
				while (game.InProgress()) {
					game = game.MakeProgress();
					game.Show();
				}
				last = game.IsLast();
			} while (!last);
			/**/
		}
	}
}
