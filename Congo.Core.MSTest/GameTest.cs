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

		[TestMethod]
		public void Game_PawnToSuperpawnPromotion()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Pawn.Piece, (int)Square.D6)
				.With(Black.Color, Pawn.Piece, (int)Square.F2)
				.With(White.Color, Pawn.Piece, (int)Square.G5)
				.With(Black.Color, Pawn.Piece, (int)Square.B3);
			var white = new Hi(White.Color, board, null);
			var black = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, white, black);
			game = game.Transition(new CongoMove((int)Square.D6, (int)Square.D7));
			game = game.Transition(new CongoMove((int)Square.F2, (int)Square.F1));
			game = game.Transition(new CongoMove((int)Square.G5, (int)Square.G6));
			game = game.Transition(new CongoMove((int)Square.B3, (int)Square.B2));
			Assert.IsTrue(
				game.Board.GetPiece((int)Square.D7).IsSuperpawn() &&
				game.Board.GetPiece((int)Square.F1).IsSuperpawn() &&
				game.Board.GetPiece((int)Square.G6).IsPawn() &&
				game.Board.GetPiece((int)Square.B2).IsPawn()
			);
		}
	}
}
