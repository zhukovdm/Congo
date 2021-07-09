using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public struct CongoMove {

		public readonly uint moveWord;
		
		public CongoMove(int fromRank, int fromFile, int toRank, int toFile) {
			moveWord = 0x0;
			moveWord = moveWord | (uint)fromRank <<  0;
			moveWord = moveWord | (uint)fromFile <<  8;
			moveWord = moveWord | (uint)toRank   << 16;
			moveWord = moveWord | (uint)toFile   << 24;
		}

		public int frr => (int)((moveWord >>  0) & 0xFF);
		public int frf => (int)((moveWord >>  8) & 0xFF);
		public int tor => (int)((moveWord >> 16) & 0xFF);
		public int tof => (int)((moveWord >> 24) & 0xFF);

	}

	public abstract class CongoPlayer {

		private readonly ImmutableArray<CongoMove> moves;

		public CongoPlayer(ColorCode color, CongoBoard board) {
			moves = GenerateMoves(color, board);
		}

		protected static ImmutableArray<CongoMove> GenerateMoves(ColorCode color, CongoBoard board) {
			var allMoves = new List<CongoMove>();
			for (int rank = 0; rank < board.Size; rank++) {
				for (int file = 0; file < board.Size; file++) {
					if (board.IsPieceFriendly(color, rank, file)) {
						var piece = board.GetPiece(rank, file);
						allMoves.AddRange(piece.GetMoves(color, board, rank, file));
					}
				}
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
