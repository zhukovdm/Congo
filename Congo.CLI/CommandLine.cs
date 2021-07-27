using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using Congo.Core;

namespace Congo.CLI
{
	class ExitCommandException : Exception { }

	public abstract class CongoCommandLine : ICongoUserInterface, IDisposable
	{
		private static readonly string resourcesPrefix = "Resources\\";
		private static readonly string textFileExt = ".txt";
		protected static readonly char[] separators = new char[] { ' ', '\t', '\n' };
		protected static readonly TextReader reader = Console.In;
		protected static readonly TextWriter writer = Console.Out;

		private delegate string[] VerifyCommandDelegate(string[] input);

		private static readonly ImmutableDictionary<string, VerifyCommandDelegate> supportedCommands =
			new Dictionary<string, VerifyCommandDelegate>() {
				{ "advise",  verifyAdviseCommand  },
				{ "allow",   verifyAllowCommand   },
				{ "connect", verifyConnectCommand },
				{ "exit",    verifyExitCommand    },
				{ "game",    verifyGameCommand    },
				{ "help",    verifyHelpCommand    },
				{ "move",    verifyMoveCommand    },
				{ "play",    verifyPlayCommand    },
				{ "show",    verifyShowCommand    }
			}.ToImmutableDictionary();

		private delegate CongoMove AlgorithmDelegate(CongoGame game);

		private static readonly ImmutableDictionary<string, AlgorithmDelegate> supportedAlgorithms =
			new Dictionary<string, AlgorithmDelegate> {
				{ "rnd",      Algorithm.Rnd      },
				{ "minimax",  Algorithm.MiniMax  },
				{ "negamax",  Algorithm.NegaMax  },
				{ "iterdeep", Algorithm.IterDeep }
			}.ToImmutableDictionary();

		private static readonly ImmutableList<string> squareView =
			new List<string> {
				"a7", "b7", "c7", "d7", "e7", "f7", "g7",
				"a6", "b6", "c6", "d6", "e6", "f6", "g6",
				"a5", "b5", "c5", "d5", "e5", "f5", "g5",
				"a4", "b4", "c4", "d4", "e4", "f4", "g4",
				"a3", "b3", "c3", "d3", "e3", "f3", "g3",
				"a2", "b2", "c2", "d2", "e2", "f2", "g2",
				"a1", "b1", "c1", "d1", "e1", "f1", "g1"
			}.ToImmutableList();

		private static readonly ImmutableList<Type> pieceTypes =
			new Type[] {
				typeof(Ground), typeof(River), typeof(Elephant), typeof(Zebra),
				typeof(Giraffe), typeof(Crocodile), typeof(Pawn), typeof(Superpawn),
				typeof(Lion), typeof(Monkey), typeof(Captured)
			}.ToImmutableList();

		// Char piece views are completely separated from pieces intentionally.
		private static readonly ImmutableDictionary<Type, string> pieceView =
			new Dictionary<Type, string>() {
				{ typeof(Ground),   "-" }, { typeof(River),     "+" },
				{ typeof(Elephant), "e" }, { typeof(Zebra),     "z" },
				{ typeof(Giraffe),  "g" }, { typeof(Crocodile), "c" },
				{ typeof(Pawn),     "p" }, { typeof(Superpawn), "s" },
				{ typeof(Lion),     "l" }, { typeof(Monkey),    "m" },
				{ typeof(Captured), "x" }
			}.ToImmutableDictionary();

		private delegate void ShowDelegate(CongoGame game);

		private static readonly ImmutableDictionary<string, ShowDelegate> supportedShows =
			new Dictionary<string, ShowDelegate> {
				{ "board", showBoard }, { "players", showPlayers },
				{ "moves", showMoves }, { "game",    showGame    }
			}.ToImmutableDictionary();

		private static string getMoveRepr(CongoMove move)
			=> "(" + squareView[move.Fr] + "," + squareView[move.To] + ")";

		private static string readTextFile(string filename)
		{
			try {
				return File.ReadAllText(resourcesPrefix + filename + textFileExt);
			} catch (Exception) /* any exception */ {
				return null;
			}
		}
		
		protected static void reportAllowedCommands(List<string> allowedCommands)
		{
			writer.WriteLine($" Allowed commands are {string.Join(", ", allowedCommands.ToArray())}.");
		}

		private static void reportHelpFile(string helpFile)
		{
			writer.Write(helpFile);
		}

		private static void reportEmptyCommand()
		{
			writer.WriteLine(" Input is empty. Consult \"allow\".");
		}

		private static void reportNotSupportedCommand(string command)
		{
			writer.WriteLine($" Command {command} is not supported. Consult \"help help\".");
		}

		private static void reportWrongCommandFormat(string command)
		{
			writer.WriteLine($" Wrong command format. Consult \"help {command}\".");
		}

		private static void reportNotAllowedCommand(string command)
		{
			writer.WriteLine($" Command {command} is not allowed at the moment.");
		}

