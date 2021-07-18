using System;
using System.Linq;

namespace Congo.Core
{
	public class CongoGame
	{
		private static CongoBoard setMixedRank(CongoBoard board, ColorCode color, int rank)
		{
			board = board.With(color, PieceCode.Giraffe,   rank * board.Size + 0)
						 .With(color, PieceCode.Monkey,	   rank * board.Size + 1)
						 .With(color, PieceCode.Elephant,  rank * board.Size + 2)
						 .With(color, PieceCode.Lion,      rank * board.Size + 3)
						 .With(color, PieceCode.Elephant,  rank * board.Size + 4)
						 .With(color, PieceCode.Crocodile, rank * board.Size + 5)
						 .With(color, PieceCode.Zebra,     rank * board.Size + 6);
			
			return board;
		}

		private static CongoBoard setPawnRank(CongoBoard board, ColorCode color, int rank)
		{
			for (int file = 0; file < board.Size; file++) {
				board = board.With(color, PieceCode.Pawn, rank * board.Size + file);
			}

			return board;
		}

		public static CongoGame New(ICongoUserInterface ui)
		{
			var b = CongoBoard.Empty;
			b = setMixedRank(b, ColorCode.Black, 0);
			b = setPawnRank (b, ColorCode.Black, 1);
			b = setPawnRank (b, ColorCode.White, 5);
			b = setMixedRank(b, ColorCode.White, 6);

			ui.Greet();
			var command = ui.ForceStart();

			var wp = CongoPlayerFactory.GetInstance(
				ColorCode.White, b, (PlayCommand)command);

			var bp = CongoPlayerFactory.GetInstance(
				ColorCode.Black, b, (PlayCommand)command);

			return new CongoGame(null, b, ColorCode.White, wp, bp, ui);
		}

		public static CongoGame GetFromFEN(string fen)
			=> throw new Exception();

		private readonly CongoGame previousGame;
		private readonly CongoBoard board;
		private readonly ColorCode activePlayer;
		private readonly CongoPlayer whitePlayer;
		private readonly CongoPlayer blackPlayer;
		private readonly ICongoUserInterface userInterface;

		public CongoPlayer ActivePlayer
			=> activePlayer.IsWhite() ? whitePlayer : blackPlayer;

		private CongoPlayer Opponent
			=> activePlayer.IsWhite() ? blackPlayer : whitePlayer;

		private bool isValid(CongoMove candidateMove)
		{
			var query = from validMove in ActivePlayer.Moves
						where CongoMove.AreEqual(candidateMove, validMove)
						select validMove;

			foreach (var move in query) return true;

			return false;
		}

		private CongoMove GetUserMove()
		{
			CongoMove move = null;

			do {
				var command = userInterface.GetUserCommand();
				if (command is AdviseCommand) {
					var advice = (AdviseCommand)command;
					move = Algorithm.GetMove(this, advice.Heuristic);
					userInterface.ReportAdvice(move, advice.Heuristic);
					move = null;
				} else if (command is ExitCommand) {
					throw new Exception(" The game has suddenly ended.");
				} else if (command is MoveCommand) {
					var unpack = (MoveCommand)command;
					move = new CongoMove(unpack.Fr, unpack.To);
					if (!isValid(move)) move = null;
				} else if (command is ShowCommand) {
					userInterface.Show(board);
				} else {
					/* do nothing */
				}
			} while (move == null);

			return move;
		}

		private CongoGame(CongoGame previousGame, CongoBoard board,
			ColorCode activePlayer, CongoPlayer whitePlayer,
			CongoPlayer blackPlayer, ICongoUserInterface userInterface)
		{
			this.previousGame = previousGame;
			this.board = board;
			this.whitePlayer = whitePlayer;
			this.blackPlayer = blackPlayer;
			this.activePlayer = activePlayer;
			this.userInterface = userInterface;
		}

		public bool HasEnded()
		{
			return false;
		}

		public CongoGame Proceed()
		{
			userInterface.Show(board);

			CongoMove move;
			if (ActivePlayer.Code.IsHI()) {
				move = GetUserMove();
			} else { // some heuristic
				move = Algorithm.GetMove(this, HeuristicCode.Random);
			}

			var newBoard = board.With(activePlayer, board.GetPieceCode(move.Fr), move.To)
								.Without(move.Fr);

			var newWhitePlayer = whitePlayer.With(ColorCode.White, newBoard);
			var newBlackPlayer = blackPlayer.With(ColorCode.Black, newBoard);

			return new CongoGame(this, newBoard, activePlayer.Invert(),
				newWhitePlayer, newBlackPlayer, userInterface);
		}

		public void ReportResult()
		{
			// TODO
			throw new Exception();
		}
	}
}
