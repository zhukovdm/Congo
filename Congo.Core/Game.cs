using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

		private static CongoPlayer getPlayerByType(CongoBoard board,
			Type playerType, CongoColor color, MonkeyJump firstMonkeyJump)
		{
			if (playerType == typeof(Ai)) return new Ai(color, board, firstMonkeyJump);
			else                          return new Hi(color, board, firstMonkeyJump);
		}

		public static CongoGame Standard(Type whitePlayerType, Type blackPlayerType)
		{
			var b = CongoBoard.Empty;
			b = setMixedRank(b, Black.Color, 0);
			b = setPawnRank (b, Black.Color, 1);
			b = setPawnRank (b, White.Color, 5);
			b = setMixedRank(b, White.Color, 6);

			var wp = getPlayerByType(b, whitePlayerType, White.Color, null);
			var bp = getPlayerByType(b, blackPlayerType, Black.Color, null);

			return Unattached(b, wp, bp, wp, null);
		}

		#endregion

		#region Fen (de-)serialization

		/* Simplified Fen format
		 *   - 7 ranks,
		 *   - 2 player types,
		 *   - 1 active color,
		 *   - 1 first monkey jump from
		 *     rank/rank/rank/rank/rank/rank/rank/type/type/color 
		 * Fen for standard board
		 *     gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/h/a/w/-1
		 */

		private static readonly string fenPieceSignatures = "gmelczps";

		private static readonly ImmutableDictionary<char, CongoPiece> fenPieceViews =
			new Dictionary<char, CongoPiece>() {
				{ 'g', Giraffe.Piece   }, { 'm', Monkey.Piece    },
				{ 'e', Elephant.Piece  }, { 'l', Lion.Piece      },
				{ 'c', Crocodile.Piece }, { 'z', Zebra.Piece     },
				{ 'p', Pawn.Piece      }, { 's', Superpawn.Piece }
			}.ToImmutableDictionary();

		private static CongoColor fenGetActivePlayerColor(string color)
		{
			if (color != "w" && color != "b") { return null; }

			return color == "w" ? White.Color : Black.Color;
		}

		private static MonkeyJump fenGetFirstMonkeyJump(string input)
		{
			var upperBound = CongoBoard.Empty.Size * CongoBoard.Empty.Size;
			if (int.TryParse(input, out int from) && from >= -1 && from < upperBound) {
				return new MonkeyJump(from, -1, -1);
			}

			return null;
		}

		private static CongoBoard fenAddPiece(CongoBoard board, CongoColor color,
			char pieceView, int square, ref int file)
		{
			file++;

			return board.With(color, fenPieceViews[pieceView], square);
		}

		private static CongoPlayer fenGetPlayer(CongoBoard board,
			string type, CongoColor color, MonkeyJump firstMonkeyJump)
		{
			if (type != "a" && type != "h") { return null; }

			var playerType = type == "a" ? typeof(Ai) : typeof(Hi);

			return getPlayerByType(board, playerType, color, firstMonkeyJump);
		}

		public static CongoGame FromFen(string fen)
		{
			var seps = new char[] { '/' };
			var sfen = fen.Split(seps, StringSplitOptions.RemoveEmptyEntries);

			CongoColor activePlayerColor = null;
			MonkeyJump whiteFirstMonkeyJump = null;
			MonkeyJump blackFirstMonkeyJump = null;

			if (sfen.Length != 11) { return null; }

			activePlayerColor = fenGetActivePlayerColor(sfen[9]);
			if (activePlayerColor == null) { return null; }

			var firstMonkeyJump = fenGetFirstMonkeyJump(sfen[10]);
			if (firstMonkeyJump == null) { return null; }
			if (firstMonkeyJump.Fr == -1) { firstMonkeyJump = null; }

			var board = CongoBoard.Empty;

			// parse board rank-by-rank
			for (int rank = 0; rank < CongoBoard.Empty.Size; rank++) {
				int file = 0;

				for (int i = 0; i < sfen[rank].Length; i++) {

					// rank overflow
					if (file >= board.Size) { return null; }

					// skip empty squares
					if (sfen[rank][i] >= '1' && sfen[rank][i] <= '7') {
						file += sfen[rank][i] - '0';
					}

					// white pieces
					else if (fenPieceSignatures.ToUpper().IndexOf(sfen[rank][i]) >= 0) {
						board = fenAddPiece(board, White.Color, (char)(sfen[rank][i] - 'A' + 'a'),
							rank * board.Size + file, ref file);
					}

					// black pieces
					else if (fenPieceSignatures.ToLower().IndexOf(sfen[rank][i]) >= 0) {
						board = fenAddPiece(board, Black.Color, sfen[rank][i],
							rank * board.Size + file, ref file);
					}
					
					// unknown signature
					else { return null; }
				}

				// invalid input
				if (file != board.Size) { return null; }
			}

			whiteFirstMonkeyJump = activePlayerColor.IsWhite() ? firstMonkeyJump : null;
			blackFirstMonkeyJump = activePlayerColor.IsBlack() ? firstMonkeyJump : null;

			var whitePlayer = fenGetPlayer(board, sfen[7], White.Color, whiteFirstMonkeyJump);
			var blackPlayer = fenGetPlayer(board, sfen[8], Black.Color, blackFirstMonkeyJump);
			if (whitePlayer == null || blackPlayer == null) { return null; }

			var activePlayer = activePlayerColor.IsWhite() ? whitePlayer : blackPlayer;

			return Unattached(board, whitePlayer, blackPlayer, activePlayer, firstMonkeyJump);
		}

		public static string ToFen(CongoGame game)
		{
			var result = "";
			var sep = "/";

			Dictionary<Type, string> typeViews = new Dictionary<Type, string>() {
				{ typeof(Elephant), "e" }, { typeof(Zebra),     "z" },
				{ typeof(Giraffe),  "g" }, { typeof(Crocodile), "c" },
				{ typeof(Pawn),     "p" }, { typeof(Superpawn), "s" },
				{ typeof(Lion),     "l" }, { typeof(Monkey),    "m" },
				{ typeof(Ai),       "a" }, { typeof(Hi),        "h" },
				{ typeof(White),    "w" }, { typeof(Black),     "b" }
			};

			for (int rank = 0; rank < game.Board.Size; rank++) {
				int cnt = 0;
				for (int file = 0; file < game.Board.Size; file++) {
					var square = rank * game.Board.Size + file;
					var piece = game.Board.GetPiece(square);

					// animals
					if (typeViews.ContainsKey(piece.GetType())) {
						
						if (cnt > 0) { result += cnt.ToString(); }
						cnt = 0;

						var repr = game.Board.IsPieceWhite(square)
							? typeViews[piece.GetType()].ToUpper()
							: typeViews[piece.GetType()].ToLower();
						result += repr;
					}

					// ground or river
					else {
						cnt++;
					}
				}

				if (cnt > 0) { result += cnt.ToString(); }
				result += sep;
			}

			result += typeViews[game.WhitePlayer.GetType()] + sep;
			result += typeViews[game.BlackPlayer.GetType()] + sep;
			result += typeViews[game.ActivePlayer.Color.GetType()] + sep;

			result += game.firstMonkeyJump == null
				? (-1).ToString()
				: game.firstMonkeyJump.Fr.ToString();

			return result;
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

		public CongoGame Transition(CongoMove move)
		{
			var newBoard = board;
			CongoPlayer newWhitePlayer;
			CongoPlayer newBlackPlayer;
			CongoColor newActivePlayerColor = activePlayer.Color.Invert();
			MonkeyJump newFirstMonkeyJump = firstMonkeyJump;

			#region define newBoard, newFirstMonkeyJump, newActivePlayerColor

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

			#region drowning

			

			#endregion

			#region define newWhitePlayers, newBlackPlayer, newActivePlayer

			var newWhiteMonkeyJumps = newActivePlayerColor.IsWhite() ? newFirstMonkeyJump : null;
			var newBlackMonkeyJumps = newActivePlayerColor.IsBlack() ? newFirstMonkeyJump : null;

			newWhitePlayer = whitePlayer.With(newBoard, newWhiteMonkeyJumps);
			newBlackPlayer = blackPlayer.With(newBoard, newBlackMonkeyJumps);

			var newActivePlayer = newActivePlayerColor.IsWhite() ?
				newWhitePlayer : newBlackPlayer;

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
