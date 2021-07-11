using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public class CongoBoard : IBoard, IParametrizedEnumerable<ColorCode, int> {

		private static int size = 7;
		public static readonly ImmutableArray<ImmutableArray<int>> KingMoves;
		public static readonly ImmutableArray<ImmutableArray<int>> KnightMoves;

		private static bool withinBoard(int rank, int file)
			=> rank >= 0 && rank < size && file >= 0 && file < size;

		private static ImmutableArray<int> precalculateKingMoves(int rank, int file) {
			var moves = new List<int>();
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					if (!(i == 0 && j == 0)) {
						var newRank = rank + i;
						var newFile = file + j;
						if (withinBoard(newRank, newFile)) {
							moves.Add(newRank * size + newFile);
						}
					}
				}
			}
			return moves.ToImmutableArray();
		}

		private static ImmutableArray<ImmutableArray<int>> precalculateKingMoves() {
			var allPositions = new ImmutableArray<int>[size * size];
			for (int rank = 0; rank < size; rank++) {
				for (int file = 0; file < size; file++) {
					allPositions[rank * size + file] = precalculateKingMoves(rank, file);
				}
			}
			return allPositions.ToImmutableArray();
		}

		private static ImmutableArray<int> precalculateKnightMoves(int rank, int file) {
			var moves = new List<int>();
			for (int i = -2; i < 3; i++) {
				for (int j = -2; j < 3; j++) {
					if (Math.Abs(i) + Math.Abs(j) == 3) {
						var newRank = rank + i;
						var newFile = file + j;
						if (withinBoard(newRank, newFile)) {
							moves.Add(newRank * size + newFile);
						}
					}
				}
			}
			return moves.ToImmutableArray();
		}

		private static ImmutableArray<ImmutableArray<int>> precalculateKnightMoves() {
			var allPositions = new ImmutableArray<int>[size * size];
			for (int rank = 0; rank < size; rank++) {
				for (int file = 0; file < size; file++) {
					allPositions[rank * size + file] = precalculateKnightMoves(rank, file);
				}
			}
			return allPositions.ToImmutableArray();
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

		public static CongoBoard Empty => empty;

		static CongoBoard() {
			KingMoves = precalculateKingMoves();
			KnightMoves = precalculateKnightMoves();
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
