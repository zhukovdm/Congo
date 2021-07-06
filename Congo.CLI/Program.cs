using Congo.Core;

namespace Congo.CLI {
	class Program {
		static void Main(string[] args) {
			var game = new Game(
				new CommandLineInterface(),
				Board.CreateStandard()
			);
			game.Play();
		}
	}
}
