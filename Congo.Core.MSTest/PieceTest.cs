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

	[TestClass]
	public class Piece_WhitePawnMoves_Test {

		[TestMethod]
		public void BelowRiverNoBackwardSlide() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.G3);
			var piece = board.GetPiece((int)SquareCode.G3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.G3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.G3, (int)SquareCode.F4),
				new CongoMove((int)SquareCode.G3, (int)SquareCode.G4)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void RiverNoBackwardSlide() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.D4);
			var piece = board.GetPiece((int)SquareCode.D4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.D5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void AboveRiverBackwardSlide() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.B5);
			var piece = board.GetPiece((int)SquareCode.B5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.B5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.B5, (int)SquareCode.A6),
				new CongoMove((int)SquareCode.B5, (int)SquareCode.B6),
				new CongoMove((int)SquareCode.B5, (int)SquareCode.C6),
				new CongoMove((int)SquareCode.B5, (int)SquareCode.B4),
				new CongoMove((int)SquareCode.B5, (int)SquareCode.B3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipFriendlyPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.D5)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.D3);
			var piece = board.GetPiece((int)SquareCode.D5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D5, (int)SquareCode.C6),
				new CongoMove((int)SquareCode.D5, (int)SquareCode.D6),
				new CongoMove((int)SquareCode.D5, (int)SquareCode.E6),
				new CongoMove((int)SquareCode.D5, (int)SquareCode.D4)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipOpponentPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.D5)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.D4);
			var piece = board.GetPiece((int)SquareCode.D5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D5, (int)SquareCode.C6),
				new CongoMove((int)SquareCode.D5, (int)SquareCode.D6),
				new CongoMove((int)SquareCode.D5, (int)SquareCode.E6)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

	}

	[TestClass]
	public class Piece_WhiteSuperpawnMoves_Test {

		[TestMethod]
		public void BelowRiverBackwardSlide() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.D2);
			var piece = board.GetPiece((int)SquareCode.D2);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D2);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D2, (int)SquareCode.C2),
				new CongoMove((int)SquareCode.D2, (int)SquareCode.C3),
				new CongoMove((int)SquareCode.D2, (int)SquareCode.D3),
				new CongoMove((int)SquareCode.D2, (int)SquareCode.E3),
				new CongoMove((int)SquareCode.D2, (int)SquareCode.E2),
				new CongoMove((int)SquareCode.D2, (int)SquareCode.E1),
				new CongoMove((int)SquareCode.D2, (int)SquareCode.D1),
				new CongoMove((int)SquareCode.D2, (int)SquareCode.C1)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void RiverBackwardSlide() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.A4);
			var piece = board.GetPiece((int)SquareCode.A4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.A4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.A4, (int)SquareCode.A5),
				new CongoMove((int)SquareCode.A4, (int)SquareCode.B5),
				new CongoMove((int)SquareCode.A4, (int)SquareCode.B4),
				new CongoMove((int)SquareCode.A4, (int)SquareCode.A3),
				new CongoMove((int)SquareCode.A4, (int)SquareCode.A2),
				new CongoMove((int)SquareCode.A4, (int)SquareCode.B3),
				new CongoMove((int)SquareCode.A4, (int)SquareCode.C2)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void AboveRiverBackwardSlide() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.F7);
			var piece = board.GetPiece((int)SquareCode.F7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.F7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.F7, (int)SquareCode.E7),
				new CongoMove((int)SquareCode.F7, (int)SquareCode.E6),
				new CongoMove((int)SquareCode.F7, (int)SquareCode.F6),
				new CongoMove((int)SquareCode.F7, (int)SquareCode.G6),
				new CongoMove((int)SquareCode.F7, (int)SquareCode.G7),
				new CongoMove((int)SquareCode.F7, (int)SquareCode.D5),
				new CongoMove((int)SquareCode.F7, (int)SquareCode.F5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipFriendlyPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.D4)
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.B2)
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.D3)
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.F2);
			var piece = board.GetPiece((int)SquareCode.D4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.D5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E3),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipOpponentPieces() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.D4)
				.With(ColorCode.Black, PieceCode.Superpawn, (int)SquareCode.B2)
				.With(ColorCode.Black, PieceCode.Superpawn, (int)SquareCode.D3)
				.With(ColorCode.Black, PieceCode.Superpawn, (int)SquareCode.F2);
			var piece = board.GetPiece((int)SquareCode.D4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.D5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E5),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E4),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.E3),
				new CongoMove((int)SquareCode.D4, (int)SquareCode.C3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

	}

	[TestClass]
	public class Piece_WhiteLionMoves_Test {

		[TestMethod]
		public void VerticalJump() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.D1)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.D6);
			var piece = board.GetPiece((int)SquareCode.D1);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D1);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D1, (int)SquareCode.C1),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.C2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.D2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.E2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.E1),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.D6)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpByFriendlyPiece() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.D1)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.D5)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.D6);
			var piece = board.GetPiece((int)SquareCode.D1);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D1);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D1, (int)SquareCode.C1),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.C2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.D2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.E2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.E1)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpByOpponentPiece() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.D1)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.D4)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.D6);
			var piece = board.GetPiece((int)SquareCode.D1);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D1);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.D1, (int)SquareCode.C1),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.C2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.D2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.E2),
				new CongoMove((int)SquareCode.D1, (int)SquareCode.E1)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpOutsideCastle() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.B3)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.B5);
			var piece = board.GetPiece((int)SquareCode.B3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.B3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				/* no moves */
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpWrongRiverSideInsideCastle() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.D5)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.D3);
			var piece = board.GetPiece((int)SquareCode.D5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.D5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				/* no moves */
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void DiagonalJump_C3_E5() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.C3)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.E5);
			var piece = board.GetPiece((int)SquareCode.C3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.C3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.C3, (int)SquareCode.D3),
				new CongoMove((int)SquareCode.C3, (int)SquareCode.D2),
				new CongoMove((int)SquareCode.C3, (int)SquareCode.C2),
				new CongoMove((int)SquareCode.C3, (int)SquareCode.E5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void DiagonalJump_E3_C5() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.E3)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.C5);
			var piece = board.GetPiece((int)SquareCode.E3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.E3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.E3, (int)SquareCode.D3),
				new CongoMove((int)SquareCode.E3, (int)SquareCode.D2),
				new CongoMove((int)SquareCode.E3, (int)SquareCode.E2),
				new CongoMove((int)SquareCode.E3, (int)SquareCode.C5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BlockedDiagonalJumpByFriendlyPiece() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.C3)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.D4)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.E5);
			var piece = board.GetPiece((int)SquareCode.C3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.C3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.C3, (int)SquareCode.D3),
				new CongoMove((int)SquareCode.C3, (int)SquareCode.D2),
				new CongoMove((int)SquareCode.C3, (int)SquareCode.C2)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

		[TestMethod]
		public void BlockedDiagonalJumpByOpponentPiece() {
			var comparer = new CongoMoveComparerGeneric();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.E3)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.D4)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.C5);
			var piece = board.GetPiece((int)SquareCode.E3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.E3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.E3, (int)SquareCode.D3),
				new CongoMove((int)SquareCode.E3, (int)SquareCode.D2),
				new CongoMove((int)SquareCode.E3, (int)SquareCode.E2)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveComparer());
		}

	}

}
