using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core {

	struct Move {
		readonly int from, to;
		public Move(int from, int to, Board board) {
			this.from = from; this.to = to;
		}

	}

	public class Board {

		private static Type[] numToPiece;
		private static Dictionary<Type, uint> pieceToNum;
		
		static Board() {

			numToPiece = new Type[8] {
				typeof(Lion), typeof(Zebra), typeof(Elephant), typeof(Giraffe),
				typeof(Crocodile), typeof(Pawn), typeof(Superpawn), typeof(Monkey)
			};

			pieceToNum = new Dictionary<Type, uint>() {
				{ typeof(Lion), 0 }, { typeof(Zebra), 1 },
				{ typeof(Elephant), 2 }, { typeof(Giraffe), 3 },
				{ typeof(Crocodile), 4 }, { typeof(Pawn), 5 },
				{ typeof(Superpawn), 6 }, { typeof(Monkey), 7 },
				{ typeof(Empty), 8 }
			};

		}

		private static ulong setBitToVal(ulong cur, int pos, int val) {
			return (val == 0) ? cur & ~(0x1UL << pos) : cur | (0x1UL << pos);
		}

		private static ulong getBit(ulong cur, int pos) {
			return (cur >> pos) & 0x1UL;
		}

		struct BoardRepr {

			readonly ImmutableArray<uint> pieceBank;

			public BoardRepr (ImmutableArray<uint> pieceBank) {
				this.pieceBank = pieceBank;
			}

			public BoardRepr With(int position, Type pieceType) {
				var shift = position % 7 * 4;
				var rank = pieceBank[position / 7];
				var mask = ~(0xFU << shift);
				var val = pieceToNum[pieceType] << shift;
				return new BoardRepr(
					pieceBank.SetItem(position / 7,(rank & mask) | val)
				);
			}

			public Type GetPieceType(int position) {
				uint num = (pieceBank[position / 7] >> (position % 7 * 4)) & 0xFU;
				return numToPiece[num];
			}

		}

		private static BoardRepr initRank(BoardRepr br, int rank) {
			int offset = rank * 7;
			return br.With(0 + offset, typeof(Giraffe))
				     .With(1 + offset, typeof(Monkey))
					 .With(2 + offset, typeof(Elephant))
					 .With(3 + offset, typeof(Lion))
					 .With(4 + offset, typeof(Elephant))
					 .With(5 + offset, typeof(Crocodile))
					 .With(6 + offset, typeof(Zebra));
		}

		public static Board CreateStandard() {
			ulong bo = 0; for (int i =   0; i < 2*7; i++) { bo = setBitToVal(bo, i, 1); }
			ulong wo = 0; for (int i = 5*7; i < 7*7; i++) { wo = setBitToVal(wo, i, 1); }
			var br = new BoardRepr(ImmutableArray.Create(new uint[7]));
			br = initRank(br, 0);
			for (int i = 1*7; i < 2*7; i++) { br = br.With(i, typeof(Pawn)); }
			for (int i = 5*7; i < 6*7; i++) { br = br.With(i, typeof(Pawn)); }
			br = initRank(br, 6);
			return new Board(bo, wo, br);
		}

		public static Board CreateFromFEN(string fen) => throw new Exception();

		readonly int activePlayer = 0;
		readonly ImmutableArray<Player> players;
		readonly ulong blackBits;
		readonly ulong whiteBits;
		readonly ulong bothBits;
		readonly BoardRepr boardRepr;

		private Board(ulong blackBits, ulong whiteBits, BoardRepr boardRepr) {
			this.activePlayer = 0; // white
			this.players = ImmutableArray.Create<Player>();
			this.blackBits = blackBits;
			this.whiteBits = whiteBits;
			this.bothBits  = blackBits | whiteBits;
			this.boardRepr = boardRepr;
		}

		private bool IsOccupied(int position) => getBit(bothBits, position) == 1U;

		private bool IsPieceOpposite(int position) =>
			activePlayer == 0 ? IsPieceBlack(position)
			                  : IsPieceWhite(position);

		public bool IsPieceWhite(int position) => getBit(whiteBits, position) == 1U;

		public bool IsPieceBlack(int position) => getBit(blackBits, position) == 1U;

		public Type GetPieceType(int position) =>
			IsOccupied(position) ? boardRepr.GetPieceType(position)
								 : typeof(Empty);

		public bool IsOpponentPiece(int position) => throw new Exception();

		public Board Clone(Board board) => throw new Exception();

		private ImmutableArray<Move> generateMoves() => throw new Exception();

		public bool IsFinal() {
			return true;
			/*
			generateAllMoves();
			return PassivePlayer.InCheck()    ||
				   ActivePlayer.InCheckmate() ||
				   IsUnsolvable();
			*/
		}

	}

}
