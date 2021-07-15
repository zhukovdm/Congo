using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using Congo.Def;

namespace Congo.CLI {

	class CongoCommandLineConfiguration : ICommandLineConfiguration {

		private static readonly string addrPrefix = "Resources\\CLI\\Congo\\";

		private static readonly string greetView =
			File.ReadAllText(addrPrefix + "greet.txt");

		private static readonly ImmutableDictionary<PieceCode, string> pieceView =
			new Dictionary<PieceCode, string>() {
				{ PieceCode.Lion, " l" }, { PieceCode.Zebra, " z" },
				{ PieceCode.Elephant, " e" }, { PieceCode.Giraffe, " g" },
				{ PieceCode.Crocodile, " c" }, { PieceCode.Pawn, " p" },
				{ PieceCode.Superpawn, " s" }, { PieceCode.Monkey, " m" },
				{ PieceCode.Empty, " -" }, { PieceCode.Captured, " x" }
			}.ToImmutableDictionary();

		private static readonly CongoCommandLineConfiguration clv =
			new CongoCommandLineConfiguration();
		
		public static ICommandLineConfiguration GetInstance() => clv;

		private CongoCommandLineConfiguration() { }

		public string GreetView { get => greetView; }
		public ImmutableDictionary<PieceCode, string> PieceView { get => pieceView; }

		public IUserCommand TryPlayCommand(string[] input, IUserInterface ui) {
			List<string> types = new List<string> {
				"ai", "hi"
			};

			if (input.Length == 3) {
				for (int i = 1; i < 3; i++) {
					if (types.IndexOf(input[i]) < 0) {
						ui.Report($" Entered player type {input[i]} does not exist.");
						return null;
					}
				}
			}

			return new PlayUserCommand(input[1], input[2]);
		}

		public IUserCommand TryHelpCommand(string[] input, IUserInterface ui) {
			List<string> commands = new List<string> {
				"play", "help", "advise", "move"
			};

			if (input.Length == 2) {
				if (commands.IndexOf(input[1]) >= 0) {
					ui.Report(File.ReadAllText(addrPrefix + input[1] + ".txt"));
				} else {
					ui.Report(" Command is not supported.");
				}
			} else {
				ui.Report(" Wrong command format.");
			}

			return null;
		}

		public IUserCommand TryAdviseCommand(string[] input, IUserInterface ui) {
			
			if (input.Length == 2) {
				if (int.TryParse(input[1], out var heuristic)) {
					return new AdviseUserCommand(heuristic);
				}
			}

			ui.Report(" Selected heuristic does not exist.");
			return null;
		}

		public IUserCommand TryMoveCommand(string[] input, IUserInterface ui) {
			
			if (input.Length == 3) {
				var fr = SquareCodeExtensions.IndexOf(input[1]);
				var to = SquareCodeExtensions.IndexOf(input[2]);
				if (fr >= 0 && to >= 0) return new MoveUserCommand(fr, to);
			}

			ui.Report(" Entered move is not correct.");
			return null;
		}

	}

}
