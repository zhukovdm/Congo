using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
	[TestClass]
	public class GameTest
	{
		[TestMethod]
		public void Game_IsInvalid()
		{
			var board = CongoBoard.Empty;
			board = board.With(White.Color, Pawn.Piece, (int)Square.A1)
						 .With(Black.Color, Pawn.Piece, (int)Square.A2);
			var white = new Hi(White.Color, board, null);
			var black = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, white, black);
			Assert.IsTrue(game.IsInvalid());
		}

		[TestMethod]
		public void Game_IsDraw()
		{
			var board = CongoBoard.Empty;
			board = board.With(White.Color, Lion.Piece, (int)Square.A1)
						 .With(Black.Color, Lion.Piece, (int)Square.A7);
			var white = new Hi(White.Color, board, null);
			var black = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, white, black);
			Assert.IsTrue(game.IsDraw());
		}

		[TestMethod]
		public void Game_IsWin()
		{
			var board = CongoBoard.Empty;
			board = board.With(White.Color, Lion.Piece, (int)Square.A1)
						 .With(White.Color, Pawn.Piece, (int)Square.A2)
						 .With(Black.Color, Pawn.Piece, (int)Square.A7);
			var white = new Hi(White.Color, board, null);
			var black = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, white, black);
			Assert.IsTrue(game.IsWin());
		}

		[TestMethod]
		public void Game_MonkeyJumps()
		{
			
		}
	}
}
