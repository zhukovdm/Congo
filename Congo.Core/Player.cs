using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public struct CongoMove {

		public readonly uint moveWord;
		
		public CongoMove(int fromRank, int fromFile, int toRank, int toFile) {
			moveWord = 0x0U;
			moveWord |= (uint)fromRank <<  0;
			moveWord |= (uint)fromFile <<  8;
			moveWord |= (uint)toRank   << 16;
			moveWord |= (uint)toFile   << 24;
		}

		public int FrR => (int)((moveWord >>  0) & 0xFFUL);
		public int FrF => (int)((moveWord >>  8) & 0xFFUL);
		public int ToR => (int)((moveWord >> 16) & 0xFFUL);
		public int ToF => (int)((moveWord >> 24) & 0xFFUL);

	}

	public abstract class CongoPlayer {

		private static readonly ulong deBruijnNumber = 0x03F79D71B4CB0A89;
		private static readonly ImmutableArray<int> deBruijnHash =
			ImmutableArray.Create(new int[] {
				 0,  1, 48,  2, 57, 49, 28,  3,
				61, 58, 50, 42, 38, 29, 17,  4,
				62, 55, 59, 36, 53, 51, 43, 22,
				45, 39, 33, 30, 24, 18, 12,  5,
				63, 47, 56, 27, 60, 41, 37, 16,
				54, 35, 52, 21, 44, 32, 23, 11,
				46, 26, 40, 15, 34, 20, 31, 10,
				25, 14, 19,  9, 13,  8,  7,  6
			});
		private readonly ImmutableArray<CongoMove> moves;

		public CongoPlayer(ColorCode color, CongoBoard board) {
			moves = GenerateMoves(color, board);
		}

		// credit: https://www.chessprogramming.org/BitScan
		public static int DeBruijnLSB(ulong word) {
			return deBruijnHash[(int)((word * deBruijnNumber) >> 58)];
		}

		protected static ImmutableArray<CongoMove> GenerateMoves(ColorCode color, CongoBoard board) {
			var allMoves = new List<CongoMove>();
			var occupied = board.GetOccupiedSquares(color);
			while (occupied != 0) {
				var lsb = occupied & (ulong.MaxValue - occupied + 1);
				var index = DeBruijnLSB(lsb);
				var rank = index / board.Size;
				var file = index % board.Size;
				var piece = board.GetPiece(rank, file);
				allMoves.AddRange(piece.GetMoves(color, board, rank, file));
				occupied &= ~lsb;
			}
			return allMoves.ToImmutableArray();
		}
	}

	sealed class CongoAIPlayer : CongoPlayer {
		public CongoAIPlayer(ColorCode color, CongoBoard board) : base(color, board) { }
	}

	sealed class CongoHIPlayer : CongoPlayer {
		public CongoHIPlayer(ColorCode color, CongoBoard board) : base(color, board) { }
	}

}
