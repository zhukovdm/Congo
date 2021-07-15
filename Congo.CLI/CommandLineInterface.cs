using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using Congo.Def;

namespace Congo.CLI {

	public sealed class CommandLineInterface : IUserInterface {

		readonly private static ImmutableDictionary<GameCode, ICommandLineConfiguration> configs =
			new Dictionary<GameCode, ICommandLineConfiguration>() {
				{ GameCode.Congo, CongoCommandLineConfiguration.GetInstance() }
			}.ToImmutableDictionary();
		
		private static readonly TextReader reader = Console.In;
		private static readonly TextWriter writer = Console.Out;

		public static IUserInterface GetParametrizedInstance(GameCode gameCode) =>
			new CommandLineInterface(gameCode);

		private readonly ICommandLineConfiguration config;

		private CommandLineInterface(GameCode gameCode) {
			if (!configs.TryGetValue(gameCode, out config)) {
				throw new Exception($"Game with code {gameCode} does not exist.");
			}
		}

		public void Greet() { writer.WriteLine(config.GreetView); }

		public void ShowBoard(IBoard board) {
			var upperBound = board.Size * board.Size;
			for (int square = 0; square < upperBound; square++) {
				if (square % board.Size == 0) writer.Write($" {board.Size - square / board.Size} ");
				var pv = config.PieceView[board.GetPieceCode(square)];
				if (board.IsFirstMovePiece(square)) pv = pv.ToUpper();
				writer.Write(pv);
				if (square % board.Size == board.Size - 1) writer.WriteLine();
			}
			writer.WriteLine();
			writer.Write(" / ");
			for (int i = 0; i < board.Size; i++) {
				writer.Write(" " + ((char)('a' + i)).ToString());
			}
			writer.WriteLine();
		}

		public IUserCommand GetUserCommand() {
			string[] line;
			IUserCommand command = null;
			char[] seps = new char[] { ' ', '\t', '\n' };

			do {
				writer.Write(" > ");
				line = reader.ReadLine().ToLower().Split(
					seps, StringSplitOptions.RemoveEmptyEntries);
				if (line.Length > 0) {
					switch (line[0]) {
						case "play": command = config.TryPlayCommand(line, this); break;
						case "help": command = config.TryHelpCommand(line, this); break;
						case "advise": command = config.TryAdviseCommand(line, this); break;
						case "move": command = config.TryMoveCommand(line, this); break;
						default:
							writer.WriteLine(" Entered command does not exist, try again.");
							break;
					}
				}
			} while (command == null);

			return command;
		}

		public void Report(string message) {
			writer.WriteLine(message);
		}
	}

}
