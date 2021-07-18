using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using Congo.Core;

namespace Congo.CLI
{
	public class CongoCommandLine : ICongoUserInterface
	{
		private static string readFile(string filename)
			=> File.ReadAllText(resourcesPrefix + filename + textFileExt);

		private static void reportWrongFormat(string command)
		{
			writer.WriteLine($"Wrong format. Consult help {command}.");
		}

		private delegate CongoUserCommand D(string[] line);
		private static readonly string resourcesPrefix = "Resources\\";
		private static readonly string textFileExt = ".txt";
		private static readonly TextReader reader = Console.In;
		private static readonly TextWriter writer = Console.Out;

		private static readonly ImmutableDictionary<PieceCode, string> pieceView =
			new Dictionary<PieceCode, string>() {
				{ PieceCode.Lion,      " l" }, { PieceCode.Zebra,    " z" },
				{ PieceCode.Elephant,  " e" }, { PieceCode.Giraffe,  " g" },
				{ PieceCode.Crocodile, " c" }, { PieceCode.Pawn,     " p" },
				{ PieceCode.Superpawn, " s" }, { PieceCode.Monkey,   " m" },
				{ PieceCode.Empty,     " -" }, { PieceCode.Captured, " x" }
			}.ToImmutableDictionary();

		private static readonly ImmutableDictionary<string, D> supportedCommands =
			new Dictionary<string, D>() {
				{ "advise", new D(tryParseAdviseCommand) },
				{ "exit"  , new D(tryParseExitCommand)   },
				{ "help"  , new D(tryParseHelpCommand)   },
				{ "move"  , new D(tryParseMoveCommand)   },
				{ "play"  , new D(tryParsePlayCommand)   },
				{ "show"  , new D(tryParseShowCommand)   }
			}.ToImmutableDictionary();

		private static readonly ImmutableDictionary<string, HeuristicCode> supportedHeuristics =
			new Dictionary<string, HeuristicCode> {
				{ "minimax",  HeuristicCode.MiniMax  },
				{ "negamax",  HeuristicCode.NegaMax  },
				{ "iterdeep", HeuristicCode.IterDeep }
			}.ToImmutableDictionary();

		private static readonly ImmutableDictionary<string, PlayerCode> supportedPlayers =
			new Dictionary<string, PlayerCode> {
				{ "ai", PlayerCode.AI },
				{ "hi", PlayerCode.HI }
			}.ToImmutableDictionary();

		private static CongoUserCommand tryParseAdviseCommand(string[] line)
		{
			if (line.Length != 2 || !supportedCommands.ContainsKey(line[0])) {
				reportWrongFormat(line[0]);
				return null;
			}

			return new AdviseCommand(supportedHeuristics[line[1]]);
		}

		private static CongoUserCommand tryParseExitCommand(string[] line)
		{
			if (line.Length != 1) {
				reportWrongFormat(line[0]);
				return null;
			}

			return new ExitCommand();
		}

		private static CongoUserCommand tryParseHelpCommand(string[] line)
		{
			if (line.Length != 2 || !supportedCommands.ContainsKey(line[1])) {
				reportWrongFormat(line[0]);
			} else {
				writer.Write(readFile(line[1]));
			}

			return null;
		}

		public static CongoUserCommand tryParseMoveCommand(string[] line)
		{
			if (line.Length == 3) {
				var fr = SquareCodeExtensions.GetValue(line[1]);
				var to = SquareCodeExtensions.GetValue(line[2]);
				if (fr >= 0 && to >= 0) return new MoveCommand(fr, to);
			}
			
			reportWrongFormat(line[0]);
			return null;
		}

		public static CongoUserCommand tryParsePlayCommand(string[] line)
		{
			if (line.Length == 3) {
				var whitePlayerCode = line[1].ToLower();
				var blackPlayerCode = line[2].ToLower();
				if (supportedPlayers.ContainsKey(whitePlayerCode) &&
					supportedPlayers.ContainsKey(blackPlayerCode)) {
					return new PlayCommand(supportedPlayers[whitePlayerCode],
										   supportedPlayers[blackPlayerCode]);
				}
			}

			reportWrongFormat(line[0]);
			return null;
		}

		public static CongoUserCommand tryParseShowCommand(string[] line)
		{
			if (line.Length != 1) {
				reportWrongFormat(line[0]);
				return null;
			}

			return new ShowCommand();
		}

		private bool isStartable(CongoUserCommand command)
		{
			return command is PlayCommand;
		}

		public CongoUserCommand ForceStart()
		{
			CongoUserCommand command;

			do {
				command = GetUserCommand();
			} while (!isStartable(command));

			return command;
		}

		public void Greet()
		{
			writer.WriteLine();
			writer.Write(readFile("greet"));
		}

		public void Show(CongoBoard board)
		{
			writer.WriteLine();
			var upperBound = board.Size * board.Size;

			for (int square = 0; square < upperBound; square++) {
				if (square % board.Size == 0) {
					writer.Write($" {board.Size - square / board.Size}  ");
				}

				var pv = pieceView[board.GetPieceCode(square)];
				if (board.IsFirstMovePiece(square)) pv = pv.ToUpper();
				writer.Write(pv);
				if (square % board.Size == board.Size - 1) writer.WriteLine();
			}

			writer.WriteLine();
			writer.Write(" /  ");
			for (int i = 0; i < board.Size; i++) {
				writer.Write(" " + ((char)('a' + i)).ToString());
			}
			writer.WriteLine();
		}

		public CongoUserCommand GetUserCommand()
		{
			string[] line;
			CongoUserCommand command = null;
			var separators = new char[] { ' ', '\t', '\n' };

			do {
				writer.WriteLine();
				writer.Write(" > ");
				line = reader.ReadLine().ToLower().Split(
					separators, StringSplitOptions.RemoveEmptyEntries);
				if (line.Length > 0 && supportedCommands.ContainsKey(line[0])) {
					command = supportedCommands[line[0]].Invoke(line);
				} else {
					writer.WriteLine(" Invalid input, consult help help.");
				}
			} while (command == null);

			return command;
		}

		public void ReportAdvice(CongoMove move, HeuristicCode heuristic)
		{
			writer.WriteLine(
				$"Move {move} is adviced by {heuristic}.");
		}

		public void ReportExit(string message)
		{
			writer.WriteLine(message);
		}
	}
}
