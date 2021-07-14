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

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position)
			=> new List<CongoMove>();

	}

	public sealed class Elephant : CongoPiece {

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return getValidCapturingLeaps(new List<CongoMove>(),
				board.LeapsAsElephant(position), color, board, position);
		}

	}

	public sealed class Zebra : CongoPiece {

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			return getValidCapturingLeaps(new List<CongoMove>(),
				board.LeapsAsKnight(position), color, board, position);
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

		private List<CongoMove> capturingWaterSlide(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int position, int direction) {
			var temp = position + direction;
			while (board.IsSquareWater(temp) && !board.IsSquareOccupied(temp)) {
				moves.Add(new CongoMove(position, temp));
				temp += direction;
			}
			if (board.IsSquareWater(temp) && board.IsOpponentPiece(color, temp)) {
				moves.Add(new CongoMove(position, temp));
			}
			return moves;
		}

		private List<CongoMove> capturingGroundSlide(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int position) {
			var direction = board.IsUpperPart(position) ? 1 : -1;
			var temp = position + direction * board.Size;
			while (!board.IsSquareWater(temp) && !board.IsSquareOccupied(temp)) {
				moves.Add(new CongoMove(position, temp));
				temp += direction * board.Size;
			}
			if (board.IsOpponentPiece(color, temp)) {
				moves.Add(new CongoMove(position, temp));
			}
			return moves;
		}

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();

			moves = getValidCapturingLeaps(
				moves, board.LeapsAsCrocodile(position), color, board, position);

			if (board.IsSquareWater(position)) {
				moves = capturingWaterSlide(moves, color, board, position, -1);
				moves = capturingWaterSlide(moves, color, board, position,  1);
			} else {
				moves = capturingGroundSlide(moves, color, board, position);
			}

			return moves;
		}

	}

	public sealed class Lion : CongoPiece {

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsLion(color, position);
			moves = getValidCapturingLeaps(moves, leaps, color, board, position);

			// TODO jump to the opponent's lion if seen

			return moves;
		}

	}

	public class Pawn : CongoPiece {

		protected List<CongoMove> nonCapturingVerticalSlide(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int position, bool s) {
			var slide = color.IsWhite() ? board.IsUpperPart(position) : board.IsLowerPart(position);

			if (slide || s) {
				var direct = color.IsWhite() ? 1 : -1;
				var steps = 1;
				while (steps < 3) {
					var newPosition = position + direct * steps * board.Size;
					if (!board.IsSquareOccupied(newPosition) && board.WithinBoard(newPosition)) {
						moves.Add(new CongoMove(position, newPosition));
					}
					else {
						break;
					}
					steps++;
				}

			}

			return moves;
		}

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsPawn(color, position);
			moves = getValidCapturingLeaps(moves, leaps, color, board, position);

			moves = nonCapturingVerticalSlide(moves, color, board, position, false);

			return moves;
		}

	}

	public sealed class Superpawn : Pawn {

		private List<CongoMove> nonCapturingDiagonalSlides(List<CongoMove> moves) {

			// TODO

			return moves;
		}

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsSuperpawn(color, position);
			moves = getValidCapturingLeaps(moves, leaps, color, board, position);

			moves = nonCapturingVerticalSlide(moves, color, board, position, true);
			moves = nonCapturingDiagonalSlides(moves);

			return moves;
		}

	}

	public sealed class Monkey : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position) {
			var moves = new List<CongoMove>();

			// TODO jumps

			return moves;
		}
	}

	public sealed class Captured : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int position)
			=> new List<CongoMove>();
	}

}
