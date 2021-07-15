using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public abstract class CongoPiece {

		protected List<CongoMove> getValidCapturingLeaps(List<CongoMove> moves,
			ImmutableArray<int> capturingLeaps, ColorCode color, CongoBoard board, int square) {

			foreach (var leap in capturingLeaps) {
				if (!board.IsOccupied(leap) || board.IsOpponentPiece(color, leap)) {
					moves.Add(new CongoMove(square, leap));
				}
			}

			return moves;
		}

		protected List<CongoMove> getValidNonCapturingLeaps(List<CongoMove> moves,
			ImmutableArray<int> nonCapturingLeaps, CongoBoard board, int square) {

			foreach (var leap in nonCapturingLeaps) {
				if (!board.IsOccupied(leap)) {
					moves.Add(new CongoMove(square, leap));
				}
			}

			return moves;
		}

		public abstract List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square);

	}

	public sealed class Empty : CongoPiece {

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square)
			=> new List<CongoMove>();

	}

	public sealed class Elephant : CongoPiece {

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {

			return getValidCapturingLeaps(new List<CongoMove>(),
				board.LeapsAsElephant(square), color, board, square);
		}

	}

	public sealed class Zebra : CongoPiece {

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {

			return getValidCapturingLeaps(new List<CongoMove>(),
				board.LeapsAsKnight(square), color, board, square);
		}

	}

	public sealed class Giraffe : CongoPiece {

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {
			var moves = new List<CongoMove>();

			moves = getValidCapturingLeaps(
				moves, board.LeapsAsCapturingGiraffe(square), color, board, square);

			moves = getValidNonCapturingLeaps(
				moves, board.LeapsAsKing(square), board, square);

			return moves;
		}

	}

	public sealed class Crocodile : CongoPiece {

		private List<CongoMove> capturingWaterSlide(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int square, int direction) {
			var temp = square + direction;

			while (board.IsRiver(temp) && !board.IsOccupied(temp)) {
				moves.Add(new CongoMove(square, temp));
				temp += direction;
			}

			if (board.IsRiver(temp) && board.IsOpponentPiece(color, temp)) {
				moves.Add(new CongoMove(square, temp));
			}

			return moves;
		}

		private List<CongoMove> capturingGroundSlide(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int square) {

			var direction = board.IsAboveRiver(square) ? 1 : -1;
			var temp = square + direction * board.Size;

			while (!board.IsRiver(temp) && !board.IsOccupied(temp)) {
				moves.Add(new CongoMove(square, temp));
				temp += direction * board.Size;
			}

			if (board.IsOpponentPiece(color, temp)) {
				moves.Add(new CongoMove(square, temp));
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {
			var moves = new List<CongoMove>();

			moves = getValidCapturingLeaps(
				moves, board.LeapsAsCrocodile(square), color, board, square);

			if (board.IsRiver(square)) {
				moves = capturingWaterSlide(moves, color, board, square, -1);
				moves = capturingWaterSlide(moves, color, board, square,  1);
			} else {
				moves = capturingGroundSlide(moves, color, board, square);
			}

			return moves;
		}

	}

	public class Pawn : CongoPiece {

		protected List<CongoMove> nonCapturingVerticalSlide(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int square, bool s) {
			var slide = color.IsWhite() ? board.IsAboveRiver(square) : board.IsBelowRiver(square);

			if (slide || s) {
				var direct = color.IsWhite() ? 1 : -1;
				for (int steps = 1; steps < 3; steps++) {
					var newSquare = square + direct * steps * board.Size;
					if (!board.IsOccupied(newSquare) && board.WithinBoard(newSquare)) {
						moves.Add(new CongoMove(square, newSquare));
					} else {
						break;
					}
				}
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsPawn(color, square);
			moves = getValidCapturingLeaps(moves, leaps, color, board, square);

			moves = nonCapturingVerticalSlide(moves, color, board, square, false);

			return moves;
		}

	}

	public sealed class Superpawn : Pawn {

		private List<CongoMove> nonCapturingDiagonalSlide(List<CongoMove> moves,
			CongoBoard board, int square, int rdir, int fdir) {
			
			/* cumbersome, looking for a better solution */

			var rank = square / board.Size;
			var file = square % board.Size;

			for (int steps = 1; steps < 3; steps++) {
				var newRank = rank + steps * rdir;
				var newFile = file + steps * fdir;
				var newSquare = newRank * board.Size + newFile;
				if (board.WithinBoard(newRank, newFile) && !board.IsOccupied(newSquare)) {
					moves.Add(new CongoMove(square, newSquare));
				} else {
					break;
				}
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsSuperpawn(color, square);
			moves = getValidCapturingLeaps(moves, leaps, color, board, square);

			moves = nonCapturingVerticalSlide(moves, color, board, square, true);

			int rdir = color.IsWhite() ? 1 : -1;
			moves = nonCapturingDiagonalSlide(moves, board, square, rdir,  1);
			moves = nonCapturingDiagonalSlide(moves, board, square, rdir, -1);

			return moves;
		}

	}

	public sealed class Lion : CongoPiece {

		public List<CongoMove> verticalJump(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int square) {

			if (board.InsideCastle(color, square)) {
				var direction = color.IsBlack() ? 1 : -1;
				var newSquare = square;
				do {
					newSquare += direction * board.Size;
				} while (board.WithinBoard(newSquare) && !board.IsOccupied(newSquare));

				if (board.WithinBoard(newSquare) && !board.IsRiver(newSquare) &&
					(board.IsAboveRiver(square) != board.IsAboveRiver(newSquare)) && // different castles
					board.GetPiece(newSquare) is Lion) {
					moves.Add(new CongoMove(square, newSquare));
				}
			}

			return moves;
		}

		public List<CongoMove> diagonalJump(List<CongoMove> moves,
			ColorCode color, CongoBoard board, int square) {

			if (board.TryDiagonalJump(color, square, out int target)) {
				moves.Add(new CongoMove(square, target));
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsLion(color, square);
			moves = getValidCapturingLeaps(moves, leaps, color, board, square);

			moves = verticalJump(moves, color, board, square);

			moves = diagonalJump(moves, color, board, square);

			return moves;
		}

	}

	public sealed class Monkey : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square) {
			var moves = new List<CongoMove>();

			/* cumbersome, looking for a better solution */

			var leaps = board.LeapsAsKing(square);
			foreach (var leap in leaps) {
				if (!board.IsOccupied(leap)) moves.Add(new CongoMove(square, leap));
				else if (board.IsOpponentPiece(color, leap)) {
					var newRank = 2 * (leap / board.Size) - square / board.Size;
					var newFile = 2 * (leap % board.Size) - square % board.Size;
					var newSquare = newRank * board.Size + newFile;
					if (board.WithinBoard(newRank, newFile) && !board.IsOccupied(newSquare)) {
						moves.Add(new CongoMove(square, newSquare));
					}
				}
			}

			return moves;
		}
	}

	public sealed class Captured : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int square)
			=> new List<CongoMove>();
	}

}
