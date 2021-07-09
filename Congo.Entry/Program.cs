using Congo.Core;
using Congo.Def;

namespace Congo.Entry {
	class Program {
		static void Main(string[] args) {
			IGame game = CongoGame.GetStandard(UICode.CommandLineInterface);

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
