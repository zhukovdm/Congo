using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
	public class CongoGame
	{
		#region Standard game

		private static CongoBoard setMixedRank(CongoBoard board, CongoColor color, int rank)
		{
			board = board.With(color, Giraffe.Piece,   rank * board.Size + 0)
						 .With(color, Monkey.Piece,	   rank * board.Size + 1)
						 .With(color, Elephant.Piece,  rank * board.Size + 2)
						 .With(color, Lion.Piece,      rank * board.Size + 3)
						 .With(color, Elephant.Piece,  rank * board.Size + 4)
						 .With(color, Crocodile.Piece, rank * board.Size + 5)
						 .With(color, Zebra.Piece,     rank * board.Size + 6);
			
			return board;
		}

		private static CongoBoard setPawnRank(CongoBoard board, CongoColor color, int rank)
		{
			for (int file = 0; file < board.Size; file++) {
				board = board.With(color, Pawn.Piece, rank * board.Size + file);
			}

			return board;
		}

		private static CongoPlayer getPlayer(CongoColor color, CongoBoard board, Type playerType)
		{
			if (playerType == typeof(AI)) return new AI(color, board, null);
			else                          return new HI(color, board, null);
		}

		public static CongoGame Standard(Type whiteType, Type blackType)
		{
			var b = CongoBoard.Empty;
			b = setMixedRank(b, Black.Color, 0);
			b = setPawnRank (b, Black.Color, 1);
			b = setPawnRank (b, White.Color, 5);
			b = setMixedRank(b, White.Color, 6);

			var wp = getPlayer(White.Color, b, whiteType);
			var bp = getPlayer(Black.Color, b, blackType);

			return new CongoGame(null, null, null, b, White.Color, wp, bp);
		}

		#endregion

		#region Unattached game

		public static CongoGame Unattached(CongoBoard board, CongoColor color,
			CongoPlayer whitePlayer, CongoPlayer blackPlayer)
		{
			return new CongoGame(null, null, null, board, color, whitePlayer, blackPlayer);
		}

		#endregion

		#region FEN (de-)serialization

		/* 
		 * Simplified FEN format:
		 *     rank/rank/rank/rank/rank/rank/rank active_color 
		 *         3z1p1/... w m
		 *         8/8/2zGm/... b -
		 */

		public static bool IsFenValid(string fen)
			=> throw new NotImplementedException();

		public static CongoGame FromFen(string fen)
			=> throw new NotImplementedException();

		public static string ToFen(CongoGame game)
			=> throw new NotImplementedException();

		#endregion

		private readonly CongoGame previousGame;
		private readonly CongoMove transitionMove;
		private readonly ImmutableList<int> monkeyCaptures;
		private readonly CongoBoard board;
		private readonly CongoColor activePlayerColor;
		private readonly CongoPlayer whitePlayer;
		private readonly CongoPlayer blackPlayer;

		private CongoGame(CongoGame previousGame, CongoMove transitionMove,
			ImmutableList<int> monkeyCaptures, CongoBoard board,
			CongoColor activePlayerColor, CongoPlayer whitePlayer,
			CongoPlayer blackPlayer)
		{
			this.previousGame = previousGame;
			this.transitionMove = transitionMove;
			this.monkeyCaptures = monkeyCaptures;
			this.board = board;
			this.activePlayerColor = activePlayerColor;
			this.whitePlayer = whitePlayer;
			this.blackPlayer = blackPlayer;
		}

		public CongoMove TransitionMove => transitionMove;

		public CongoBoard Board => board;

		public CongoColor ActivePlayerColor => activePlayerColor;

		public CongoPlayer ActivePlayer
			=> activePlayerColor.IsWhite() ? whitePlayer : blackPlayer;

		public CongoPlayer Opponent
			=> activePlayerColor.IsWhite() ? blackPlayer : whitePlayer;

		public CongoGame Transition(CongoMove move)
		{
			var newMonkeyCaptures = monkeyCaptures;
			var newBoard = board;
			CongoColor newActivePlayerColor = activePlayerColor.Invert();
			CongoPlayer newWhitePlayer;
			CongoPlayer newBlackPlayer;

			if (move is MonkeyJump) {

				/* first or consecutive monkey jump */

				var jump = (MonkeyJump)move;

				newBoard = newBoard.With(activePlayerColor, board.GetPiece(move.Fr), move.To)
								   .With(activePlayerColor.Invert(), Captured.Piece, jump.Bt)
								   .Without(move.Fr);

				if (newMonkeyCaptures == null) {
					newMonkeyCaptures = new List<int>().ToImmutableList();
				}

				newMonkeyCaptures = newMonkeyCaptures.Add(jump.Bt);
				newActivePlayerColor = newActivePlayerColor.Invert(); // repeated move

			} else if (board.GetPiece(move.Fr) is Monkey && move.Fr == move.To) {

				/* interrupted monkey jump, remove all captured pieces */

				foreach (var capture in monkeyCaptures) {
					newBoard = newBoard.Without(capture);
				}

			} else {

				/* ordinary move */

				newBoard = newBoard.With(activePlayerColor, board.GetPiece(move.Fr), move.To)
								   .Without(move.Fr);
			}

			newWhitePlayer = whitePlayer.With(White.Color, newBoard, newMonkeyCaptures);
			newBlackPlayer = blackPlayer.With(Black.Color, newBoard, newMonkeyCaptures);

			return new CongoGame(this, move, newMonkeyCaptures, newBoard,
				newActivePlayerColor, newWhitePlayer, newBlackPlayer);
		}

		public bool IsInvalid()
		{
			return !ActivePlayer.HasLion && !Opponent.HasLion;
		}

		public bool IsWin()
		{
			return !ActivePlayer.HasLion;
		}

		public bool IsDraw()
		{
			return !ActivePlayer.HasNonLion && !ActivePlayer.HasNonLion;
		}

		public bool HasEnded()
		{
			return IsInvalid() || IsWin() || IsDraw();
		}
	}
}
