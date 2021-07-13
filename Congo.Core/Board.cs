using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public class CongoBoard : IBoard, IParametrizedEnumerable<ColorCode, int> {

		private delegate ImmutableArray<int> LeapGenerator(int rank, int file);

		private static int size = 7;
		
		private static bool withinBoard(int rank, int file)
			=> rank >= 0 && rank < size && file >= 0 && file < size;

		private static bool tryAddLeap(List<int> leaps, int rank, int file) {
			if (withinBoard(rank, file)) {
				leaps.Add(rank * size + file);
				return true;
			} else {
				return false;
			}
		}

		private static bool IsPositionAllowed(int position, int[] allowed) {
			for (int i = 0; i < allowed.Length; i++) {
				if (position == allowed[i]) return true;
			}
			return false;
		}

		private static ImmutableArray<int> kingLeapGenerator(int rank, int file) {
			var leaps = new List<int>();
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (i != 0 || j != 0) {
						_ = tryAddLeap(leaps, rank, file);
					}
				}
			}
			return leaps.ToImmutableArray();
		}

		private static ImmutableArray<int> knightLeapGenerator(int rank, int file) {
			var leaps = new List<int>();
			for (int i = -2; i < 3; i++) {
				for (int j = -2; j < 3; j++) {
					if (Math.Abs(i) + Math.Abs(j) == 3) {
						_ = tryAddLeap(leaps, rank + i, file + j);
					}
				}
			}
			return leaps.ToImmutableArray();
		}

		private static ImmutableArray<int> elephantLeapGenerator(int rank, int file) {
			var leaps = new List<int>();
			for (int i = -2; i < 3; i++) {
				for (int j = -2; j < 3; j++) {
					if ((Math.Abs(i) == 1 && j == 0) || (i == 0 && Math.Abs(j) == 1) ||
						(Math.Abs(i) == 2 && j == 0) || (i == 0 && Math.Abs(j) == 2)) {
						_ = tryAddLeap(leaps, rank + i, file + j);
					}
				}
			}
			return leaps.ToImmutableArray();
		}

		private static ImmutableArray<int> capturingGiraffeLeapGenerator(int rank, int file) {
			var leaps = new List<int>();
			for (int i = -2; i < 3; i++) {
				for (int j = -2; j < 3; j++) {
					if ((Math.Abs(i) == 2 && (j == 0 || Math.Abs(j) == 2)) ||
						(Math.Abs(j) == 2 && (i == 0 || Math.Abs(i) == 2))) {
						_ = tryAddLeap(leaps, rank + i, file + j);
					}
				}
			}
			return leaps.ToImmutableArray();
		}

		private static ImmutableArray<ImmutableArray<int>> lionLeapGenerator(ColorCode color) {

			var allLeaps = new ImmutableArray<int>[size * size];

			var whiteAllowed = new int[] { 30, 31, 32, 37, 38, 39, 44, 45, 46 };
			var blackAllowed = new int[] {  2,  3,  4,  9, 10, 11, 16, 17, 18 };
			var allowed = color.IsWhite() ? whiteAllowed : blackAllowed;
			
			for (int rank = 0; rank < size; rank++) {
				for (int file = 0; file < size; file++) {
					var leaps = new List<int>();
					if (IsPositionAllowed(rank * size + file, allowed)) {
						var possibleLeaps = kingLeapGenerator(rank, file);
						foreach (var leap in possibleLeaps) {
							if (IsPositionAllowed(leap, allowed)) leaps.Add(leap);
						}
					}
					allLeaps[rank * size + file] = leaps.ToImmutableArray();
				}
			}

			return allLeaps.ToImmutableArray();
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

		private static readonly ImmutableArray<CongoPiece> map = new CongoPiece[] {
			new Empty(), new Lion(), new Zebra(), new Elephant(), new Giraffe(),
			new Crocodile(), new Pawn(), new Superpawn(), new Monkey(), new Captured()
		}.ToImmutableArray();

		private static readonly CongoBoard empty = new CongoBoard(
			new ulong[2].ToImmutableArray(), new uint[7].ToImmutableArray());

		private static bool getBit(ulong word, int position)
			=> ((word >> position) & 0x1UL) == 0x1UL;

		private static ulong setBitToValue(ulong current, int position, bool value)
			=> value ? current | (0x1UL << position) : current & ~(0x1UL << position);

		private static readonly ImmutableArray<ImmutableArray<int>> kingLeaps,
			knightLeaps, elephantLeaps, capturingGiraffeLeaps, whiteLionLeaps,
			blackLionLeaps;

		public static CongoBoard Empty => empty;
		
		static CongoBoard() {
			kingLeaps = precalculateLeaps(kingLeapGenerator);
			knightLeaps = precalculateLeaps(knightLeapGenerator);
			elephantLeaps = precalculateLeaps(elephantLeapGenerator);
			capturingGiraffeLeaps = precalculateLeaps(capturingGiraffeLeapGenerator);
			crocodileLeaps = crocodileLeapGenerator();
			whiteLionLeaps = lionLeapGenerator(ColorCode.White);
			blackLionLeaps = lionLeapGenerator(ColorCode.Black);
			whitePawnLeaps = pawnLeapGenerator(ColorCode.White);
			blackPawnLeaps = pawnLeapGenerator(ColorCode.Black);
			whiteSuperpawnLeaps = superpawnLeapGenerator(ColorCode.White);
			blackSuperpawnLeaps = superpawnLeapGenerator(ColorCode.Black);
		}

		private readonly ImmutableArray<ulong> occupied;
		private readonly ImmutableArray<uint> pieces;

		private bool IsPieceWhite(int position)
			=> getBit(occupied[(int)ColorCode.White], position);

		private CongoBoard(ImmutableArray<ulong> occupied, ImmutableArray<uint> pieces) {
			this.occupied = occupied; this.pieces = pieces;
		}

		public int Size => size;

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

		public ImmutableArray<int> LeapsAsLion(ColorCode color, int position)
			=> color.IsWhite() ? whiteLionLeaps[position] : blackLionLeaps[position];

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
