using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
	public class CongoGame
	{
		#region Unattached game

		public static CongoGame Unattached(CongoBoard board, CongoPlayer activePlayer,
			CongoPlayer whitePlayer, CongoPlayer blackPlayer)
		{
			return new CongoGame(null, null, null, board, activePlayer, whitePlayer, blackPlayer);
		}

		#endregion

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
			if (playerType == typeof(Ai)) return new Ai(color, board, null);
			else                          return new Hi(color, board, null);
		}

		public static CongoGame Standard(Type whitePlayerType, Type blackPlayerType)
		{
			var b = CongoBoard.Empty;
			b = setMixedRank(b, Black.Color, 0);
			b = setPawnRank (b, Black.Color, 1);
			b = setPawnRank (b, White.Color, 5);
			b = setMixedRank(b, White.Color, 6);

			var wp = getPlayer(White.Color, b, whitePlayerType);
			var bp = getPlayer(Black.Color, b, blackPlayerType);

			return Unattached(b, wp, wp, bp);
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
		private readonly ImmutableList<MonkeyJump> monkeyJumps;
		private readonly CongoBoard board;
		private readonly CongoPlayer activePlayer;
		private readonly CongoPlayer whitePlayer;
		private readonly CongoPlayer blackPlayer;

		private CongoGame(CongoGame previousGame, CongoMove transitionMove,
			ImmutableList<MonkeyJump> monkeyJumps, CongoBoard board,
			CongoPlayer activePlayer, CongoPlayer whitePlayer,
			CongoPlayer blackPlayer)
		{
			this.previousGame = previousGame;
			this.transitionMove = transitionMove;
			this.monkeyJumps = monkeyJumps; // only jumps of the active player
			this.board = board;
			this.activePlayer = activePlayer;
			this.whitePlayer = whitePlayer;
			this.blackPlayer = blackPlayer;
		}

		public CongoMove TransitionMove => transitionMove;

		public CongoBoard Board => board;

		public CongoPlayer ActivePlayer
			=> activePlayer.Color.IsWhite() ? whitePlayer : blackPlayer;

		public CongoPlayer Opponent
			=> activePlayer.Color.IsWhite() ? blackPlayer : whitePlayer;

		public CongoGame Transition(CongoMove move)
		{
			var newMonkeyJumps = monkeyJumps;
			var newBoard = board;

			// new players are generated later, white <-> black
			CongoColor newActivePlayerColor = activePlayer.Color.Invert();

			CongoPlayer newWhitePlayer;
			CongoPlayer newBlackPlayer;

			if (move is MonkeyJump) {

				/* first or consecutive monkey jump */

				var jump = (MonkeyJump)move;

				newBoard = newBoard.With(activePlayer.Color, board.GetPiece(move.Fr), move.To)
								   .With(activePlayer.Color.Invert(), Captured.Piece, jump.Bt)
								   .Without(move.Fr);

				if (newMonkeyJumps == null) {
					newMonkeyJumps = new List<MonkeyJump>().ToImmutableList();
				}

				newMonkeyJumps = newMonkeyJumps.Add(jump);
				newActivePlayerColor = newActivePlayerColor.Invert(); // same as activePlayer.Color!

			} else if (board.GetPiece(move.Fr).IsMonkey() && move.Fr == move.To) {

				/* interrupted monkey jump, remove all captured pieces */

				foreach (var jump in monkeyJumps) {
					newBoard = newBoard.Without(jump.Bt);
				}

				newMonkeyJumps = null;

			} else if (board.GetPiece(move.Fr).IsPawn() && board.IsPawnPromotion(ActivePlayer.Color, move.To)) {

				/* pawn -> superpawn promotion */

				newBoard = newBoard.With(activePlayer.Color, Superpawn.Piece, move.To)
								   .Without(move.Fr);
			} else {

				/* ordinary move */

				newBoard = newBoard.With(activePlayer.Color, board.GetPiece(move.Fr), move.To)
								   .Without(move.Fr);
			}

			var newWhiteMonkeyJumps = newActivePlayerColor.IsWhite() ? newMonkeyJumps : null;
			var newBlackMonkeyJumps = newActivePlayerColor.IsBlack() ? newMonkeyJumps : null;

			newWhitePlayer = whitePlayer.With(newBoard, newWhiteMonkeyJumps);
			newBlackPlayer = blackPlayer.With(newBoard, newBlackMonkeyJumps);

			var newActivePlayer = newActivePlayerColor.IsWhite() ?
				newWhitePlayer : newBlackPlayer;

			return new CongoGame(this, move, newMonkeyJumps, newBoard,
				newActivePlayer, newWhitePlayer, newBlackPlayer);
		}

		public bool IsInvalid()
		{
			return !ActivePlayer.HasLion && !Opponent.HasLion;
		}

		public bool IsWin()
		{

			return (ActivePlayer.HasLion && !Opponent.HasLion) ||
				(!ActivePlayer.HasLion && Opponent.HasNonLion);
		}

		public bool IsDraw()
		{
			return ActivePlayer.HasLion && !ActivePlayer.HasNonLion &&
				   Opponent.HasLion && !Opponent.HasNonLion;
		}

		public bool HasEnded()
		{
			return IsInvalid() || IsWin() || IsDraw();
		}
	}
}
