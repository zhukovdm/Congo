using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public abstract class CongoPiece {

		protected delegate ImmutableArray<int> LeapGetter(int position);

		protected List<CongoMove> getCapturingLeaps(List<CongoMove> moves,
			LeapGetter getter, ColorCode color, CongoBoard board, int position) {
			var capturingLeaps = getter.Invoke(position);
			foreach (var leap in capturingLeaps) {
				if (!board.IsSquareOccupied(leap) || board.IsOpponentPiece(color, leap)) {
					moves.Add(new CongoMove(position, leap));
				}
			}
			return moves;
		}

		protected List<CongoMove> getNonCapturingLeaps(List<CongoMove> moves,
			LeapGetter getter, CongoBoard board, int position) {
				var nonCapturingLeaps = getter.Invoke(position);
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
			return new List<CongoMove>();
		}
	}

	public sealed class Zebra : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();
			moves = getCapturingLeaps(moves, board.LeapsAsKnight, color, board, position);
			return moves;
		}

	}

	public sealed class Elephant : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();
			moves = getCapturingLeaps(moves, board.LeapsAsElephant, color, board, position);
			return moves;
		}
	}

	public sealed class Giraffe : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();
			moves = getCapturingLeaps(moves, board.LeapsAsCapturingGiraffe, color, board, position);
			moves = getNonCapturingLeaps(moves, board.LeapsAsKing, board, position);
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
