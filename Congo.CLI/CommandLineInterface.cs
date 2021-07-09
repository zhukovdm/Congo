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
			for (int rank = 0; rank < board.Size; rank++) {
				writer.Write($" {board.Size - rank} ");
				for (int file = 0; file < board.Size; file++) {
					var pv = config.PieceView[board.GetPieceCode(rank, file)];
					if (board.IsFirstMovePiece(rank, file)) pv = pv.ToUpper();
					writer.Write(pv);
				}
				writer.WriteLine();
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
