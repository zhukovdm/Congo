using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
	[TestClass]
	public class Board_SupportingMethods_Test
	{
		[TestMethod]
		public void Board_EmptyBoardExists()
		{
			var board = CongoBoard.Empty;
			Assert.IsNotNull(board);
		}

		[TestMethod]
		public void Board_GetEmptyPieces()
		{
			var board = CongoBoard.Empty;
			var piece1 = board.GetPiece((int)SquareCode.a7);
			var piece2 = board.GetPiece((int)SquareCode.d3);
			Assert.IsTrue(
				piece1.GetType() == typeof(Empty) &&
				piece2.GetType() == typeof(Empty));
		}

		[TestMethod]
		public void Board_EmptySquaresAreNotOccupied()
		{
			var board = CongoBoard.Empty;
			Assert.IsFalse(
				board.IsOccupied((int)SquareCode.a1) ||
				board.IsOccupied((int)SquareCode.b3) ||
				board.IsOccupied((int)SquareCode.g3) ||
				board.IsOccupied((int)SquareCode.g1));
		}

		[TestMethod]
		public void Board_AddAndGetBlackLion()
		{
			var board = CongoBoard.Empty;
			board = board.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.a1);
			var piece = board.GetPiece((int)SquareCode.a1);
			Assert.IsTrue(piece.GetType() == typeof(Lion));
		}

		[TestMethod]
		public void Board_AddAndRemoveBlackLion()
		{
			var board = CongoBoard.Empty;
			board = board.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.a1);
			board = board.Without((int)SquareCode.a1);
			var piece = board.GetPiece((int)SquareCode.a1);
			Assert.IsTrue(piece.GetType() == typeof(Empty));
		}

		[TestMethod]
		public void Board_IsSquareWater()
		{
			var board = CongoBoard.Empty;
			var isWater = true;
			for (int i = 0; i < board.Size * 3; i++) {
				isWater &= !board.IsRiver(i);
			}
			for (int i = board.Size * 4; i < board.Size * board.Size; i++) {
				isWater &= !board.IsRiver(i);
			}
			var isNotWater = true;
			for (int i = board.Size * 3; i < board.Size * 4; i++) {
				isNotWater &= board.IsRiver(i);
			}
			Assert.IsTrue(isWater & isNotWater);
		}
	}

	[TestClass]
	public class Board_KingLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.b7, (int)SquareCode.a6, (int)SquareCode.b6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.e7, (int)SquareCode.c6,
				(int)SquareCode.d6, (int)SquareCode.e6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.f7, (int)SquareCode.f6, (int)SquareCode.g6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a5, (int)SquareCode.b5, (int)SquareCode.b4,
				(int)SquareCode.a3, (int)SquareCode.b3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.c5, (int)SquareCode.d5, (int)SquareCode.e5,
				(int)SquareCode.c4, (int)SquareCode.e4, (int)SquareCode.c3,
				(int)SquareCode.d3, (int)SquareCode.e3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.f5, (int)SquareCode.g5, (int)SquareCode.f4,
				(int)SquareCode.f3, (int)SquareCode.g3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.a2, (int)SquareCode.b2, (int)SquareCode.b1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.c2, (int)SquareCode.d2, (int)SquareCode.e2,
				(int)SquareCode.c1, (int)SquareCode.e1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.f2, (int)SquareCode.g2, (int)SquareCode.f1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_KnightLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.c6, (int)SquareCode.b5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.b6, (int)SquareCode.c5, (int)SquareCode.e5,
				(int)SquareCode.f6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.e6, (int)SquareCode.f5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.b6, (int)SquareCode.c5, (int)SquareCode.c3,
				(int)SquareCode.b2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.b5, (int)SquareCode.c6, (int)SquareCode.e6,
				(int)SquareCode.f5, (int)SquareCode.f3, (int)SquareCode.e2,
				(int)SquareCode.c2, (int)SquareCode.b3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.f6, (int)SquareCode.e5, (int)SquareCode.e3,
				(int)SquareCode.f2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.b3, (int)SquareCode.c2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.b2, (int)SquareCode.c3, (int)SquareCode.e3,
				(int)SquareCode.f2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.e2, (int)SquareCode.f3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_ElephantLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.a6, (int)SquareCode.a5, (int)SquareCode.b7,
				(int)SquareCode.c7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.b7, (int)SquareCode.c7, (int)SquareCode.e7,
				(int)SquareCode.f7, (int)SquareCode.d6, (int)SquareCode.d5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.e7, (int)SquareCode.f7, (int)SquareCode.g6,
				(int)SquareCode.g5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a6, (int)SquareCode.a5, (int)SquareCode.b4,
				(int)SquareCode.c4, (int)SquareCode.a3, (int)SquareCode.a2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.d6, (int)SquareCode.d5, (int)SquareCode.e4,
				(int)SquareCode.f4, (int)SquareCode.d3, (int)SquareCode.d2,
				(int)SquareCode.c4, (int)SquareCode.b4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.g6, (int)SquareCode.g5, (int)SquareCode.g3,
				(int)SquareCode.g2, (int)SquareCode.f4, (int)SquareCode.e4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.a2, (int)SquareCode.a3, (int)SquareCode.b1,
				(int)SquareCode.c1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.b1, (int)SquareCode.c1, (int)SquareCode.d2,
				(int)SquareCode.d3, (int)SquareCode.e1, (int)SquareCode.f1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.e1, (int)SquareCode.f1, (int)SquareCode.g2,
				(int)SquareCode.g3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperLeftCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.b6).Sort();
			var expected = new int[] {
				(int)SquareCode.a6, (int)SquareCode.b7, (int)SquareCode.c6,
				(int)SquareCode.d6, (int)SquareCode.b5, (int)SquareCode.b4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperRightCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.f6).Sort();
			var expected = new int[] {
				(int)SquareCode.f7, (int)SquareCode.d6, (int)SquareCode.e6,
				(int)SquareCode.g6, (int)SquareCode.f5, (int)SquareCode.f4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerLeftCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.b2).Sort();
			var expected = new int[] {
				(int)SquareCode.b4, (int)SquareCode.b3, (int)SquareCode.a2,
				(int)SquareCode.c2, (int)SquareCode.d2, (int)SquareCode.b1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerRightCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.f2).Sort();
			var expected = new int[] {
				(int)SquareCode.f4, (int)SquareCode.f3, (int)SquareCode.d2,
				(int)SquareCode.e2, (int)SquareCode.g2, (int)SquareCode.f1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_CapturingGiraffeLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.a5, (int)SquareCode.c5, (int)SquareCode.c7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.b7, (int)SquareCode.b5, (int)SquareCode.d5,
				(int)SquareCode.f5, (int)SquareCode.f7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.e7, (int)SquareCode.e5, (int)SquareCode.g5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a6, (int)SquareCode.c6, (int)SquareCode.c4,
				(int)SquareCode.c2, (int)SquareCode.a2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.b6, (int)SquareCode.d6, (int)SquareCode.f6,
				(int)SquareCode.f4, (int)SquareCode.f2, (int)SquareCode.d2,
				(int)SquareCode.b2, (int)SquareCode.b4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.g6, (int)SquareCode.e6, (int)SquareCode.e4,
				(int)SquareCode.e2, (int)SquareCode.g2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.a3, (int)SquareCode.c3, (int)SquareCode.c1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.b1, (int)SquareCode.b3, (int)SquareCode.d3,
				(int)SquareCode.f3, (int)SquareCode.f1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.e1, (int)SquareCode.e3, (int)SquareCode.g3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperLeftCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.b6).Sort();
			var expected = new int[] {
				(int)SquareCode.b4, (int)SquareCode.d4, (int)SquareCode.d6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperRightCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.f6).Sort();
			var expected = new int[] {
				(int)SquareCode.f4, (int)SquareCode.d4, (int)SquareCode.d6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerLeftCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.b2).Sort();
			var expected = new int[] {
				(int)SquareCode.b4, (int)SquareCode.d4, (int)SquareCode.d2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerRightCornerInside()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.f2).Sort();
			var expected = new int[] {
				(int)SquareCode.d2, (int)SquareCode.d4, (int)SquareCode.f4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_CrocodileLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.b6, (int)SquareCode.b7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.c6, (int)SquareCode.e6,
				(int)SquareCode.e7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.f7, (int)SquareCode.f6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void InsideBoardCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.d6).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.d7, (int)SquareCode.e7,
				(int)SquareCode.e6, (int)SquareCode.e5, (int)SquareCode.c5,
				(int)SquareCode.c6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void WaterLeftBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a5, (int)SquareCode.b5, (int)SquareCode.b3,
				(int)SquareCode.a3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void WaterCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.c5, (int)SquareCode.d5, (int)SquareCode.e5,
				(int)SquareCode.c3, (int)SquareCode.d3, (int)SquareCode.e3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void WaterRightBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.f5, (int)SquareCode.g5, (int)SquareCode.f3,
				(int)SquareCode.g3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.b2, (int)SquareCode.b1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.c1, (int)SquareCode.c2, (int)SquareCode.e1,
				(int)SquareCode.e2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.f1, (int)SquareCode.f2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_WhitePawnLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.a7).Sort();
			var expected = new int[] {
				/* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.d7).Sort();
			var expected = new int[] {
				/* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.g7).Sort();
			var expected = new int[] {
				/* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLeftBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a5, (int)SquareCode.b5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.c5, (int)SquareCode.d5, (int)SquareCode.e5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleRightBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.f5, (int)SquareCode.g5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.a2, (int)SquareCode.b2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.c2, (int)SquareCode.d2, (int)SquareCode.e2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.f2, (int)SquareCode.g2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_BlackPawnLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.a6, (int)SquareCode.b6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.c6, (int)SquareCode.d6, (int)SquareCode.e6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.f6, (int)SquareCode.g6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLeftBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a3, (int)SquareCode.b3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.c3, (int)SquareCode.d3, (int)SquareCode.e3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleRightBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.f3, (int)SquareCode.g3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.a1).Sort();
			var expected = new int[] {
				/* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.d1).Sort();
			var expected = new int[] {
				/* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.Black, (int)SquareCode.g1).Sort();
			var expected = new int[] {
				/* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_WhiteSuperpawnLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.b7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.e7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.f7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLeftBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a5, (int)SquareCode.b5, (int)SquareCode.b4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.c4, (int)SquareCode.c5, (int)SquareCode.d5,
				(int)SquareCode.e5, (int)SquareCode.e4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleRightBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.f4, (int)SquareCode.f5, (int)SquareCode.g5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.a2, (int)SquareCode.b2, (int)SquareCode.b1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.c1, (int)SquareCode.c2, (int)SquareCode.d2,
				(int)SquareCode.e2, (int)SquareCode.e1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.f1, (int)SquareCode.f2, (int)SquareCode.g2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_BlackSuperpawnLeapGeneration_Test
	{
		[TestMethod]
		public void UpperBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.a7).Sort();
			var expected = new int[] {
				(int)SquareCode.a6, (int)SquareCode.b6, (int)SquareCode.b7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.c6, (int)SquareCode.d6,
				(int)SquareCode.e6, (int)SquareCode.e7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.g7).Sort();
			var expected = new int[] {
				(int)SquareCode.f7, (int)SquareCode.f6, (int)SquareCode.g6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleLeftBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.a4).Sort();
			var expected = new int[] {
				(int)SquareCode.a3, (int)SquareCode.b3, (int)SquareCode.b4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.d4).Sort();
			var expected = new int[] {
				(int)SquareCode.c4, (int)SquareCode.c3, (int)SquareCode.d3,
				(int)SquareCode.e3, (int)SquareCode.e4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleRightBorder()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.g4).Sort();
			var expected = new int[] {
				(int)SquareCode.f4, (int)SquareCode.f3, (int)SquareCode.g3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.a1).Sort();
			var expected = new int[] {
				(int)SquareCode.b1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.d1).Sort();
			var expected = new int[] {
				(int)SquareCode.c1, (int)SquareCode.e1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.Black, (int)SquareCode.g1).Sort();
			var expected = new int[] {
				(int)SquareCode.f1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_WhiteLionLeapGeneration_Test
	{
		[TestMethod]
		public void OutsideCastleUpperPart()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.d6).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void OutsideCastleRiver()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.d4).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void OutsideCastleLowerPart()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.b3).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.c3).Sort();
			var expected = new int[] {
				(int)SquareCode.c2, (int)SquareCode.d2, (int)SquareCode.d3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.d3).Sort();
			var expected = new int[] {
				(int)SquareCode.c3, (int)SquareCode.c2, (int)SquareCode.d2,
				(int)SquareCode.e2, (int)SquareCode.e3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.e3).Sort();
			var expected = new int[] {
				(int)SquareCode.d3, (int)SquareCode.d2, (int)SquareCode.e2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.c2).Sort();
			var expected = new int[] {
				(int)SquareCode.c3, (int)SquareCode.d3, (int)SquareCode.d2,
				(int)SquareCode.d1, (int)SquareCode.c1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.d2).Sort();
			var expected = new int[] {
				(int)SquareCode.c3, (int)SquareCode.d3, (int)SquareCode.e3,
				(int)SquareCode.c2, (int)SquareCode.e2, (int)SquareCode.c1,
				(int)SquareCode.d1, (int)SquareCode.e1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.e1).Sort();
			var expected = new int[] {
				(int)SquareCode.d1, (int)SquareCode.d2, (int)SquareCode.e2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}

	[TestClass]
	public class Board_BlackLionLeapGeneration_Test
	{
		[TestMethod]
		public void OutsideCastleUpperPart()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.a1).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void OutsideCastleRiver()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.d4).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void OutsideCastleLowerPart()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.d2).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderLeftCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.c7).Sort();
			var expected = new int[] {
				(int)SquareCode.c6, (int)SquareCode.d6, (int)SquareCode.d7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.d7).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.c6, (int)SquareCode.d6,
				(int)SquareCode.e6, (int)SquareCode.e7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpperBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.e7).Sort();
			var expected = new int[] {
				(int)SquareCode.d7, (int)SquareCode.d6, (int)SquareCode.e6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.c6).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.d7, (int)SquareCode.d6,
				(int)SquareCode.d5, (int)SquareCode.c5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void MiddleCenter()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.d6).Sort();
			var expected = new int[] {
				(int)SquareCode.c7, (int)SquareCode.d7, (int)SquareCode.e7,
				(int)SquareCode.c6, (int)SquareCode.e6, (int)SquareCode.c5,
				(int)SquareCode.d5, (int)SquareCode.e5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LowerBorderRightCorner()
		{
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.Black, (int)SquareCode.e5).Sort();
			var expected = new int[] {
				(int)SquareCode.d5, (int)SquareCode.d6, (int)SquareCode.e6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(expected, actual);
		}
	}
}
