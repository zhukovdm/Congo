using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public class CongoBoard : IBoard, IParametrizedEnumerable<ColorCode, int> {

		private delegate ImmutableArray<int> LeapGenerator(int rank, int file);
		private delegate ImmutableArray<int> ColoredLeapGenerator(ColorCode color, int rank, int file);

		private static int size = 7;
		
		private static bool withinBoard(int rank, int file)
			=> rank >= 0 && rank < size && file >= 0 && file < size;

		private static bool withinBoard(int position)
			=> position >= 0 && position < size * size;

		private static void addLeap(List<int> leaps, int rank, int file) {
			if (withinBoard(rank, file)) leaps.Add(rank * size + file);
		}

		private static bool isPositionAllowed(int position, int[] allowed) {
			for (int i = 0; i < allowed.Length; i++) {
				if (position == allowed[i]) return true;
			}
			return false;
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
			var position = rank * size + file;
			var direction = isUpperPart(position) ? 1 : -1;

			Func<int, int, bool> ad = isSquareWater(rank * size + file)
				? ad = (int i, int j) => i != 0
				: ad = (int i, int j) => (i != 0 || j != 0) && (i != direction || j != 0);

			return circleLeapGenerator(ad, rank, file, 1);
		}

		private static int[] whiteLionAllowedLeaps = new int[] {
			(int)SquareCode.C3, (int)SquareCode.D3, (int)SquareCode.E3,
			(int)SquareCode.C2, (int)SquareCode.D2, (int)SquareCode.E2,
			(int)SquareCode.C1, (int)SquareCode.D1, (int)SquareCode.E1
		};

		private static int[] blackLionAllowedLeaps = new int[] {
			(int)SquareCode.C7, (int)SquareCode.D7, (int)SquareCode.E7,
			(int)SquareCode.C6, (int)SquareCode.D6, (int)SquareCode.E6,
			(int)SquareCode.C5, (int)SquareCode.D5, (int)SquareCode.E5
		};

		private static ImmutableArray<int> lionLeapGenerator(ColorCode color, int rank, int file) {
			var leaps = new List<int>();
			var allowedLeaps = color.IsWhite() ? whiteLionAllowedLeaps : blackLionAllowedLeaps;

			if (isPositionAllowed(rank * size + file, allowedLeaps)) {
				var possibleLeaps = kingLeapGenerator(rank, file);
				foreach (var leap in possibleLeaps) {
					if (isPositionAllowed(leap, allowedLeaps)) leaps.Add(leap);
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

		private static ImmutableArray<ImmutableArray<int>> precalculateLeaps(ColoredLeapGenerator gen, ColorCode color) {
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

		private static bool isUpperPart(int position) => position / size < size / 2;
		private static bool isSquareWater(int position) => position / size == size / 2;
		private static bool isLowerPart(int position) => position / size > size / 2;

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

		private bool IsPieceWhite(int position)
			=> getBit(occupied[(int)ColorCode.White], position);

		private CongoBoard(ImmutableArray<ulong> occupied, ImmutableArray<uint> pieces) {
			this.occupied = occupied; this.pieces = pieces;
		}

		public int Size => size;

		public bool WithinBoard(int position)
			=> withinBoard(position);

		public bool IsUpperPart(int position)
			=> isUpperPart(position);

		public bool IsSquareWater(int position)
			=> isSquareWater(position);

		public bool IsLowerPart(int position)
			=> isLowerPart(position);

		public bool IsSquareOccupied(int position)
			=> getBit(occupied[0] | occupied[1], position);

		public bool IsFirstMovePiece(int position)
			=> IsPieceWhite(position);

		public bool IsPieceFriendly(ColorCode color, int position)
			=> getBit(occupied[(int)color], position);

		public bool IsOpponentPiece(ColorCode color, int position)
			=> getBit(occupied[1 - (int)color], position);

		public PieceCode GetPieceCode(int position)
			=> (PieceCode)((pieces[position / Size] >> (position % Size * 4)) & 0xFU);

		public CongoPiece GetPiece(int position) {
			return !IsSquareOccupied(position) ? map[(int)PieceCode.Empty] :
				map[(int)GetPieceCode(position)];
		}

		public CongoBoard With(ColorCode color, PieceCode pieceCode, int position) {
			var occupy = setBitToValue(occupied[(int)color], position, true);
			var rank   = position / 7;
			var shift  = position % 7 * 4;
			return new CongoBoard(
				occupied.SetItem((int)color, occupy),
				pieces.SetItem(rank, (pieces[rank] & ~(0xFU << shift)) | ((uint)pieceCode << shift))
			);
		}

		public CongoBoard Without(int position) {
			var newOccupied = occupied;
			
			for (int i = 0; i < 2; i++) {
				newOccupied = newOccupied.SetItem(i, setBitToValue(occupied[i], position, false));
			}

			return new CongoBoard(newOccupied, pieces);
		}

		public ImmutableArray<int> LeapsAsKing(int position)
			=> kingLeaps[position];

		public ImmutableArray<int> LeapsAsKnight(int position)
			=> knightLeaps[position];

		public ImmutableArray<int> LeapsAsElephant(int position)
			=> elephantLeaps[position];

		public ImmutableArray<int> LeapsAsCapturingGiraffe(int position)
			=> capturingGiraffeLeaps[position];

		public ImmutableArray<int> LeapsAsCrocodile(int position)
			=> crocodileLeaps[position];

		public ImmutableArray<int> LeapsAsLion(ColorCode color, int position)
			=> color.IsWhite() ? whiteLionLeaps[position] : blackLionLeaps[position];

		public ImmutableArray<int> LeapsAsPawn(ColorCode color, int position)
			=> color.IsWhite() ? whitePawnLeaps[position] : blackPawnLeaps[position];

		public ImmutableArray<int> LeapsAsSuperpawn(ColorCode color, int position)
			=> color.IsWhite() ? whiteSuperpawnLeaps[position] : blackSuperpawnLeaps[position];

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
