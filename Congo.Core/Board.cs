using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public class CongoBoard : IBoard, IParametrizedEnumerable<ColorCode, int> {

		private delegate ImmutableArray<int> LeapGenerator(int rank, int file);
		private delegate ImmutableArray<int> ColoredLeapGenerator(ColorCode color, int rank, int file);

		private static readonly int size = 7;

		private static bool withinBoard(int rank, int file)
			=> rank >= 0 && rank < size && file >= 0 && file < size;

		private static bool withinBoard(int square)
			=> square >= 0 && square < size * size;

		private static void addLeap(List<int> leaps, int rank, int file) {
			if (withinBoard(rank, file)) leaps.Add(rank * size + file);
		}

		private static ImmutableArray<int> circleLeapGenerator(
			Func<int, int, bool> condition, int rank, int file, int radius) {
			var leaps = new List<int>();

			for (int i = -radius; i < radius + 1; i++) {
				for (int j = -radius; j < radius + 1; j++) {
					if (condition.Invoke(i, j)) addLeap(leaps, rank + i, file + j);
				}
			}

			return leaps.ToImmutableArray();
		}

		private static ImmutableArray<int> kingLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => i != 0 || j != 0,
				rank, file, 1
			);
		
		private static ImmutableArray<int> knightLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => Math.Abs(i) + Math.Abs(j) == 3,
				rank, file, 2
			);

		private static ImmutableArray<int> elephantLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => (Math.Abs(i) == 1 && j == 0) || (i == 0 && Math.Abs(j) == 1) ||
								  (Math.Abs(i) == 2 && j == 0) || (i == 0 && Math.Abs(j) == 2),
				rank, file, 2
			);

		private static ImmutableArray<int> capturingGiraffeLeapGenerator(int rank, int file)
			=> circleLeapGenerator(
				(int i, int j) => (Math.Abs(i) == 2 && (j == 0 || Math.Abs(j) == 2)) ||
								  (Math.Abs(j) == 2 && (i == 0 || Math.Abs(i) == 2)),
				rank, file, 2
			);

		private static ImmutableArray<int> crocodileLeapGenerator(int rank, int file) {
			var square = rank * size + file;
			var direction = isAboveRiver(square) ? 1 : -1;

			Func<int, int, bool> ad = isRiver(rank * size + file)
				? ad = (int i, int j) => i != 0
				: ad = (int i, int j) => (i != 0 || j != 0) && (i != direction || j != 0);

			return circleLeapGenerator(ad, rank, file, 1);
		}

		private static HashSet<int> whiteLionCastle = new HashSet<int> {
			(int)SquareCode.C3, (int)SquareCode.D3, (int)SquareCode.E3,
			(int)SquareCode.C2, (int)SquareCode.D2, (int)SquareCode.E2,
			(int)SquareCode.C1, (int)SquareCode.D1, (int)SquareCode.E1
		};

		private static HashSet<int> blackLionCastle = new HashSet<int> {
			(int)SquareCode.C7, (int)SquareCode.D7, (int)SquareCode.E7,
			(int)SquareCode.C6, (int)SquareCode.D6, (int)SquareCode.E6,
			(int)SquareCode.C5, (int)SquareCode.D5, (int)SquareCode.E5
		};

		private static ImmutableArray<int> lionLeapGenerator(ColorCode color, int rank, int file) {
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

		private static ImmutableArray<int> pawnLeapGenerator(ColorCode color, int rank, int file) {
			var direction = color.IsBlack() ? 1 : -1;
			return circleLeapGenerator(
				(int i, int j) => i == direction,
				rank, file, 1
			);
		}

		private static ImmutableArray<int> superpawnLeapGenerator(ColorCode color, int rank, int file) {
			var direction = color.IsBlack() ? 1 : -1;
			return circleLeapGenerator(
				(int i, int j) => (i == direction) || (i == 0 && j != 0),
				rank, file, 1
			);
		}

		private static ImmutableArray<ImmutableArray<int>> precalculateLeaps(LeapGenerator gen) {
			var allLeaps = new ImmutableArray<int>[size * size];

			for (int rank = 0; rank < size; rank++) {
				for (int file = 0; file < size; file++) {
					allLeaps[rank * size + file] = gen.Invoke(rank, file);
				}
			}

			return allLeaps.ToImmutableArray();
		}

		private static ImmutableArray<ImmutableArray<int>> precalculateLeaps(
			ColoredLeapGenerator gen, ColorCode color) {
			var allLeaps = new ImmutableArray<int>[size * size];

			for (int rank = 0; rank < size; rank++) {
				for (int file = 0; file < size; file++) {
					allLeaps[rank * size + file] = gen.Invoke(color, rank, file);
				}
			}

			return allLeaps.ToImmutableArray();
		}

		private static readonly ImmutableArray<CongoPiece> map = new CongoPiece[] {
			new Empty(), new Lion(), new Zebra(), new Elephant(), new Giraffe(),
			new Crocodile(), new Pawn(), new Superpawn(), new Monkey(), new Captured()
		}.ToImmutableArray();

		private static readonly CongoBoard empty = new CongoBoard(
			new ulong[2].ToImmutableArray(), new uint[size].ToImmutableArray());

		private static bool getBit(ulong word, int position)
			=> ((word >> position) & 0x1UL) == 0x1UL;

		private static ulong setBitToValue(ulong current, int position, bool value)
			=> value ? current | (0x1UL << position) : current & ~(0x1UL << position);

		private uint setPieceCode(uint current, PieceCode code, int file) {
			var shift = file * 4;
			return (current & ~(0xFU << shift)) | (uint)code << shift;
		}

		private static bool isAboveRiver(int square) => square / size < size / 2;
		private static bool isRiver(int square) => square / size == size / 2;
		private static bool isBelowRiver(int square) => square / size > size / 2;

		private static readonly ImmutableArray<ImmutableArray<int>> kingLeaps,
			knightLeaps, elephantLeaps, capturingGiraffeLeaps, crocodileLeaps,
			whiteLionLeaps, blackLionLeaps, whitePawnLeaps, blackPawnLeaps,
			whiteSuperpawnLeaps, blackSuperpawnLeaps;

		public static CongoBoard Empty => empty;
		
		static CongoBoard() {
			kingLeaps = precalculateLeaps(kingLeapGenerator);
			knightLeaps = precalculateLeaps(knightLeapGenerator);
			elephantLeaps = precalculateLeaps(elephantLeapGenerator);
			capturingGiraffeLeaps = precalculateLeaps(capturingGiraffeLeapGenerator);
			crocodileLeaps = precalculateLeaps(crocodileLeapGenerator);

			// color-dependent leaps
			whiteLionLeaps = precalculateLeaps(lionLeapGenerator, ColorCode.White);
			blackLionLeaps = precalculateLeaps(lionLeapGenerator, ColorCode.Black);
			whitePawnLeaps = precalculateLeaps(pawnLeapGenerator, ColorCode.White);
			blackPawnLeaps = precalculateLeaps(pawnLeapGenerator, ColorCode.Black);
			whiteSuperpawnLeaps = precalculateLeaps(superpawnLeapGenerator, ColorCode.White);
			blackSuperpawnLeaps = precalculateLeaps(superpawnLeapGenerator, ColorCode.Black);
		}

		private readonly ImmutableArray<ulong> occupied;
		private readonly ImmutableArray<uint> pieces;

		private bool IsPieceWhite(int square)
			=> getBit(occupied[(int)ColorCode.White], square);

		private CongoBoard(ImmutableArray<ulong> occupied, ImmutableArray<uint> pieces) {
			this.occupied = occupied; this.pieces = pieces;
		}

		public int Size => size;

		public bool WithinBoard(int rank, int file) => withinBoard(rank, file);

		public bool WithinBoard(int square) => withinBoard(square);

		public bool IsAboveRiver(int square) => isAboveRiver(square);

		public bool IsRiver(int square) => isRiver(square);

		public bool IsBelowRiver(int square) => isBelowRiver(square);

		public bool IsOccupied(int square) => getBit(occupied[0] | occupied[1], square);

		public bool IsFirstMovePiece(int square) => IsPieceWhite(square);

		public bool IsPieceFriendly(ColorCode color, int square)
			=> getBit(occupied[(int)color], square);

		public bool IsOpponentPiece(ColorCode color, int square)
			=> getBit(occupied[1 - (int)color], square);

		public PieceCode GetPieceCode(int square)
			=> (PieceCode)((pieces[square / Size] >> (square % Size * 4)) & 0xFU);

		public CongoPiece GetPiece(int square)
			=> !IsOccupied(square) ? map[(int)PieceCode.Empty] : map[(int)GetPieceCode(square)];

		public CongoBoard With(ColorCode color, PieceCode pieceCode, int square) {
			var occupy = setBitToValue(occupied[(int)color], square, true);
			var rank   = square / 7;
			var shift  = square % 7 * 4;

			return new CongoBoard(
				occupied.SetItem((int)color, occupy),
				pieces.SetItem(rank, (pieces[rank] & ~(0xFU << shift)) | ((uint)pieceCode << shift))
			);
		}

		public CongoBoard Without(int square) {
			var newOccupied = occupied;
			
			for (int i = 0; i < 2; i++) {
				newOccupied = newOccupied.SetItem(i, setBitToValue(occupied[i], square, false));
			}

			var rank = square / size;
			var newPieces = pieces.SetItem(rank, setPieceCode(pieces[rank], PieceCode.Empty, square % size));

			return new CongoBoard(newOccupied, newPieces);
		}

		public ImmutableArray<int> LeapsAsKing(int square) => kingLeaps[square];

		public ImmutableArray<int> LeapsAsKnight(int square) => knightLeaps[square];

		public ImmutableArray<int> LeapsAsElephant(int square) => elephantLeaps[square];

		public ImmutableArray<int> LeapsAsCapturingGiraffe(int square) => capturingGiraffeLeaps[square];

		public ImmutableArray<int> LeapsAsCrocodile(int square) => crocodileLeaps[square];

		public ImmutableArray<int> LeapsAsLion(ColorCode color, int square)
			=> color.IsWhite() ? whiteLionLeaps[square] : blackLionLeaps[square];

		public ImmutableArray<int> LeapsAsPawn(ColorCode color, int square)
			=> color.IsWhite() ? whitePawnLeaps[square] : blackPawnLeaps[square];

		public ImmutableArray<int> LeapsAsSuperpawn(ColorCode color, int square)
			=> color.IsWhite() ? whiteSuperpawnLeaps[square] : blackSuperpawnLeaps[square];

		public bool TryDiagonalJump(ColorCode color, int square, out int target) {
			var freePath = !IsOccupied((int)SquareCode.D4);
			if (GetPiece(square) is Lion) {
				switch (square) {
					case (int)SquareCode.C3:
						target = (int)SquareCode.E5;
						return color.IsWhite() && freePath && GetPiece(target) is Lion;
					case (int)SquareCode.E3:
						target = (int)SquareCode.C5;
						return color.IsWhite() && freePath && GetPiece(target) is Lion;
					case (int)SquareCode.C5:
						target = (int)SquareCode.E3;
						return color.IsBlack() && freePath && GetPiece(target) is Lion;
					case (int)SquareCode.E5:
						target = (int)SquareCode.C3;
						return color.IsBlack() && freePath && GetPiece(target) is Lion;
					default:
						break;
				}
			}
			target = -1;
			return false;
		}

		public bool InsideCastle(ColorCode color, int square) {
			var castle = color.IsWhite() ? whiteLionCastle : blackLionCastle;
			return GetPiece(square) is Lion && castle.Contains(square);
		}

		public IParametrizedEnumerator<int> GetEnumerator(ColorCode color)
			=> new CongoBoardEnumerator(occupied[(int)color]);

		private class CongoBoardEnumerator : IParametrizedEnumerator<int> {

			private ulong occupancy;

			public CongoBoardEnumerator(ulong occupancy) {
				this.occupancy = occupancy;
				Current = -1;
			}

			public int Current { get; private set; }

			public bool MoveNext() {
				if (occupancy == 0) return false;
				var lsb = occupancy & (ulong.MaxValue - occupancy + 1);
				Current = Decoder.DeBruijnLSB(lsb);
				occupancy &= ~lsb;
				return true;
			}

		}

	}

}
