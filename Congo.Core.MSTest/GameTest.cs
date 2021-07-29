using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
	[TestClass]
	public class Game_State_Test
	{
		[TestMethod]
		public void IsInvalid()
		{
			var board = CongoBoard.Empty;
			board = board.With(White.Color, Pawn.Piece, (int)Square.A1)
						 .With(Black.Color, Pawn.Piece, (int)Square.A2);
			var white = new Hi(White.Color, board, null);
			var black = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, black, white, null);
			Assert.IsTrue(game.IsInvalid());
		}

		[TestMethod]
		public void IsWin()
		{
			var board = CongoBoard.Empty;
			board = board.With(White.Color, Lion.Piece, (int)Square.A1)
						 .With(White.Color, Pawn.Piece, (int)Square.A2)
						 .With(Black.Color, Pawn.Piece, (int)Square.A7);
			var white = new Hi(White.Color, board, null);
			var black = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, black, white, null);
			Assert.IsTrue(game.IsWin());
		}

		[TestMethod]
		public void MonkeyJumps()
		{
			
		}

		[TestMethod]
		public void PawnToSuperpawnPromotion()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Pawn.Piece, (int)Square.D6)
				.With(Black.Color, Pawn.Piece, (int)Square.F2)
				.With(White.Color, Pawn.Piece, (int)Square.G5)
				.With(Black.Color, Pawn.Piece, (int)Square.B3);
			var white = new Hi(White.Color, board, null);
			var black = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, black, white, null);
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

	[TestClass]
	public class Game_Fen_Test
	{
		[TestMethod]
		public void Game_FromFen_TwoLions()
		{
			var game = CongoFen.FromFen("3l3/7/7/7/7/7/3L3/h/a/w/-1");
			Assert.IsTrue(
				game.Board.GetPiece((int)Square.D7).IsLion() &&
				game.Board.IsBlackPiece((int)Square.D7) &&
				game.Board.GetPiece((int)Square.D1).IsLion() &&
				game.Board.IsWhitePiece((int)Square.D1) &&
				game.WhitePlayer is Hi &&
				game.BlackPlayer is Ai &&
				game.ActivePlayer.Color.IsWhite()
			);
		}

		[TestMethod]
		public void Game_FromFen_InvalidShortRankUnderflow()
		{
			var game = CongoFen.FromFen("3l2/7/7/7/7/7/3L3/h/a/w/-1");
			Assert.IsTrue(game == null);
		}

		[TestMethod]
		public void Game_FromFen_InvalidLongRankOverflow()
		{
			var game = CongoFen.FromFen("3l3/8/7/7/7/7/3L3/h/a/w/-1");
			Assert.IsTrue(game == null);
		}

		[TestMethod]
		public void Game_FromFen_InvalidRankPiecesOnRankOverflow()
		{
			var game = CongoFen.FromFen("7/7/7/7/7/7/3L3P/h/a/w/-1");
			Assert.IsTrue(game == null);
		}

		[TestMethod]
		public void Game_ToFen_Standard()
		{
			var game = CongoGame.Standard(typeof(Hi), typeof(Ai));
			var actual = CongoFen.ToFen(game);
			var expected = "gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/h/a/w/-1";
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void Game_ToFen_Empty()
		{
			var board = CongoBoard.Empty;
			var white = new Hi(White.Color, board, null);
			var black = new Ai(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, black, black, null);
			var actual = CongoFen.ToFen(game);
			var expected = "7/7/7/7/7/7/7/h/a/b/-1";
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void Game_ToFen_WithFirstMonkeyJump()
		{
			var board = CongoBoard.Empty;
			var white = new Hi(White.Color, board, null);
			var black = new Ai(Black.Color, board, null);
			var game = CongoGame.Unattached(board, white, black, white, new MonkeyJump(5, -1, -1));
			var actual = CongoFen.ToFen(game);
			var expected = "7/7/7/7/7/7/7/h/a/w/5";
			Assert.IsTrue(expected == actual);
		}
	}

	[TestClass]
	public class Game_Drowning_Test
	{
		[TestMethod]
		public void Game_Drowning_CrocodilesSwim()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Crocodile.Piece, (int)Square.C4);
			var whitePlayer = new Hi(White.Color, board, null);
			var blackPlayer = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, whitePlayer, blackPlayer, whitePlayer, null);
			game = game.Transition(new CongoMove((int)Square.C4, (int)Square.D4));
			Assert.IsTrue(
				game.Board.GetPiece((int)Square.D4).IsAnimal()
			);
		}

		[TestMethod]
		public void Game_Drowning_NonFriendlyPiecesStay()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Crocodile.Piece, (int)Square.C4)
				.With(Black.Color, Elephant.Piece, (int)Square.E4);
			var whitePlayer = new Hi(White.Color, board, null);
			var blackPlayer = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, whitePlayer, blackPlayer, whitePlayer, null);
			game = game.Transition(new CongoMove((int)Square.C4, (int)Square.D4));
			Assert.IsTrue(
				game.Board.GetPiece((int)Square.D4).IsAnimal() &&
				game.Board.GetPiece((int)Square.E4).IsAnimal()
			);
		}

		[TestMethod]
		public void Game_Drowning_StayedAndDrowned()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Elephant.Piece, (int)Square.B4)
				.With(White.Color, Elephant.Piece, (int)Square.C4);
			var whitePlayer = new Hi(White.Color, board, null);
			var blackPlayer = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, whitePlayer, blackPlayer, whitePlayer, null);
			game = game.Transition(new CongoMove((int)Square.B4, (int)Square.B5));
			Assert.IsTrue(
				game.Board.GetPiece((int)Square.B5).IsAnimal() &&
				!game.Board.GetPiece((int)Square.C4).IsAnimal()
			);
		}

		[TestMethod]
		public void Game_Drowning_AnimalMoveRiverToRiver()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Elephant.Piece, (int)Square.C4);
			var whitePlayer = new Hi(White.Color, board, null);
			var blackPlayer = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, whitePlayer, blackPlayer, whitePlayer, null);
			game = game.Transition(new CongoMove((int)Square.C4, (int)Square.D4));
			Assert.IsTrue(
				!game.Board.GetPiece((int)Square.D4).IsAnimal()
			);
		}

		[TestMethod]
		public void Game_Drowning_ContinuedMonkeyJump()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Monkey.Piece, (int)Square.C4)
				.With(Black.Color, Pawn.Piece, (int)Square.D4);
			var firstMonkeyJump = new MonkeyJump((int)Square.A4, -1, -1);
			var whitePlayer = new Hi(White.Color, board, firstMonkeyJump);
			var blackPlayer = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, whitePlayer, blackPlayer, whitePlayer, firstMonkeyJump);
			game = game.Transition(new MonkeyJump((int)Square.C4, (int)Square.D4, (int)Square.E4));
			Assert.IsTrue(
				!game.Board.GetPiece((int)Square.D4).IsAnimal() &&
				game.Board.GetPiece((int)Square.E4).IsMonkey()
			);
		}

		[TestMethod]
		public void Game_Drowning_InterruptedMonkeyJumpFirstNonRiver()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Monkey.Piece, (int)Square.D4);
			var firstMonkeyJump = new MonkeyJump((int)Square.A1, -1, -1);
			var whitePlayer = new Hi(White.Color, board, firstMonkeyJump);
			var blackPlayer = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, whitePlayer, blackPlayer, whitePlayer, firstMonkeyJump);
			game = game.Transition(new CongoMove((int)Square.D4, (int)Square.D4));
			Assert.IsTrue(
				game.Board.GetPiece((int)Square.D4).IsMonkey()
			);
		}

		[TestMethod]
		public void Game_Drowning_InterruptedMonkeyJumpFirstRiver()
		{
			var board = CongoBoard.Empty
				.With(White.Color, Monkey.Piece, (int)Square.D4);
			var firstMonkeyJump = new MonkeyJump((int)Square.A4, -1, -1);
			var whitePlayer = new Hi(White.Color, board, firstMonkeyJump);
			var blackPlayer = new Hi(Black.Color, board, null);
			var game = CongoGame.Unattached(board, whitePlayer, blackPlayer, whitePlayer, firstMonkeyJump);
			game = game.Transition(new CongoMove((int)Square.D4, (int)Square.D4));
			Assert.IsTrue(
				!game.Board.GetPiece((int)Square.D4).IsAnimal()
			);
		}
	}
}
