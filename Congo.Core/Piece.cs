using System.Collections.Generic;
using System.Collections.Immutable;

using Congo.Def;

namespace Congo.Core {

	public abstract class CongoPiece {
		public abstract List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file);
	}

	public sealed class Empty : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Lion : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Zebra : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Elephant : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Giraffe : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Crocodile : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Pawn : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Superpawn : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Monkey : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

	public sealed class Captured : CongoPiece {
		public override List<CongoMove> GetMoves(ColorCode color, CongoBoard board, int rank, int file) {
			return new List<CongoMove>();
		}
	}

}