		private static void reportAdvisedMove(CongoMove move, string algorithm)
		{
			writer.WriteLine($" Algorithm {algorithm} advises move {getMoveRepr(move)}.");
		}

		private static int[] countPieces(CongoBoard board, CongoColor color)
		{
			var counter = new int[pieceTypes.Count];
			var enumerator = board.GetEnumerator(color);

			while (enumerator.MoveNext()) {
				var type = board.GetPiece(enumerator.Current).GetType();
				counter[pieceTypes.IndexOf(type)]++;
			}

			return counter;
		}

		private static void showTransition(CongoGame game)
		{
			writer.WriteLine();
			writer.WriteLine($" transition {getMoveRepr(game.TransitionMove)}");
		}

		protected static void showBoard(CongoGame game)
		{
			writer.WriteLine();
			var upperBound = game.Board.Size * game.Board.Size;

			for (int square = 0; square < upperBound; square++) {
				if (square % game.Board.Size == 0) {
					writer.Write($" {game.Board.Size - square / game.Board.Size}  ");
				}

				var pv = pieceView[game.Board.GetPiece(square).GetType()];
				if (game.Board.IsFirstMovePiece(square)) pv = pv.ToUpper();
				writer.Write(" " + pv);
				if (square % game.Board.Size == game.Board.Size - 1) writer.WriteLine();
			}

			writer.WriteLine();
			writer.Write(" /  ");
			for (int i = 0; i < game.Board.Size; i++) {
				writer.Write(" " + ((char)('a' + i)).ToString());
			}
			writer.WriteLine();
		}

		private static void showPlayer(CongoBoard board, CongoColor color, CongoPlayer activePlayer)
		{
			var activeRepr = color.Equals(activePlayer.Color) ? "*" : " ";
			var colorRepr  = color.IsWhite() ? "white" : "black";
			var counter    = countPieces(board, color);

			writer.Write($" {activeRepr} {colorRepr}");
			for (int i = 2; i < pieceTypes.Count; i++) {
				var pieceRepr = color.IsWhite()
					? pieceView[pieceTypes[i]].ToUpper()
					: pieceView[pieceTypes[i]].ToLower();
				writer.Write($" {counter[i]}{pieceRepr}");
			}
			writer.WriteLine();
		}

		private static void showPlayers(CongoGame game)
		{
			writer.WriteLine();
			showPlayer(game.Board, White.Color, game.ActivePlayer);
			showPlayer(game.Board, Black.Color, game.ActivePlayer);
		}

		private static void showMoves(CongoGame game)
		{
			writer.WriteLine();
			int cnt = 0;
			foreach (var move in game.ActivePlayer.Moves) {
				var repr = " " + getMoveRepr(move);
				if (cnt + repr.Length > 40) {
					cnt = 0;
					writer.WriteLine();
				}
				cnt += repr.Length;
				writer.Write(repr);
			}
			writer.WriteLine();
		}

		private static void showGame(CongoGame game)
		{
			showBoard(game);
			showPlayers(game);
			showMoves(game);
		}

		private static string[] verifyCommand(Func<string[], bool> predicate, string[] input)
		{
			if (predicate(input)) {
				reportWrongCommandFormat(input[0]);
				input = null;
			}
			
			return input;
		}

		private static string[] verifyAdviseCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr)
				=> arr.Length != 2 || !supportedAlgorithms.ContainsKey(arr[1]);

