using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public abstract class CongoPiece {

		protected List<CongoMove> getValidCapturingLeaps(List<CongoMove> moves,
			ImmutableArray<int> capturingLeaps, ColorCode color, CongoBoard board, int position) {
			foreach (var leap in capturingLeaps) {
				if (!board.IsSquareOccupied(leap) || board.IsOpponentPiece(color, leap)) {
					moves.Add(new CongoMove(position, leap));
				}
			}
			return moves;
		}

		protected List<CongoMove> getValidNonCapturingLeaps(List<CongoMove> moves,
			ImmutableArray<int> nonCapturingLeaps, CongoBoard board, int position) {
				foreach (var leap in nonCapturingLeaps) {
					if (!board.IsSquareOccupied(leap)) {
						moves.Add(new CongoMove(position, leap));
					}
				}
				return moves;
			}

		public abstract List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position);

	}

	public sealed class Empty : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return new List<CongoMove>();
		}
	}

	public sealed class Lion : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsLion(color, position);
			moves = getValidCapturingLeaps(moves, leaps, color, board, position);

			// TODO jump to the opponent's lion

			return moves;
		}
	}

	public sealed class Zebra : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return getValidCapturingLeaps(new List<CongoMove>(),
				board.LeapsAsKnight(position), color, board, position);
		}

	}

	public sealed class Elephant : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return getValidCapturingLeaps(new List<CongoMove>(),
				board.LeapsAsElephant(position), color, board, position);
		}
	}

	public sealed class Giraffe : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();
			
			moves = getValidCapturingLeaps(
				moves, board.LeapsAsCapturingGiraffe(position), color, board, position);

			moves = getValidNonCapturingLeaps(
				moves, board.LeapsAsKing(position), board, position);

			return moves;
		}
	}

	public sealed class Crocodile : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return new List<CongoMove>();
		}
	}

	public sealed class Pawn : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return new List<CongoMove>();
		}
	}

	public sealed class Superpawn : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return new List<CongoMove>();
		}
	}

	public sealed class Monkey : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return new List<CongoMove>();
		}
	}

	public sealed class Captured : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return new List<CongoMove>();
		}
	}

}
