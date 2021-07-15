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
		
		readonly private static TextReader reader = Console.In;
		readonly private static TextWriter writer = Console.Out;

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
				writer.Write(" " + ((char)('A' + i)).ToString());
			}
			writer.WriteLine();
		}

	}

}
