using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public abstract class CongoPlayer {

		protected readonly CongoBoard board;
		protected readonly IUserInterface ui;
		protected readonly ImmutableArray<CongoMove> moves;
		
		public ImmutableArray<CongoMove> Moves => moves;

		public CongoPlayer(ColorCode color, CongoBoard board, IUserInterface ui) {
			this.board = board;
			this.ui = ui;
			moves = GenerateMoves(color, board);
		}

		protected static ImmutableArray<CongoMove> GenerateMoves(ColorCode color, CongoBoard board) {
			var allMoves = new List<CongoMove>();
			var e = board.GetEnumerator(color);
			while (e.MoveNext()) {
				var piece = board.GetPiece(e.Current);
				allMoves.AddRange(piece.GetMoves(color, board, e.Current));
			}
			return allMoves.ToImmutableArray();
		}

		public abstract CongoMove DecideMove();

	}

	sealed class CongoAIPlayer : CongoPlayer {
		
		public CongoAIPlayer(ColorCode color, CongoBoard board, IUserInterface ui)
			: base(color, board, ui) { }

		public override CongoMove DecideMove() {
			return moves.Length > 0 ? moves[0] : new CongoMove(-1, -1);
		}

	}

	sealed class CongoHIPlayer : CongoPlayer {
		
		public CongoHIPlayer(ColorCode color, CongoBoard board, IUserInterface ui)
			: base(color, board, ui) { }

		public override CongoMove DecideMove() {
			IUserCommand command;

			do {
				command = ui.GetUserCommand();
				if (command is AdviseUserCommand) {
					var cmd1 = (AdviseUserCommand) command;
					ui.Report($" Heuristic #{cmd1.Heuristic}...");
				}
			} while (!(command is MoveUserCommand));

			var cmd2 = (MoveUserCommand) command;
			return new CongoMove(cmd2.Fr, cmd2.To);
		}

	}

}
