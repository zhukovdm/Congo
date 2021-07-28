using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
	public abstract class CongoPiece
	{
		private protected enum PieceId : uint
		{
			Ground, River, Elephant, Zebra, Giraffe,
			Crocodile, Pawn, Superpawn, Lion, Monkey,
			Captured
		}

		private protected abstract PieceId Id { get; }

		internal uint Code => (uint)Id;

		protected List<CongoMove> getValidCapturingLeaps(List<CongoMove> moves,
			ImmutableArray<int> capturingLeaps, CongoColor color, CongoBoard board, int square)
		{
			foreach (var leap in capturingLeaps) {
				if (!board.IsOccupied(leap) || board.IsOpponentPiece(color, leap)) {
					moves.Add(new CongoMove(square, leap));
				}
			}

			return moves;
		}

		protected List<CongoMove> getValidNonCapturingLeaps(List<CongoMove> moves,
			ImmutableArray<int> nonCapturingLeaps, CongoBoard board, int square)
		{
			foreach (var leap in nonCapturingLeaps) {
				if (!board.IsOccupied(leap)) {
					moves.Add(new CongoMove(square, leap));
				}
			}

			return moves;
		}

		public bool IsAnimal() => Id != PieceId.Ground && Id != PieceId.River;

		public bool IsCrocodile() => Id == PieceId.Crocodile;

		public bool IsPawn() => Id == PieceId.Pawn;

		public bool IsSuperpawn() => Id == PieceId.Superpawn;

		public bool IsLion() => Id == PieceId.Lion;

		public bool IsMonkey() => Id == PieceId.Monkey;

		public bool IsCaptured() => Id == PieceId.Captured;

		public abstract List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square);
	}

	public sealed class Ground : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Ground();

		private Ground() { }

		private protected override PieceId Id { get; } = PieceId.Ground;

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
			=> new List<CongoMove>();

		public override string ToString() => "-";
	}

	public sealed class River : CongoPiece
	{
		public static CongoPiece Piece { get; } = new River();

		private River() { }

		private protected override PieceId Id => PieceId.River;

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
			=> new List<CongoMove>();

		public override string ToString() => "+";
	}

	public sealed class Elephant : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Elephant();

		private Elephant() { }

		private protected override PieceId Id => PieceId.Elephant;

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
			=> getValidCapturingLeaps(new List<CongoMove>(), board.LeapsAsElephant(square),
				color, board, square);

		public override string ToString() => "e";
	}

	public sealed class Zebra : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Zebra();

		private Zebra() { }

		private protected override PieceId Id => PieceId.Zebra;

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
			=> getValidCapturingLeaps(new List<CongoMove>(), board.LeapsAsKnight(square),
				color, board, square);

		public override string ToString() => "z";
	}

	public sealed class Giraffe : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Giraffe();
		
		private Giraffe() { }

		private protected override PieceId Id => PieceId.Giraffe;

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
		{
			var moves = new List<CongoMove>();

			moves = getValidCapturingLeaps(
				moves, board.LeapsAsCapturingGiraffe(square), color, board, square);

			moves = getValidNonCapturingLeaps(
				moves, board.LeapsAsKing(square), board, square);

			return moves;
		}

		public override string ToString() => "g";
	}

	public sealed class Crocodile : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Crocodile();

		private Crocodile() { }

		private protected override PieceId Id => PieceId.Crocodile;

		private List<CongoMove> capturingRiverSlide(List<CongoMove> moves,
			CongoColor color, CongoBoard board, int square, int direction)
		{
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
			CongoColor color, CongoBoard board, int square)
		{
			var direction = board.IsAboveRiver(square) ? 1 : -1;
			var temp = square + direction * board.Size;

			while (!board.IsRiver(temp) && !board.IsOccupied(temp)) {
				moves.Add(new CongoMove(square, temp));
				temp += direction * board.Size;
			}

			if (!board.IsFriendlyPiece(color, temp)) {
				moves.Add(new CongoMove(square, temp));
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
		{
			var moves = new List<CongoMove>();

			moves = getValidCapturingLeaps(
				moves, board.LeapsAsCrocodile(square), color, board, square);

			if (board.IsRiver(square)) {
				moves = capturingRiverSlide(moves, color, board, square, -1);
				moves = capturingRiverSlide(moves, color, board, square,  1);
			} else {
				moves = capturingGroundSlide(moves, color, board, square);
			}

			return moves;
		}

		public override string ToString() => "c";
	}

	public class Pawn : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Pawn();

		protected Pawn() { }

		private protected override PieceId Id => PieceId.Pawn;

		protected List<CongoMove> nonCapturingVerticalSlide(List<CongoMove> moves,
			CongoColor color, CongoBoard board, int square, bool s)
		{
			var slide = color.IsWhite() ? board.IsAboveRiver(square) : board.IsBelowRiver(square);

			if (slide || s) {
				var direct = color.IsWhite() ? 1 : -1;
				for (int steps = 1; steps < 3; steps++) {
					var newSquare = square + direct * steps * board.Size;
					if (!board.IsOccupied(newSquare) && board.IsJungle(newSquare)) {
						moves.Add(new CongoMove(square, newSquare));
					} else {
						break;
					}
				}
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
		{
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsPawn(color, square);
			moves = getValidCapturingLeaps(moves, leaps, color, board, square);

			moves = nonCapturingVerticalSlide(moves, color, board, square, false);

			return moves;
		}

		public override string ToString() => "p";
	}

	public sealed class Superpawn : Pawn
	{
		public static new CongoPiece Piece { get; } = new Superpawn();

		private Superpawn() { }

		private protected override PieceId Id => PieceId.Superpawn;

		private List<CongoMove> nonCapturingDiagonalSlide(List<CongoMove> moves,
			CongoBoard board, int square, int rdir, int fdir)
		{
			var rank = square / board.Size;
			var file = square % board.Size;

			for (int steps = 1; steps < 3; steps++) {
				var newRank = rank + steps * rdir;
				var newFile = file + steps * fdir;
				var newSquare = newRank * board.Size + newFile;
				if (board.IsJungle(newRank, newFile) && !board.IsOccupied(newSquare)) {
					moves.Add(new CongoMove(square, newSquare));
				} else {
					break;
				}
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
		{
			var moves = new List<CongoMove>();
			
			var leaps = board.LeapsAsSuperpawn(color, square);
			moves = getValidCapturingLeaps(moves, leaps, color, board, square);

			moves = nonCapturingVerticalSlide(moves, color, board, square, true);

			int rdir = color.IsWhite() ? 1 : -1;
			moves = nonCapturingDiagonalSlide(moves, board, square, rdir,  1);
			moves = nonCapturingDiagonalSlide(moves, board, square, rdir, -1);

			return moves;
		}

		public override string ToString() => "s";
	}

	public sealed class Lion : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Lion();

		private Lion() { }

		private protected override PieceId Id => PieceId.Lion;

		private List<CongoMove> verticalJump(List<CongoMove> moves,
			CongoColor color, CongoBoard board, int square)
		{
			if (board.IsLionCastle(color, square)) {
				var direction = color.IsBlack() ? 1 : -1;
				var newSquare = square;
				do {
					newSquare += direction * board.Size;
				} while (board.IsJungle(newSquare) && !board.IsOccupied(newSquare));

				if (board.IsJungle(newSquare) && !board.IsRiver(newSquare) &&
					(board.IsAboveRiver(square) != board.IsAboveRiver(newSquare)) && // different castles
					board.GetPiece(newSquare) is Lion) {
					moves.Add(new CongoMove(square, newSquare));
				}
			}

			return moves;
		}

		private List<CongoMove> diagonalJump(List<CongoMove> moves,
			CongoColor color, CongoBoard board, int square)
		{
			if (board.TryDiagonalJump(color, square, out int target)) {
				moves.Add(new CongoMove(square, target));
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
		{
			var moves = new List<CongoMove>();

			var leaps = board.LeapsAsLion(color, square);
			moves = getValidCapturingLeaps(moves, leaps, color, board, square);

			moves = verticalJump(moves, color, board, square);

			moves = diagonalJump(moves, color, board, square);

			return moves;
		}

		public override string ToString() => "l";
	}

	public sealed class Monkey : CongoPiece
	{
		public static CongoPiece Piece { get; } = new Monkey();

		private Monkey() { }

		private protected override PieceId Id => PieceId.Monkey;

		private List<CongoMove> addMonkeyJump(List<CongoMove> moves,
			CongoBoard board, int square, int leap)
		{
			var newRank = 2 * (leap / board.Size) - square / board.Size;
			var newFile = 2 * (leap % board.Size) - square % board.Size;
			var newSquare = newRank * board.Size + newFile;
			if (board.IsJungle(newRank, newFile) && !board.IsOccupied(newSquare)) {
				moves.Add(new MonkeyJump(square, leap, newSquare));
			}

			return moves;
		}

		public List<CongoMove> ContinueJump(CongoColor color, CongoBoard board, int square)
		{
			var moves = new List<CongoMove>();

			moves.Add(new CongoMove(square, square));

			var leaps = board.LeapsAsKing(square);

			foreach (var leap in leaps) {
				if (board.IsOpponentPiece(color, leap) && !board.GetPiece(leap).IsCaptured()) {
					moves = addMonkeyJump(moves, board, square, leap);					
				} else {
					/* do nothing, only captures are allowed */
				}
			}

			return moves;
		}

		public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
		{
			var moves = new List<CongoMove>();
			var leaps = board.LeapsAsKing(square);

			foreach (var leap in leaps) {

				if (!board.IsOccupied(leap)) {
					moves.Add(new CongoMove(square, leap));
				} else if (board.IsOpponentPiece(color, leap)) {
					moves = addMonkeyJump(moves, board, square, leap);
				} else {
					/* do nothing, occupied by friendly piece */
				}
			}

			return moves;
		}

		public override string ToString() => "m";
	}
}
