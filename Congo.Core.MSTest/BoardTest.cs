using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Congo.Def;

namespace Congo.Core.MSTest {

	[TestClass]
	public class Board_PublicAPI_Test {

		[TestMethod]
		public void Board_EmptyBoardExists() {
			var board = CongoBoard.Empty;
			Assert.IsNotNull(board);
		}

		[TestMethod]
		public void Board_GetEmptyPieces() {
			var board = CongoBoard.Empty;
			var piece1 = board.GetPiece((int)SquareCode.A7);
			var piece2 = board.GetPiece((int)SquareCode.D3);
			Assert.IsTrue(
				piece1.GetType() == typeof(Empty) &&
				piece2.GetType() == typeof(Empty)
			);
		}

		[TestMethod]
		public void Board_EmptySquaresAreNotOccupied() {
			var board = CongoBoard.Empty;
			Assert.IsFalse(
				board.IsSquareOccupied((int)SquareCode.A1) ||
				board.IsSquareOccupied((int)SquareCode.B3) ||
				board.IsSquareOccupied((int)SquareCode.G3) ||
				board.IsSquareOccupied((int)SquareCode.G1)
			);
		}

		[TestMethod]
		public void Board_AddAndGetBlackLion() {
			var board = CongoBoard.Empty;
			board = board.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.A1);
			var piece = board.GetPiece((int)SquareCode.A1);
			Assert.IsTrue(piece.GetType() == typeof(Lion));
		}

		[TestMethod]
		public void Board_AddAndRemoveBlackLion() {
			var board = CongoBoard.Empty;
			board = board.With(ColorCode.Black, PieceCode.Lion, (int)SquareCode.A1);
			board = board.Without((int)SquareCode.A1);
			var piece = board.GetPiece((int)SquareCode.A1);
			Assert.IsTrue(piece.GetType() == typeof(Empty));
		}

