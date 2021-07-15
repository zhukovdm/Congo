using System;
using System.Collections.Immutable;
using System.Linq;

using Congo.Def;
using Congo.UI;

namespace Congo.Core {

	public class CongoGame : IGame {

		private static CongoBoard setMixedRank(CongoBoard board, ColorCode color, int rank) {
			board = board.With(color, PieceCode.Giraffe, rank * board.Size + 0)
						 .With(color, PieceCode.Monkey, rank * board.Size + 1)
						 .With(color, PieceCode.Elephant, rank * board.Size + 2)
						 .With(color, PieceCode.Lion, rank * board.Size + 3)
						 .With(color, PieceCode.Elephant, rank * board.Size + 4)
						 .With(color, PieceCode.Crocodile, rank * board.Size + 5)
						 .With(color, PieceCode.Zebra, rank * board.Size + 6);
			return board;
		}

		private static CongoBoard setPawnRank(CongoBoard board, ColorCode color, int rank) {
			for (int file = 0; file < board.Size; file++) {
				board = board.With(color, PieceCode.Pawn, rank * board.Size + file);
			}
			return board;
		}

		public static IGame GetStandard(UICode uiCode) {
			var ui = UserInterfaceFactory.GetInstance(uiCode, GameCode.Congo);
			ui.Greet();
			var bo = CongoBoard.Empty;
			bo = setMixedRank(bo, ColorCode.Black, 0);
			bo = setPawnRank (bo, ColorCode.Black, 1);
			bo = setPawnRank (bo, ColorCode.White, 5);
			bo = setMixedRank(bo, ColorCode.White, 6);
			ui.ShowBoard(bo);
			var wp = new CongoHIPlayer(ColorCode.White, bo, ui);
			var bp = new CongoHIPlayer(ColorCode.Black, bo, ui);
			var pa = new CongoPlayer[] { wp, bp }.ToImmutableArray();
			return new CongoGame(bo, pa, ColorCode.White, ui);
		}

		public static IGame GetFromFEN(string fen) => throw new Exception();

		private readonly CongoBoard board;
		private readonly ImmutableArray<CongoPlayer> players;
		private readonly ColorCode player;
		private readonly IUserInterface userInterface;
		private ColorCode opponent => player.Opponent();

		private bool isValid(CongoMove move) {
			var comparer = new CongoMoveComparerGeneric();
			
			var query = from m in players[(int)player].Moves
						where comparer.Compare(m, move) == 0
						select m;
			foreach (var result in query) { return true; }

			return false;
		}

		private CongoGame(CongoBoard board, ImmutableArray<CongoPlayer> players,
			ColorCode player, IUserInterface userInterface) {
			this.board = board;
			this.players = players;
			this.player = player;
			this.userInterface = userInterface;
		}

		public void Show() => userInterface.ShowBoard(board);

		public bool InProgress() {
			return true;
		}

		public IGame MakeProgress() {
			CongoMove move;

			do {
				move = players[(int)player].DecideMove();
			} while (!isValid(move));

			return new CongoGame(
				board.With(player, board.GetPieceCode(move.Fr), move.To).Without(move.Fr),
				players, opponent, userInterface);
		}

		public void ReportResult() {
			// TODO
			userInterface.Report("The end!");
		}

		public bool Repeat() {
			// TODO
			return false;
		}

	}

}
