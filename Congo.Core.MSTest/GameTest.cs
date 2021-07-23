using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
	[TestClass]
	public class GameTest
	{
		[TestMethod]
		public void Game_IsDraw()
		{
			var board = CongoBoard.Empty;
			board = board.With(White.Color, Lion.Piece, (int)Square.A1)
						 .With(Black.Color, Lion.Piece, (int)Square.A7);
			var white = new HI(White.Color, board, null);
			var black = new HI(Black.Color, board, null);
			var game = CongoGame.Unattached(board, White.Color, white, black);
			Assert.IsTrue(game.IsDraw());
		}


	}
}
