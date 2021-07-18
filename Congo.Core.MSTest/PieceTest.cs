using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{

	[TestClass]
	public class Piece_ElephantMoves_Test
	{
		[TestMethod]
		public void SkipFriendlyPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Elephant, (int)SquareCode.a7)
				.With(ColorCode.White, PieceCode.Elephant, (int)SquareCode.b7);
			var piece = board.GetPiece((int)SquareCode.a7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.a7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.a7, (int)SquareCode.c7),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.a6),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.a5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void CaptureOpponentPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Elephant, (int)SquareCode.a7)
				.With(ColorCode.Black, PieceCode.Elephant, (int)SquareCode.b7)
				.With(ColorCode.Black, PieceCode.Elephant, (int)SquareCode.c7);
			var piece = board.GetPiece((int)SquareCode.a7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.a7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.a7, (int)SquareCode.c7),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.b7),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.a6),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.a5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}

	[TestClass]
	public class Piece_ZebraMoves_Test
	{
		[TestMethod]
		public void SkipFriendlyPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.d4)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.e6)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.f5)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.b3)
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.e2);
			var piece = board.GetPiece((int)SquareCode.d4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d4, (int)SquareCode.f3),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c2),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c6),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.b5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void CaptureOpponentPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Zebra, (int)SquareCode.d7)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.c5)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.b6)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.f6)
				.With(ColorCode.Black, PieceCode.Zebra, (int)SquareCode.e5);
			var piece = board.GetPiece((int)SquareCode.d7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d7, (int)SquareCode.b6),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.c5),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.e5),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.f6)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}

	[TestClass]
	public class Piece_GiraffeMoves_Test
	{
		[TestMethod]
		public void SkipFriendlyPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.a7)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.a6)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.b7)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.a5)
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.c5);
			var piece = board.GetPiece((int)SquareCode.a7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.a7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.a7, (int)SquareCode.b6),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.c7)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void CaptureAndSkipOpponentPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Giraffe, (int)SquareCode.a7)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.a6)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.b6)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.b7)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.a5)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.c5)
				.With(ColorCode.Black, PieceCode.Giraffe, (int)SquareCode.c7);
			var piece = board.GetPiece((int)SquareCode.a7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.a7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.a7, (int)SquareCode.a5),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.c5),
				new CongoMove((int)SquareCode.a7, (int)SquareCode.c7)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}

	[TestClass]
	public class Piece_CrocodileMoves_Test
	{
		[TestMethod]
		public void SkipFriendlyPiecesWater()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.d4)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.c4)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.c5)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.e5)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.d3);
			var piece = board.GetPiece((int)SquareCode.d4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.f4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.g4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e3),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void CaptureOpponentPiecesWater()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.d4)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.c4)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.f4)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.c5)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.e5)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.d3);
			var piece = board.GetPiece((int)SquareCode.d4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.f4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e3),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d3),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void SkipFriendlyPiecesGround()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.d7)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.c7)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.c6)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.e6)
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.d4);
			var piece = board.GetPiece((int)SquareCode.d7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d7, (int)SquareCode.d6),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.e7),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.d5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void CaptureOpponentPiecesGround()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Crocodile, (int)SquareCode.d7)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.c7)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.c6)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.e6)
				.With(ColorCode.Black, PieceCode.Crocodile, (int)SquareCode.d4);
			var piece = board.GetPiece((int)SquareCode.d7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d7, (int)SquareCode.c7),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.c6),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.d6),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.e6),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.e7),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.d5),
				new CongoMove((int)SquareCode.d7, (int)SquareCode.d4)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}

	[TestClass]
	public class Piece_WhitePawnMoves_Test
	{
		[TestMethod]
		public void BelowRiverNoBackwardSlide()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.g3);
			var piece = board.GetPiece((int)SquareCode.g3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.g3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.g3, (int)SquareCode.f4),
				new CongoMove((int)SquareCode.g3, (int)SquareCode.g4)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void RiverNoBackwardSlide()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d4);
			var piece = board.GetPiece((int)SquareCode.d4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void AboveRiverBackwardSlide()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.b5);
			var piece = board.GetPiece((int)SquareCode.b5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.b5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.b5, (int)SquareCode.a6),
				new CongoMove((int)SquareCode.b5, (int)SquareCode.b6),
				new CongoMove((int)SquareCode.b5, (int)SquareCode.c6),
				new CongoMove((int)SquareCode.b5, (int)SquareCode.b4),
				new CongoMove((int)SquareCode.b5, (int)SquareCode.b3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipFriendlyPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d5)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d3);
			var piece = board.GetPiece((int)SquareCode.d5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d5, (int)SquareCode.c6),
				new CongoMove((int)SquareCode.d5, (int)SquareCode.d6),
				new CongoMove((int)SquareCode.d5, (int)SquareCode.e6),
				new CongoMove((int)SquareCode.d5, (int)SquareCode.d4)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipOpponentPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d5)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.d4);
			var piece = board.GetPiece((int)SquareCode.d5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d5, (int)SquareCode.c6),
				new CongoMove((int)SquareCode.d5, (int)SquareCode.d6),
				new CongoMove((int)SquareCode.d5, (int)SquareCode.e6)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}

	[TestClass]
	public class Piece_WhiteSuperpawnMoves_Test
	{
		[TestMethod]
		public void BelowRiverBackwardSlide()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.d2);
			var piece = board.GetPiece((int)SquareCode.d2);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d2);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d2, (int)SquareCode.c2),
				new CongoMove((int)SquareCode.d2, (int)SquareCode.c3),
				new CongoMove((int)SquareCode.d2, (int)SquareCode.d3),
				new CongoMove((int)SquareCode.d2, (int)SquareCode.e3),
				new CongoMove((int)SquareCode.d2, (int)SquareCode.e2),
				new CongoMove((int)SquareCode.d2, (int)SquareCode.e1),
				new CongoMove((int)SquareCode.d2, (int)SquareCode.d1),
				new CongoMove((int)SquareCode.d2, (int)SquareCode.c1)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void RiverBackwardSlide()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.a4);
			var piece = board.GetPiece((int)SquareCode.a4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.a4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.a4, (int)SquareCode.a5),
				new CongoMove((int)SquareCode.a4, (int)SquareCode.b5),
				new CongoMove((int)SquareCode.a4, (int)SquareCode.b4),
				new CongoMove((int)SquareCode.a4, (int)SquareCode.a3),
				new CongoMove((int)SquareCode.a4, (int)SquareCode.a2),
				new CongoMove((int)SquareCode.a4, (int)SquareCode.b3),
				new CongoMove((int)SquareCode.a4, (int)SquareCode.c2)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void AboveRiverBackwardSlide()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.f7);
			var piece = board.GetPiece((int)SquareCode.f7);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.f7);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.f7, (int)SquareCode.e7),
				new CongoMove((int)SquareCode.f7, (int)SquareCode.e6),
				new CongoMove((int)SquareCode.f7, (int)SquareCode.f6),
				new CongoMove((int)SquareCode.f7, (int)SquareCode.g6),
				new CongoMove((int)SquareCode.f7, (int)SquareCode.g7),
				new CongoMove((int)SquareCode.f7, (int)SquareCode.d5),
				new CongoMove((int)SquareCode.f7, (int)SquareCode.f5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipFriendlyPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.d4)
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.b2)
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.d3)
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.f2);
			var piece = board.GetPiece((int)SquareCode.d4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e3),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BackwardSlideSkipOpponentPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Superpawn, (int)SquareCode.d4)
				.With(ColorCode.Black, PieceCode.Superpawn, (int)SquareCode.b2)
				.With(ColorCode.Black, PieceCode.Superpawn, (int)SquareCode.d3)
				.With(ColorCode.Black, PieceCode.Superpawn, (int)SquareCode.f2);
			var piece = board.GetPiece((int)SquareCode.d4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e3),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}

	[TestClass]
	public class Piece_WhiteLionMoves_Test
	{
		[TestMethod]
		public void VerticalJump()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.d1)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.d6);
			var piece = board.GetPiece((int)SquareCode.d1);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d1);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d1, (int)SquareCode.c1),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.c2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.e2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.e1),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.d6)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpByFriendlyPiece()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.d1)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d5)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.d6);
			var piece = board.GetPiece((int)SquareCode.d1);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d1);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d1, (int)SquareCode.c1),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.c2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.e2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.e1)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpByOpponentPiece()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.d1)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.d4)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.d6);
			var piece = board.GetPiece((int)SquareCode.d1);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d1);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d1, (int)SquareCode.c1),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.c2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.e2),
				new CongoMove((int)SquareCode.d1, (int)SquareCode.e1)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpOutsideCastle()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.b3)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.b5);
			var piece = board.GetPiece((int)SquareCode.b3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.b3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				/* no moves */
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BlockedVerticalJumpWrongRiverSideInsideCastle()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.d5)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.d3);
			var piece = board.GetPiece((int)SquareCode.d5);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d5);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				/* no moves */
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void DiagonalJump_C3_E5()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.c3)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.e5);
			var piece = board.GetPiece((int)SquareCode.c3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.c3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.c3, (int)SquareCode.d3),
				new CongoMove((int)SquareCode.c3, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.c3, (int)SquareCode.c2),
				new CongoMove((int)SquareCode.c3, (int)SquareCode.e5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void DiagonalJump_E3_C5()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.e3)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.c5);
			var piece = board.GetPiece((int)SquareCode.e3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.e3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.e3, (int)SquareCode.d3),
				new CongoMove((int)SquareCode.e3, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.e3, (int)SquareCode.e2),
				new CongoMove((int)SquareCode.e3, (int)SquareCode.c5)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BlockedDiagonalJumpByFriendlyPiece()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.c3)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d4)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.e5);
			var piece = board.GetPiece((int)SquareCode.c3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.c3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.c3, (int)SquareCode.d3),
				new CongoMove((int)SquareCode.c3, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.c3, (int)SquareCode.c2)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BlockedDiagonalJumpByOpponentPiece()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Lion, (int)SquareCode.e3)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d4)
				.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.c5);
			var piece = board.GetPiece((int)SquareCode.e3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.e3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.e3, (int)SquareCode.d3),
				new CongoMove((int)SquareCode.e3, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.e3, (int)SquareCode.e2)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}

	[TestClass]
	public class Piece_MonkeyMoves_Test
	{
		[TestMethod]
		public void SkipFriendlyPieces()
		{
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Monkey, (int)SquareCode.d3)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.c3)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.d4)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.e4)
				.With(ColorCode.White, PieceCode.Pawn, (int)SquareCode.e2);
			var piece = board.GetPiece((int)SquareCode.d3);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d3);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d3, (int)SquareCode.c4),
				new CongoMove((int)SquareCode.d3, (int)SquareCode.e3),
				new CongoMove((int)SquareCode.d3, (int)SquareCode.c2),
				new CongoMove((int)SquareCode.d3, (int)SquareCode.d2)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void JumpOverOpponentPieces() {
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Monkey, (int)SquareCode.d4)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.c3)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.c4)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.d5)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.e3);
			var piece = board.GetPiece((int)SquareCode.d4);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.d4);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.d4, (int)SquareCode.b2),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.b4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.c5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d6),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e5),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.e4),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.f2),
				new CongoMove((int)SquareCode.d4, (int)SquareCode.d3)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}

		[TestMethod]
		public void BoundaryJumpOverOpponentPieces() {
			var comparer = new CongoMoveGenComparer();
			var board = CongoBoard.Empty
				.With(ColorCode.White, PieceCode.Monkey, (int)SquareCode.f2)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.e1)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.e2)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.e3)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.f3)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.g3)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.g2)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.g1)
				.With(ColorCode.Black, PieceCode.Pawn, (int)SquareCode.f1);
			var piece = board.GetPiece((int)SquareCode.f2);
			var actual = piece.GetMoves(ColorCode.White, board, (int)SquareCode.f2);
			actual.Sort(comparer);
			var expected = new List<CongoMove>() {
				new CongoMove((int)SquareCode.f2, (int)SquareCode.d2),
				new CongoMove((int)SquareCode.f2, (int)SquareCode.d4),
				new CongoMove((int)SquareCode.f2, (int)SquareCode.f4)
			};
			expected.Sort(comparer);
			CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
		}
	}
}
