using Microsoft.VisualStudio.TestTools.UnitTesting;

using Congo.Def;

namespace Congo.Core.MSTest {

	[TestClass]
	public class BoardTest {

		[TestMethod]
		public void Board_EmptyBoardExists() {
			var board = CongoBoard.Empty;
			Assert.IsNotNull(board);
		}

		[TestMethod]
		public void Board_GetEmptyPieces() {
			var board = CongoBoard.Empty;
			var piece1 = board.GetPiece(1, 6);
			var piece2 = board.GetPiece(4, 2);
			Assert.IsTrue(
				piece1.GetType() == typeof(Empty) &&
				piece2.GetType() == typeof(Empty)
			);
		}

		[TestMethod]
		public void Board_EmptySquaresAreNotOccupied() {
			var board = CongoBoard.Empty;
			Assert.IsFalse(
				board.IsSquareOccupied(0, 0) ||
				board.IsSquareOccupied(1, 2) ||
				board.IsSquareOccupied(4, 5) ||
				board.IsSquareOccupied(6, 6)
			);
		}

		[TestMethod]
		public void Board_AddAndGetBlackLion() {
			var board = CongoBoard.Empty;
			board = board.With(
				ColorCode.Black, PieceCode.Lion, 0, 0
			);
			var piece = board.GetPiece(0, 0);
			Assert.IsTrue(
				piece.GetType() == typeof(Lion)
			);
		}

		[TestMethod]
		public void Board_AddAndRemoveBlackLion() {
			var board = CongoBoard.Empty;
			board = board.With(
				ColorCode.Black, PieceCode.Lion, 0, 0
			);
			board = board.Without(0, 0);
			var piece = board.GetPiece(0, 0);
			Assert.IsTrue(
				piece.GetType() == typeof(Empty)
			);
		}

	}
}