		[TestMethod]
		public void Board_IsSquareWater() {
			var board = CongoBoard.Empty;
			var isWater = true;
			for (int i = 0; i < board.Size * 3; i++) {
				isWater &= !board.IsSquareWater(i);
			}
			for (int i = board.Size * 4; i < board.Size * board.Size; i++) {
				isWater &= !board.IsSquareWater(i);
			}
			var isNotWater = true;
			for (int i = board.Size * 3; i < board.Size * 4; i++) {
				isNotWater &= board.IsSquareWater(i);
			}
			Assert.IsTrue(isWater & isNotWater);
		}

	}

	[TestClass]
	public class Board_KingLeapGeneration_Test {

		[TestMethod]
		public void UpperBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.A7).Sort();
			var expected = new int[] {
				(int)SquareCode.B7, (int)SquareCode.A6, (int)SquareCode.B6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.D7).Sort();
			var expected = new int[] {
				(int)SquareCode.C7, (int)SquareCode.E7, (int)SquareCode.C6,
				(int)SquareCode.D6, (int)SquareCode.E6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.G7).Sort();
			var expected = new int[] {
				(int)SquareCode.F7, (int)SquareCode.F6, (int)SquareCode.G6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.A4).Sort();
			var expected = new int[] {
				(int)SquareCode.A5, (int)SquareCode.B5, (int)SquareCode.B4,
				(int)SquareCode.A3, (int)SquareCode.B3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.D4).Sort();
			var expected = new int[] {
				(int)SquareCode.C5, (int)SquareCode.D5, (int)SquareCode.E5,
				(int)SquareCode.C4, (int)SquareCode.E4, (int)SquareCode.C3,
				(int)SquareCode.D3, (int)SquareCode.E3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.G4).Sort();
			var expected = new int[] {
				(int)SquareCode.F5, (int)SquareCode.G5, (int)SquareCode.F4,
				(int)SquareCode.F3, (int)SquareCode.G3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.A1).Sort();
			var expected = new int[] {
				(int)SquareCode.A2, (int)SquareCode.B2, (int)SquareCode.B1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.D1).Sort();
			var expected = new int[] {
				(int)SquareCode.C2, (int)SquareCode.D2, (int)SquareCode.E2,
				(int)SquareCode.C1, (int)SquareCode.E1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKing((int)SquareCode.G1).Sort();
			var expected = new int[] {
				(int)SquareCode.F2, (int)SquareCode.G2, (int)SquareCode.F1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}
	
	[TestClass]
	public class Board_KnightLeapGeneration_Test {

		[TestMethod]
		public void UpperBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.A7).Sort();
			var expected = new int[] {
				(int)SquareCode.C6, (int)SquareCode.B5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.D7).Sort();
			var expected = new int[] {
				(int)SquareCode.B6, (int)SquareCode.C5, (int)SquareCode.E5,
				(int)SquareCode.F6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.G7).Sort();
			var expected = new int[] {
				(int)SquareCode.E6, (int)SquareCode.F5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.A4).Sort();
			var expected = new int[] {
				(int)SquareCode.B6, (int)SquareCode.C5, (int)SquareCode.C3,
				(int)SquareCode.B2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.D4).Sort();
			var expected = new int[] {
				(int)SquareCode.B5, (int)SquareCode.C6, (int)SquareCode.E6,
				(int)SquareCode.F5, (int)SquareCode.F3, (int)SquareCode.E2,
				(int)SquareCode.C2, (int)SquareCode.B3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.G4).Sort();
			var expected = new int[] {
				(int)SquareCode.F6, (int)SquareCode.E5, (int)SquareCode.E3,
				(int)SquareCode.F2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.A1).Sort();
			var expected = new int[] {
				(int)SquareCode.B3, (int)SquareCode.C2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.D1).Sort();
			var expected = new int[] {
				(int)SquareCode.B2, (int)SquareCode.C3, (int)SquareCode.E3,
				(int)SquareCode.F2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsKnight((int)SquareCode.G1).Sort();
			var expected = new int[] {
				(int)SquareCode.E2, (int)SquareCode.F3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}

	[TestClass]
	public class Board_ELephantLeapGeneration_Test {

		[TestMethod]
		public void UpperBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.A7).Sort();
			var expected = new int[] {
				(int)SquareCode.A6, (int)SquareCode.A5, (int)SquareCode.B7,
				(int)SquareCode.C7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.D7).Sort();
			var expected = new int[] {
				(int)SquareCode.B7, (int)SquareCode.C7, (int)SquareCode.E7,
				(int)SquareCode.F7, (int)SquareCode.D6, (int)SquareCode.D5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.G7).Sort();
			var expected = new int[] {
				(int)SquareCode.E7, (int)SquareCode.F7, (int)SquareCode.G6,
				(int)SquareCode.G5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.A4).Sort();
			var expected = new int[] {
				(int)SquareCode.A6, (int)SquareCode.A5, (int)SquareCode.B4,
				(int)SquareCode.C4, (int)SquareCode.A3, (int)SquareCode.A2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.D4).Sort();
			var expected = new int[] {
				(int)SquareCode.D6, (int)SquareCode.D5, (int)SquareCode.E4,
				(int)SquareCode.F4, (int)SquareCode.D3, (int)SquareCode.D2,
				(int)SquareCode.C4, (int)SquareCode.B4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.G4).Sort();
			var expected = new int[] {
				(int)SquareCode.G6, (int)SquareCode.G5, (int)SquareCode.G3,
				(int)SquareCode.G2, (int)SquareCode.F4, (int)SquareCode.E4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.A1).Sort();
			var expected = new int[] {
				(int)SquareCode.A2, (int)SquareCode.A3, (int)SquareCode.B1,
				(int)SquareCode.C1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.D1).Sort();
			var expected = new int[] {
				(int)SquareCode.B1, (int)SquareCode.C1, (int)SquareCode.D2,
				(int)SquareCode.D3, (int)SquareCode.E1, (int)SquareCode.F1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.G1).Sort();
			var expected = new int[] {
				(int)SquareCode.E1, (int)SquareCode.F1, (int)SquareCode.G2,
				(int)SquareCode.G3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperLeftCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.B6).Sort();
			var expected = new int[] {
				(int)SquareCode.A6, (int)SquareCode.B7, (int)SquareCode.C6,
				(int)SquareCode.D6, (int)SquareCode.B5, (int)SquareCode.B4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperRightCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.F6).Sort();
			var expected = new int[] {
				(int)SquareCode.F7, (int)SquareCode.D6, (int)SquareCode.E6,
				(int)SquareCode.G6, (int)SquareCode.F5, (int)SquareCode.F4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerLeftCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.B2).Sort();
			var expected = new int[] {
				(int)SquareCode.B4, (int)SquareCode.B3, (int)SquareCode.A2,
				(int)SquareCode.C2, (int)SquareCode.D2, (int)SquareCode.B1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerRightCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsElephant((int)SquareCode.F2).Sort();
			var expected = new int[] {
				(int)SquareCode.F4, (int)SquareCode.F3, (int)SquareCode.D2,
				(int)SquareCode.E2, (int)SquareCode.G2, (int)SquareCode.F1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}

	[TestClass]
	public class Board_CapturingGiraffeLeapGeneration_Test {

		[TestMethod]
		public void UpperBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.A7).Sort();
			var expected = new int[] {
				(int)SquareCode.A5, (int)SquareCode.C5, (int)SquareCode.C7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.D7).Sort();
			var expected = new int[] {
				(int)SquareCode.B7, (int)SquareCode.B5, (int)SquareCode.D5,
				(int)SquareCode.F5, (int)SquareCode.F7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.G7).Sort();
			var expected = new int[] {
				(int)SquareCode.E7, (int)SquareCode.E5, (int)SquareCode.G5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineLeftBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.A4).Sort();
			var expected = new int[] {
				(int)SquareCode.A6, (int)SquareCode.C6, (int)SquareCode.C4,
				(int)SquareCode.C2, (int)SquareCode.A2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.D4).Sort();
			var expected = new int[] {
				(int)SquareCode.B6, (int)SquareCode.D6, (int)SquareCode.F6,
				(int)SquareCode.F4, (int)SquareCode.F2, (int)SquareCode.D2,
				(int)SquareCode.B2, (int)SquareCode.B4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLineRightBorderLine() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.G4).Sort();
			var expected = new int[] {
				(int)SquareCode.G6, (int)SquareCode.E6, (int)SquareCode.E4,
				(int)SquareCode.E2, (int)SquareCode.G2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.A1).Sort();
			var expected = new int[] {
				(int)SquareCode.A3, (int)SquareCode.C3, (int)SquareCode.C1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.D1).Sort();
			var expected = new int[] {
				(int)SquareCode.B1, (int)SquareCode.B3, (int)SquareCode.D3,
				(int)SquareCode.F3, (int)SquareCode.F1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLineRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.G1).Sort();
			var expected = new int[] {
				(int)SquareCode.E1, (int)SquareCode.E3, (int)SquareCode.G3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperLeftCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.B6).Sort();
			var expected = new int[] {
				(int)SquareCode.B4, (int)SquareCode.D4, (int)SquareCode.D6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperRightCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.F6).Sort();
			var expected = new int[] {
				(int)SquareCode.F4, (int)SquareCode.D4, (int)SquareCode.D6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerLeftCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.B2).Sort();
			var expected = new int[] {
				(int)SquareCode.B4, (int)SquareCode.D4, (int)SquareCode.D2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerRightCornerInside() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCapturingGiraffe((int)SquareCode.F2).Sort();
			var expected = new int[] {
				(int)SquareCode.D2, (int)SquareCode.D4, (int)SquareCode.F4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}

	[TestClass]
	public class Board_CrocodileLeapGeneration_Test {

		[TestMethod]
		public void UpperBorderLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.A7).Sort();
			var expected = new int[] {
				(int)SquareCode.B6, (int)SquareCode.B7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.D7).Sort();
			var expected = new int[] {
				(int)SquareCode.C7, (int)SquareCode.C6, (int)SquareCode.E6,
				(int)SquareCode.E7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.G7).Sort();
			var expected = new int[] {
				(int)SquareCode.F7, (int)SquareCode.F6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void InsideBoardCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.D6).Sort();
			var expected = new int[] {
				(int)SquareCode.C7, (int)SquareCode.D7, (int)SquareCode.E7,
				(int)SquareCode.E6, (int)SquareCode.E5, (int)SquareCode.C5,
				(int)SquareCode.C6
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void WaterLeftBorder() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.A4).Sort();
			var expected = new int[] {
				(int)SquareCode.A5, (int)SquareCode.B5, (int)SquareCode.B3,
				(int)SquareCode.A3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void WaterCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.D4).Sort();
			var expected = new int[] {
				(int)SquareCode.C5, (int)SquareCode.D5, (int)SquareCode.E5,
				(int)SquareCode.C3, (int)SquareCode.D3, (int)SquareCode.E3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void WaterRightBorder() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.G4).Sort();
			var expected = new int[] {
				(int)SquareCode.F5, (int)SquareCode.G5, (int)SquareCode.F3,
				(int)SquareCode.G3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.A1).Sort();
			var expected = new int[] {
				(int)SquareCode.B2, (int)SquareCode.B1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.D1).Sort();
			var expected = new int[] {
				(int)SquareCode.C1, (int)SquareCode.C2, (int)SquareCode.E1,
				(int)SquareCode.E2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsCrocodile((int)SquareCode.G1).Sort();
			var expected = new int[] {
				(int)SquareCode.F1, (int)SquareCode.F2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}

	[TestClass]
	public class Board_WhiteLionLeapGeneration_Test {
		
		[TestMethod]
		public void OutsideCastleUpperPart() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.A1).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void OutsideCastleRiver() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.D4).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void OutsideCastleLowerPart() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.B3).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.C3).Sort();
			var expected = new int[] {
				(int)SquareCode.C2, (int)SquareCode.D2, (int)SquareCode.D3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.D3).Sort();
			var expected = new int[] {
				(int)SquareCode.C3, (int)SquareCode.C2, (int)SquareCode.D2,
				(int)SquareCode.E2, (int)SquareCode.E3
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.E3).Sort();
			var expected = new int[] {
				(int)SquareCode.D3, (int)SquareCode.D2, (int)SquareCode.E2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.C2).Sort();
			var expected = new int[] {
				(int)SquareCode.C3, (int)SquareCode.D3, (int)SquareCode.D2,
				(int)SquareCode.D1, (int)SquareCode.C1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.D2).Sort();
			var expected = new int[] {
				(int)SquareCode.C3, (int)SquareCode.D3, (int)SquareCode.E3,
				(int)SquareCode.C2, (int)SquareCode.E2, (int)SquareCode.C1,
				(int)SquareCode.D1, (int)SquareCode.E1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsLion(ColorCode.White, (int)SquareCode.E1).Sort();
			var expected = new int[] {
				(int)SquareCode.D1, (int)SquareCode.D2, (int)SquareCode.E2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}



	[TestClass]
	public class Board_WhitePawnLeapGeneration_Test {

		[TestMethod]
		public void UpperBorderLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.A7).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.D7).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.G7).Sort();
			var expected = new int[] { /* no leaps */
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLeftBorder() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.A4).Sort();
			var expected = new int[] {
				(int)SquareCode.A5, (int)SquareCode.B5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.D4).Sort();
			var expected = new int[] {
				(int)SquareCode.C5, (int)SquareCode.D5, (int)SquareCode.E5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleRightBorder() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.G4).Sort();
			var expected = new int[] {
				(int)SquareCode.F5, (int)SquareCode.G5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.A1).Sort();
			var expected = new int[] {
				(int)SquareCode.A2, (int)SquareCode.B2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.D1).Sort();
			var expected = new int[] {
				(int)SquareCode.C2, (int)SquareCode.D2, (int)SquareCode.E2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsPawn(ColorCode.White, (int)SquareCode.G1).Sort();
			var expected = new int[] {
				(int)SquareCode.F2, (int)SquareCode.G2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}



	[TestClass]
	public class Board_WhiteSuperpawnLeapGeneration_Test {

		[TestMethod]
		public void UpperBorderLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.A7).Sort();
			var expected = new int[] {
				(int)SquareCode.B7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.D7).Sort();
			var expected = new int[] {
				(int)SquareCode.C7, (int)SquareCode.E7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void UpperBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.G7).Sort();
			var expected = new int[] {
				(int)SquareCode.F7
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleLeftBorder() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.A4).Sort();
			var expected = new int[] {
				(int)SquareCode.A5, (int)SquareCode.B5, (int)SquareCode.B4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.D4).Sort();
			var expected = new int[] {
				(int)SquareCode.C4, (int)SquareCode.C5, (int)SquareCode.D5,
				(int)SquareCode.E5, (int)SquareCode.E4
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void MiddleRightBorder() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.G4).Sort();
			var expected = new int[] {
				(int)SquareCode.F4, (int)SquareCode.F5, (int)SquareCode.G5
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderLeftCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.A1).Sort();
			var expected = new int[] {
				(int)SquareCode.A2, (int)SquareCode.B2, (int)SquareCode.B1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderCenter() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.D1).Sort();
			var expected = new int[] {
				(int)SquareCode.C1, (int)SquareCode.C2, (int)SquareCode.D2,
				(int)SquareCode.E2, (int)SquareCode.E1
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

		[TestMethod]
		public void LowerBorderRightCorner() {
			var board = CongoBoard.Empty;
			var actual = board.LeapsAsSuperpawn(ColorCode.White, (int)SquareCode.G1).Sort();
			var expected = new int[] {
				(int)SquareCode.F1, (int)SquareCode.F2, (int)SquareCode.G2
			}.ToImmutableArray().Sort();
			CollectionAssert.AreEqual(actual, expected);
		}

	}



}
