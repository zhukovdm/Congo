using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public abstract class CongoPlayer {

		private readonly ImmutableArray<CongoMove> moves;

		public CongoPlayer(ColorCode color, CongoBoard board) {
			moves = GenerateMoves(color, board);
		}

		protected static ImmutableArray<CongoMove> GenerateMoves(ColorCode color, CongoBoard board) {
			var allMoves = new List<CongoMove>();
			var e = board.GetEnumerator(color);
			while (e.MoveNext()) {
				var piece = board.GetPiece(e.Current);
				allMoves.AddRange(piece.GetMoves(color, board, e.Current));
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