			return verifyCommand(predicate, input);
		}

		private static string[] verifyAllowCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr) => arr.Length != 1;

			return verifyCommand(predicate, input);
		}

		private static string[] verifyConnectCommand(string[] input)
		{
			throw new NotImplementedException();
		}
		
		private static string[] verifyExitCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr) => arr.Length != 1;

			return verifyCommand(predicate, input);
		}

		private static string[] verifyGameCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr) =>
			{
				return arr.Length != 2 || !(arr[1] == "standard" ||
					CongoGame.FromFen(arr[1]) != null ||
					CongoGame.FromFen(readTextFile(arr[1])) != null);
			};

			return verifyCommand(predicate, input);
		}

		private static string[] verifyHelpCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr)
				=> arr.Length != 2 || !supportedCommands.ContainsKey(arr[1]);

			return verifyCommand(predicate, input);
		}

		private static string[] verifyMoveCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr)
				=> arr.Length != 3 || squareView.IndexOf(arr[1]) < 0 || squareView.IndexOf(arr[1]) < 0;

			return verifyCommand(predicate, input);
		}

		private static string[] verifyPlayCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr)
				=> arr.Length != 2 || !(arr[1] == "local" || arr[1] == "network");

			return verifyCommand(predicate, input);
		}

		private static string[] verifyShowCommand(string[] input)
		{
			Func<string[], bool> predicate = (string[] arr)
				=> arr.Length != 2 || !supportedShows.ContainsKey(arr[1]);

			return verifyCommand(predicate, input);
		}

		private static void Greet()
		{
			writer.WriteLine();
			writer.Write(readTextFile("greet"));
		}

		protected static string[] getUserCommand(List<string> allowedCommands)
		{
			string[] input;
			string[] command = null;

			do {
				writer.WriteLine();
				writer.Write(" > ");
				input = reader.ReadLine().ToLower().Split(
					separators, StringSplitOptions.RemoveEmptyEntries);

				if (input.Length > 0 && supportedCommands.ContainsKey(input[0])) {
					if (allowedCommands.IndexOf(input[0]) >= 0) {
						command = supportedCommands[input[0]].Invoke(input);
					} else {
						reportNotAllowedCommand(input[0]);
						command = null;
					}					
				} else if (input.Length == 0) {
					reportEmptyCommand();
				} else {
					reportNotSupportedCommand(input[0]);
				}
			} while (command == null);

			return command;
		}

		protected static Type getPlayerType(CongoColor color)
		{
			string type;
			var playerColor = color.IsWhite() ? "white" : "black";
			var allowedTypes = new Dictionary<string, Type> {
				{ "ai", typeof(Ai) }, { "hi", typeof(Hi) }
			};

			do {
				writer.WriteLine();
				writer.Write($" Set {playerColor} player type (ai|hi) ");
				type = reader.ReadLine().ToLower();

				if (!allowedTypes.ContainsKey(type)) {
					writer.WriteLine($" The type {type} is not allowed. Try again.");
				}
			} while (!allowedTypes.ContainsKey(type));

			return allowedTypes[type];
		}

		public void ShowBoard(CongoGame game)
		{
			showTransition(game);
			showBoard(game);
		}

		/* decides local or network command line */
		public static CongoCommandLine SetCommandLine()
		{
			Greet();
			var allowedCommands = new List<string>() {
				"allow", "exit", "help", "play"
			};

			CongoCommandLine cli = null;

			do {
				var command = getUserCommand(allowedCommands);

				switch (command[0]) {

					case "allow":
						reportAllowedCommands(allowedCommands);
						break;

					case "exit":
						throw new ExitCommandException();

					case "help":
						reportHelpFile(readTextFile(command[1]));
						break;

					case "play":
						if (command[1] == "local") cli = new LocalCommandLine();
						else cli = new NetworkCommandLine();
						break;

					default:
						throw new InvalidOperationException();
				}
			} while (cli == null);

			return cli;
		}

		public CongoMove GetHiMove(CongoGame game)
		{
			CongoMove move = null;
			var allowedCommands = new List<string>() {
				"advise", "allow", "exit", "help", "move", "show"
			};

			do {
				var command = getUserCommand(allowedCommands);

				switch (command[0]) {

					case "advise":
						move = supportedAlgorithms[command[1]].Invoke(game);
						reportAdvisedMove(move, command[1]);
						move = null;
						break;

					case "allow":
						reportAllowedCommands(allowedCommands);
						break;

					case "exit":
						throw new ExitCommandException();

					case "help":
						reportHelpFile(readTextFile(command[1]));
						break;

					case "move":
						move = new CongoMove(squareView.IndexOf(command[1]),
											 squareView.IndexOf(command[2]));
						break;

					case "show":
						supportedShows[command[1]].Invoke(game);
						break;

					default:
						throw new InvalidOperationException();
				}

			} while (move == null);

			return move;
		}

		public void ReportWrongHiMove()
		{
			writer.WriteLine(" Entered move is wrong. Consult \"show moves\".");
		}

		public abstract CongoGame SetGame();

		public abstract CongoGame WaitResponse(CongoGame game);

		public void ReportResult(CongoGame game)
		{
			writer.WriteLine();
			var winner = game.ActivePlayer.Color.Invert().IsWhite() ? "white" : "black";
			writer.WriteLine($" {winner} wins.");
			writer.WriteLine();
		}

		public abstract void Dispose();
	}

	public class LocalCommandLine : CongoCommandLine
	{
		public override CongoGame SetGame()
		{
			string[] command;
			var allowedCommands = new List<string>() {
				"allow", "game"
			};

			do {
				command = getUserCommand(allowedCommands);

				switch (command[0]) {

					case "allow":
						reportAllowedCommands(allowedCommands);
						break;

					case "game":
						break;

					default:
						throw new NotImplementedException();
				}
			} while (command[0] != "game");

			var wp = getPlayerType(White.Color);
			var bp = getPlayerType(Black.Color);

			CongoGame game;

			if (command[1] == "standard") {
				game = CongoGame.Standard(wp, bp);
			} else if (CongoGame.FromFen(command[1]) != null) {
				throw new NotImplementedException(); // TODO
			} else {
				throw new NotImplementedException(); // TODO
			}

			showBoard(game);
			return game;
		}

		public override CongoGame WaitResponse(CongoGame game) => game;

		public override void Dispose() { } // TODO
	}

	public class NetworkCommandLine : CongoCommandLine
	{
		public override CongoGame SetGame()
		{
			throw new NotImplementedException(); // TODO
		}

		public override CongoGame WaitResponse(CongoGame game)
		{
			throw new NotImplementedException(); // TODO
		}

		public override void Dispose()
		{
			throw new NotImplementedException(); // TODO
		}
	}
}
