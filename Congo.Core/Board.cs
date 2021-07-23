using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
	public interface IParametrizedEnumerator<T>
	{
		T Current { get; }
		bool MoveNext();
	}

	public interface IParametrizedEnumerable<T, U>
	{
		IParametrizedEnumerator<U> GetEnumerator(T param);
	}

	public sealed class CongoBoard : IParametrizedEnumerable<CongoColor, int>
	{
		private delegate ImmutableArray<int> LeapGenerator(int rank, int file);
		
		private delegate ImmutableArray<int> ColoredLeapGenerator(
			CongoColor color, int rank, int file);

		private static int size = 7;

		private static bool isJungle(int rank, int file)
			=> rank >= 0 && rank < size && file >= 0 && file < size;

		private static bool isJungle(int square)
			=> square >= 0 && square < size * size;

		private static void addLeap(List<int> leaps, int rank, int file)
		{
			if (isJungle(rank, file)) leaps.Add(rank * size + file);
		}

		private static ImmutableArray<int> circleLeapGenerator(
			Func<int, int, bool> predicate, int rank, int file, int radius)
		{
			var leaps = new List<int>();

			for (int i = -radius; i < radius + 1; i++) {
				for (int j = -radius; j < radius + 1; j++) {
					if (predicate.Invoke(i, j)) addLeap(leaps, rank + i, file + j);
				}
			}

			return leaps.ToImmutableArray();
		}

		private static ImmutableArray<int> kingLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => i != 0 || j != 0,
				rank, file, 1);
		
		private static ImmutableArray<int> knightLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => Math.Abs(i) + Math.Abs(j) == 3,
				rank, file, 2);

		private static ImmutableArray<int> elephantLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => (Math.Abs(i) == 1 && j == 0) || (i == 0 && Math.Abs(j) == 1) ||
								  (Math.Abs(i) == 2 && j == 0) || (i == 0 && Math.Abs(j) == 2),
				rank, file, 2);

		private static ImmutableArray<int> capturingGiraffeLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => (Math.Abs(i) == 2 && (j == 0 || Math.Abs(j) == 2)) ||
								  (Math.Abs(j) == 2 && (i == 0 || Math.Abs(i) == 2)),
				rank, file, 2);

		private static ImmutableArray<int> crocodileLeapGenerator(int rank, int file)
		{
			var square = rank * size + file;
			var direction = isAboveRiver(square) ? 1 : -1;
			
			Func<int, int, bool> riverPredicate =
				(int i, int j) => i != 0;
			
			Func<int, int, bool> groundPredicate =
				(int i, int j) => (i != 0 || j != 0) && (i != direction || j != 0);
			
			Func<int, int, bool> predicate = isRiver(rank * size + file) ?
				riverPredicate : groundPredicate;

			return circleLeapGenerator(predicate, rank, file, 1);
		}

		private static HashSet<int> whiteLionCastle = new HashSet<int> {
			(int)Square.C3, (int)Square.D3, (int)Square.E3,
			(int)Square.C2, (int)Square.D2, (int)Square.E2,
			(int)Square.C1, (int)Square.D1, (int)Square.E1
		};

		private static HashSet<int> blackLionCastle = new HashSet<int> {
			(int)Square.C7, (int)Square.D7, (int)Square.E7,
			(int)Square.C6, (int)Square.D6, (int)Square.E6,
			(int)Square.C5, (int)Square.D5, (int)Square.E5
		};

		private static ImmutableArray<int> lionLeapGenerator(CongoColor color, int rank, int file)
		{
			var leaps = new List<int>();
			var castle = color.IsWhite() ? whiteLionCastle : blackLionCastle;

			if (castle.Contains(rank * size + file)) {
				var possibleLeaps = kingLeapGenerator(rank, file);
				foreach (var leap in possibleLeaps) {
					if (castle.Contains(leap)) leaps.Add(leap);
				}
			}

			return leaps.ToImmutableArray();
		}

		private static ImmutableArray<int> pawnLeapGenerator(CongoColor color, int rank, int file)
		{
			var direction = color.IsBlack() ? 1 : -1;

			return circleLeapGenerator(
				(int i, int j) => i == direction,
				rank, file, 1);
		}

		private static ImmutableArray<int> superpawnLeapGenerator(CongoColor color, int rank, int file)
		{
			var direction = color.IsBlack() ? 1 : -1;

			return circleLeapGenerator(
				(int i, int j) => (i == direction) || (i == 0 && j != 0),
				rank, file, 1);
		}

		private static ImmutableArray<ImmutableArray<int>> precalculateLeaps(LeapGenerator gen)
		{
			var allLeaps = new ImmutableArray<int>[size * size];

			for (int rank = 0; rank < size; rank++) {
				for (int file = 0; file < size; file++) {
					allLeaps[rank * size + file] = gen.Invoke(rank, file);
				}
			}

			return allLeaps.ToImmutableArray();
		}

		private static ImmutableArray<ImmutableArray<int>> precalculateLeaps(
			ColoredLeapGenerator gen, CongoColor color)
		{
			var allLeaps = new ImmutableArray<int>[size * size];

			for (int rank = 0; rank < size; rank++) {
				for (int file = 0; file < size; file++) {
					allLeaps[rank * size + file] = gen.Invoke(color, rank, file);
				}
			}

			return allLeaps.ToImmutableArray();
		}

		/* The order is critical, solution is implemented due to 
		 * a performance issue (4-bit indexing). */
		public static readonly ImmutableArray<CongoPiece> sample =
			new CongoPiece[] {
				Ground.Piece, River.Piece, Elephant.Piece, Zebra.Piece, Giraffe.Piece,
				Crocodile.Piece, Pawn.Piece, Superpawn.Piece, Lion.Piece, Monkey.Piece,
				Captured.Piece
			}.ToImmutableArray();

		private static readonly CongoBoard empty =
			new CongoBoard(0, 0, new[] { 0U, 0U, 0U, 0x1111111U, 0U, 0U, 0U }.ToImmutableArray());

		private static bool getBit(ulong word, int position)
			=> ((word >> position) & 0x1UL) == 0x1UL;

		private static ulong setBitToValue(ulong current, int position, bool value)
			=> value ? current | (0x1UL << position) : current & ~(0x1UL << position);

		private static uint setPieceCode(uint rank, CongoPiece piece, int file)
		{
			var shift = file * 4;
			return (rank & ~(0xFU << shift)) | piece.Code << shift;
		}

		private static bool isAboveRiver(int square) => square / size < size / 2;
		
		private static bool isRiver(int square) => square / size == size / 2;
		
		private static bool isBelowRiver(int square) => square / size > size / 2;

		private static readonly ImmutableArray<ImmutableArray<int>> kingLeaps,
			knightLeaps, elephantLeaps, capturingGiraffeLeaps, crocodileLeaps,
			whiteLionLeaps, blackLionLeaps, whitePawnLeaps, blackPawnLeaps,
			whiteSuperpawnLeaps, blackSuperpawnLeaps;

		public static CongoBoard Empty => empty;
		
		static CongoBoard()
		{
			kingLeaps = precalculateLeaps(kingLeapGenerator);
			knightLeaps = precalculateLeaps(knightLeapGenerator);
			elephantLeaps = precalculateLeaps(elephantLeapGenerator);
			capturingGiraffeLeaps = precalculateLeaps(capturingGiraffeLeapGenerator);
			crocodileLeaps = precalculateLeaps(crocodileLeapGenerator);

			// color-dependent leaps
			whiteLionLeaps = precalculateLeaps(lionLeapGenerator, White.Color);
			blackLionLeaps = precalculateLeaps(lionLeapGenerator, Black.Color);
			whitePawnLeaps = precalculateLeaps(pawnLeapGenerator, White.Color);
			blackPawnLeaps = precalculateLeaps(pawnLeapGenerator, Black.Color);
			whiteSuperpawnLeaps = precalculateLeaps(superpawnLeapGenerator, White.Color);
			blackSuperpawnLeaps = precalculateLeaps(superpawnLeapGenerator, Black.Color);
		}

		private readonly ulong whiteOccupied;
		private readonly ulong blackOccupied;
		private readonly ImmutableArray<uint> pieces;

		private bool isPieceWhite(int square) => getBit(whiteOccupied, square);

		private uint getPieceCode(int square)
			=> (pieces[square / Size] >> (square % Size * 4)) & 0xFU;

		private CongoBoard(ulong whiteOccupied, ulong blackOccupied, ImmutableArray<uint> pieces)
		{
			this.whiteOccupied = whiteOccupied;
			this.blackOccupied = blackOccupied;
			this.pieces = pieces;
		}

		public int Size => size;

		public bool IsJungle(int rank, int file) => isJungle(rank, file);

		public bool IsJungle(int square) => isJungle(square);

		public bool IsAboveRiver(int square) => isAboveRiver(square);

		public bool IsRiver(int square) => isRiver(square);

		public bool IsBelowRiver(int square) => isBelowRiver(square);

		public bool IsOccupied(int square) => getBit(whiteOccupied | blackOccupied, square);

		public bool IsFirstMovePiece(int square) => isPieceWhite(square);

		public bool IsPieceFriendly(CongoColor color, int square)
			=> getBit(color.IsWhite() ? whiteOccupied : blackOccupied, square);

		public bool IsOpponentPiece(CongoColor color, int square)
			=> getBit(color.IsWhite() ? blackOccupied : whiteOccupied, square);

		public CongoPiece GetPiece(int square)
			=> sample[(int)getPieceCode(square)];

		public CongoBoard With(CongoColor color, CongoPiece piece, int square)
		{
			var rank  = square / Size;
			var shift = square % Size * 4;
			var newWhiteOccupied = whiteOccupied;
			var newBlackOccupied = blackOccupied;
			
			if (color.IsWhite()) {
				newWhiteOccupied = setBitToValue(whiteOccupied, square, true);
			} else {
				newBlackOccupied = setBitToValue(blackOccupied, square, true);
			}

			return new CongoBoard(newWhiteOccupied, newBlackOccupied,
				pieces.SetItem(rank, (pieces[rank] & ~(0xFU << shift)) | (piece.Code << shift)));
		}

		public CongoBoard Without(int square)
		{
			var newWhiteOccupied = setBitToValue(whiteOccupied, square, false);
			var newBlackOccupied = setBitToValue(blackOccupied, square, false);
			var newPiece = isRiver(square) ? River.Piece : Ground.Piece;
			var rank = square / Size;

			return new CongoBoard(newWhiteOccupied, newBlackOccupied,
				pieces.SetItem(rank, setPieceCode(pieces[rank], newPiece, square % Size)));
		}

		public ImmutableArray<int> LeapsAsKing(int square)
			=> kingLeaps[square];

		public ImmutableArray<int> LeapsAsKnight(int square)
			=> knightLeaps[square];

		public ImmutableArray<int> LeapsAsElephant(int square)
			=> elephantLeaps[square];

		public ImmutableArray<int> LeapsAsCapturingGiraffe(int square)
			=> capturingGiraffeLeaps[square];

		public ImmutableArray<int> LeapsAsCrocodile(int square)
			=> crocodileLeaps[square];

		public ImmutableArray<int> LeapsAsLion(CongoColor color, int square)
			=> color.IsWhite() ? whiteLionLeaps[square] : blackLionLeaps[square];

		public ImmutableArray<int> LeapsAsPawn(CongoColor color, int square)
			=> color.IsWhite() ? whitePawnLeaps[square] : blackPawnLeaps[square];

		public ImmutableArray<int> LeapsAsSuperpawn(CongoColor color, int square)
			=> color.IsWhite() ? whiteSuperpawnLeaps[square] : blackSuperpawnLeaps[square];

		public bool TryDiagonalJump(CongoColor color, int square, out int target)
		{
			var freePath = !IsOccupied((int)Square.D4);

			if (GetPiece(square).IsLion()) {
				switch (square) {
					case (int)Square.C3:
						target = (int)Square.E5;
						return color.IsWhite() && freePath && GetPiece(target).IsLion();
					case (int)Square.E3:
						target = (int)Square.C5;
						return color.IsWhite() && freePath && GetPiece(target).IsLion();
					case (int)Square.C5:
						target = (int)Square.E3;
						return color.IsBlack() && freePath && GetPiece(target).IsLion();
					case (int)Square.E5:
						target = (int)Square.C3;
						return color.IsBlack() && freePath && GetPiece(target).IsLion();
					default:
						break;
				}
			}

			target = -1;
			return false;
		}

		public bool IsLionCastle(CongoColor color, int square)
		{
			var castle = color.IsWhite() ? whiteLionCastle : blackLionCastle;
			return GetPiece(square).IsLion() && castle.Contains(square);
		}

		private class CongoBoardEnumerator : IParametrizedEnumerator<int>
		{
			private ulong occupancy;

			public CongoBoardEnumerator(ulong occupancy)
			{
				this.occupancy = occupancy;
				Current = -1;
			}

			public int Current { get; private set; }

			public bool MoveNext()
			{
				if (occupancy == 0) return false;
				var lsb = occupancy & (ulong.MaxValue - occupancy + 1);
				Current = Decoder.DeBruijnLSB(lsb);
				occupancy &= ~lsb;

				return true;
			}
		}

		public IParametrizedEnumerator<int> GetEnumerator(CongoColor color)
			=> new CongoBoardEnumerator(color.IsWhite() ? whiteOccupied : blackOccupied);
	}
}
