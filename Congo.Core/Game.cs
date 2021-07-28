using System;

namespace Congo.Core
{
	public class CongoGame
	{
		#region Unattached game

		public static CongoGame Unattached(CongoBoard board, CongoPlayer whitePlayer,
			CongoPlayer blackPlayer, CongoPlayer activePlayer, MonkeyJump firstMonkeyJump)
		{
			return new CongoGame(null, null, board, whitePlayer, blackPlayer,
				activePlayer, firstMonkeyJump);
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

		public static CongoGame Standard(Type whitePlayerType, Type blackPlayerType)
		{
			var b = CongoBoard.Empty;
			b = setMixedRank(b, Black.Color, 0);
			b = setPawnRank (b, Black.Color, 1);
			b = setPawnRank (b, White.Color, 5);
			b = setMixedRank(b, White.Color, 6);

			var wp = CongoPlayer.GetByType(b, whitePlayerType, White.Color, null);
			var bp = CongoPlayer.GetByType(b, blackPlayerType, Black.Color, null);

			return Unattached(b, wp, bp, wp, null);
		}

		#endregion

		#region Instance members

		private readonly CongoGame previousGame;
		private readonly CongoMove transitionMove;
		private readonly CongoBoard board;
		private readonly CongoPlayer whitePlayer;
		private readonly CongoPlayer blackPlayer;
		private readonly CongoPlayer activePlayer;
		private readonly MonkeyJump firstMonkeyJump;

		private bool isInterruptedMonkeyJump(CongoMove move)
			=> board.GetPiece(move.Fr).IsMonkey() && move.Fr == move.To;

		private bool isPawnPromotion(CongoMove move)
		{
			return board.GetPiece(move.Fr).IsPawn() &&
				board.IsUpDownBorder(activePlayer.Color, move.To);
		}

		private bool isFriendlyAnimal(CongoPiece piece, CongoColor color)
			=> piece.IsAnimal() && color == activePlayer.Color;

		private CongoGame(CongoGame previousGame, CongoMove transitionMove,
			CongoBoard board, CongoPlayer whitePlayer, CongoPlayer blackPlayer,
			CongoPlayer activePlayer, MonkeyJump firstMonkeyJump)
		{
			this.previousGame = previousGame;
			this.transitionMove = transitionMove;
			this.firstMonkeyJump = firstMonkeyJump; // only jump of the active player
			this.board = board;
			this.activePlayer = activePlayer;
			this.whitePlayer = whitePlayer;
			this.blackPlayer = blackPlayer;
		}

		public CongoMove TransitionMove => transitionMove;

		public CongoBoard Board => board;

		public CongoPlayer WhitePlayer => whitePlayer;

		public CongoPlayer BlackPlayer => blackPlayer;

		public CongoPlayer ActivePlayer
			=> activePlayer.Color.IsWhite() ? whitePlayer : blackPlayer;

		public CongoPlayer Opponent
			=> activePlayer.Color.IsWhite() ? blackPlayer : whitePlayer;

		public CongoMove FirstMonkeyJump => firstMonkeyJump;

		public CongoGame Transition(CongoMove move)
		{
			var newBoard = board;
			CongoPlayer newWhitePlayer;
			CongoPlayer newBlackPlayer;
			CongoColor newActivePlayerColor = activePlayer.Color.Invert();
			MonkeyJump newFirstMonkeyJump = firstMonkeyJump;

			#region Execute move. Define newBoard, newFirstMonkeyJump, newActivePlayerColor.

			// first or consecutive monkey jump
			if (move is MonkeyJump) {
				var jump = (MonkeyJump)move;

				newBoard = newBoard.With(activePlayer.Color, board.GetPiece(jump.Fr), jump.To)
								   .Without(jump.Bt)
								   .Without(jump.Fr);

				if (newFirstMonkeyJump == null) { newFirstMonkeyJump = jump; }
				newActivePlayerColor = newActivePlayerColor.Invert(); // the color remains
			}

			// interrupted monkey jump
			else if (isInterruptedMonkeyJump(move)) { newFirstMonkeyJump = null; }

			// pawn -> superpawn promotion
			else if (isPawnPromotion(move)) {

				newBoard = newBoard.With(activePlayer.Color, Superpawn.Piece, move.To)
								   .Without(move.Fr);
			}

			// ordinary move
			else {

				newBoard = newBoard.With(activePlayer.Color, board.GetPiece(move.Fr), move.To)
								   .Without(move.Fr);
			}

			#endregion

			#region Drowning. Define newBoard.

			for (int square = (int)Square.A4; square <= (int)Square.G4; square++) {

				var piece = board.GetPiece(square);
				var color = board.IsPieceWhite(square) ? White.Color : Black.Color;

				// consider only friendly non-crocodiles
				if (!isFriendlyAnimal(piece, color) || piece.IsCrocodile()) { }

				// not-moved piece -> stay at the river -> drown
				else if (move.To != square) { newBoard = newBoard.Without(square); }

				/* from now onwards move.To == square */

				// ground-to-river move
				else if (!board.IsRiver(move.Fr)) { }

				// ordinary non-monkey river-to-river move -> drown
				else if (!piece.IsMonkey() && board.IsRiver(move.Fr)) {
					newBoard = newBoard.Without(square);
				}

				/* from now onwards monkey river-to-river move */

				// interrupted monkey jump
				else if (move.Fr == move.To) {

					// started from the river -> drown
					if (board.IsRiver(firstMonkeyJump.Fr)) {
						newBoard = newBoard.Without(square);
					}
					
					// otherwise, remains on the board
					else { }
				}

				// continued monkey jump
				else if (move is MonkeyJump) { }

				// ordinary monkey river-to-river move -> drown
				else { newBoard = newBoard.Without(square); }
			}

			#endregion

			#region Finalize. Define newWhitePlayers, newBlackPlayer, newActivePlayer.

			var newWhiteMonkeyJumps = newActivePlayerColor.IsWhite() ? newFirstMonkeyJump : null;
			var newBlackMonkeyJumps = newActivePlayerColor.IsBlack() ? newFirstMonkeyJump : null;

			newWhitePlayer = whitePlayer.With(newBoard, newWhiteMonkeyJumps);
			newBlackPlayer = blackPlayer.With(newBoard, newBlackMonkeyJumps);

			var newActivePlayer = newActivePlayerColor.IsWhite()
				? newWhitePlayer : newBlackPlayer;

			#endregion

			return new CongoGame(this, move, newBoard, newWhitePlayer,
				newBlackPlayer, newActivePlayer, newFirstMonkeyJump);
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

	#endregion
}
