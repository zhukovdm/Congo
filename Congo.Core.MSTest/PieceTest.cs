using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Congo.Def;

namespace Congo.Core.MSTest {

	[TestClass]
	public class Piece_ElephantMoves_Test {

		[TestMethod]
		public void SkipFriendlyPieces() {
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Elephant, (int)SquareCode.A7)
				.With(ColorCode.White, PieceCode.Elephant, (int)SquareCode.B7);
			var piece = board.GetPiece((int)SquareCode.A7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.A7);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.A7, (int)SquareCode.C7),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.A6),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.A5)
			};
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void CaptureOpponentPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Elephant, (int)SquareCode.A7)
				.With(ColorCode.Black, PieceCode.Elephant, (int)SquareCode.B7)
				.With(ColorCode.Black, PieceCode.Elephant, (int)SquareCode.C7);
			var piece = board.GetPiece((int)SquareCode.A7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.A7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.A7, (int)SquareCode.C7),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.B7),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.A6),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.A5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

	}

	[TestClass]
	public class Piece_ZebraMoves_Test {

		[TestMethod]
		public void SkipFriendlyPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.D4)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.E6)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.F5)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.B3)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.E2);
			var piece = board.GetPiece((int)SquareCode.D4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D4, (int)SquareCode.F3),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C2),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C6),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.B5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void CaptureOpponentPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.D7)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.C5)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.B6)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.F6)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.E5);
			var piece = board.GetPiece((int)SquareCode.D7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D7, (int)SquareCode.B6),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.C5),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.E5),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.F6)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

	}

	[TestClass]
	public class Piece_GiraffeMoves_Test {

		[TestMethod]
		public void SkipFriendlyPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.A7)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.A6)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.B7)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.A5)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.C5);
			var piece = board.GetPiece((int)SquareCode.A7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.A7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.A7, (int)SquareCode.B6),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.C7)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void CaptureAndSkipOpponentPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.A7)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.A6)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.B6)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.B7)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.A5)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.C5)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.C7);
			var piece = board.GetPiece((int)SquareCode.A7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.A7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.A7, (int)SquareCode.A5),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.C5),
				new CongoMove((int)SquareCode.A7, (int)SquareCode.C7)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

	}

	[TestClass]
	public class Piece_CrocodileMoves_Test {

		[TestMethod]
		public void SkipFriendlyPiecesWater() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.D4)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.C4)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.C5)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.E5)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.D3);
			var piece = board.GetPiece((int)SquareCode.D4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D4, (int)SquareCode.D5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.F4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.G4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E3),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void CaptureOpponentPiecesWater() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.D4)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.C4)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.F4)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.C5)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.E5)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.D3);
			var piece = board.GetPiece((int)SquareCode.D4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.D5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.F4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E3),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.D3),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void SkipFriendlyPiecesGround() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.D7)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.C7)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.C6)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.E6)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.D4);
			var piece = board.GetPiece((int)SquareCode.D7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D7, (int)SquareCode.D6),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.E7),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.D5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void CaptureOpponentPiecesGround() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.D7)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.C7)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.C6)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.E6)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.D4);
			var piece = board.GetPiece((int)SquareCode.D7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D7, (int)SquareCode.C7),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.C6),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.D6),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.E6),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.E7),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.D5),
				new CongoMove((int)SquareCode.D7, (int)SquareCode.D4)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

	}

}
