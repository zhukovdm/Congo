using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public class CongoBoard : IBoard {

		private static readonly ImmutableArray<CongoPiece> map =
			ImmutableArray.Create(new CongoPiece[] {
				new Empty(), new Lion(), new Zebra(), new Elephant(),
				new Giraffe(), new Crocodile(), new Pawn(), new Superpawn(),
				new Monkey(), new Captured()
			});

		private static readonly CongoBoard empty =
			new CongoBoard(ImmutableArray.Create(new ulong[2]),
						   ImmutableArray.Create(new uint[7]));
		
		public static CongoBoard Empty => empty;

		private readonly ImmutableArray<ulong> occupied;
		private readonly ulong bothOccupied;
		private readonly ImmutableArray<uint> pieces;

		private bool getBit(ulong current, int rank, int file)
			=> ((current >> (rank * Size + file)) & 0x1UL) == 0x1UL;

		private ulong setBitToValue(ulong current, int rank, int file, bool value) {
			var position = rank * Size + file;
			return value ? current | (0x1UL << position) : current & ~(0x1UL << position);
		}

		private CongoBoard(ImmutableArray<ulong> occupied, ImmutableArray<uint> pieces) {
			this.occupied = occupied;
			bothOccupied = occupied[0] | occupied[1];
			this.pieces = pieces;
		}

		public int Size => 7;

		public ulong GetOccupiedSquares(ColorCode color) =>
			occupied[(int)color];

		public bool IsSquareOccupied(int rank, int file) =>
			getBit(bothOccupied, rank, file);

		public bool IsPieceWhite(int rank, int file) =>
			getBit(occupied[(int)ColorCode.White], rank, file);

		public bool IsFirstMovePiece(int rank, int file) =>
			IsPieceWhite(rank, file);

		public bool IsPieceFriendly(ColorCode color, int rank, int file) =>
			getBit(occupied[(int)color], rank, file);

		public bool IsOpponentPiece(ColorCode color, int rank, int file) =>
			getBit(occupied[1 - (int)color], rank, file);

		public PieceCode GetPieceCode(int rank, int file) {
			return (PieceCode)((pieces[rank] >> (file * 4)) & 0xFU);
		}

		public CongoPiece GetPiece(int rank, int file) {
			return !IsSquareOccupied(rank, file) ? map[(int)PieceCode.Empty] :
				map[(int)GetPieceCode(rank, file)];
		}

		public CongoBoard With(ColorCode color, PieceCode pieceCode, int rank, int file) {
			var occupy = setBitToValue(occupied[(int)color], rank, file, true);
			var shift = file * 4;
			var line = pieces[rank];
			var mask = ~(0xFU << shift);
			var value = (uint)pieceCode << shift;
			return new CongoBoard(
				occupied.SetItem((int)color, occupy),
				pieces.SetItem(rank, (line & mask) | value)
			);
		}

		public CongoBoard Without(int rank, int file) {
			var newOccupied = occupied;
			for (int i = 0; i < 2; i++) {
				newOccupied = newOccupied.SetItem(
					i, setBitToValue(occupied[i], rank, file, false)
				);
			}
			return new CongoBoard(newOccupied, pieces);
		}

	}

}
