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

		private static CongoPlayer getPlayerByType(CongoColor color, Type playerType,
			CongoBoard board, ImmutableList<MonkeyJump> monkeyJumps)
		{
			if (playerType == typeof(Ai)) return new Ai(color, board, monkeyJumps);
			else                          return new Hi(color, board, monkeyJumps);
		}

		public static CongoGame Standard(Type whitePlayerType, Type blackPlayerType)
		{
			var b = CongoBoard.Empty;
			b = setMixedRank(b, Black.Color, 0);
			b = setPawnRank (b, Black.Color, 1);
			b = setPawnRank (b, White.Color, 5);
			b = setMixedRank(b, White.Color, 6);

			var wp = getPlayerByType(White.Color, whitePlayerType, b, null);
			var bp = getPlayerByType(Black.Color, blackPlayerType, b, null);

			return Unattached(b, wp, wp, bp);
		}

		#endregion

		#region Fen (de-)serialization

		/* 
		 * Simplified Fen format, 7 ranks, 2 player types, 1 active color
		 *     rank/rank/rank/rank/rank/rank/rank/type/type/color 
		 * Fen for standard board
		 *     gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/h/a/w
		 */

		private static readonly string fenPieceSignatures = "gmelczpsx";
		private static readonly ImmutableDictionary<char, CongoPiece> fenPieceViews =
			new Dictionary<char, CongoPiece>() {
				{ 'g', Giraffe.Piece   }, { 'm', Monkey.Piece    },
				{ 'e', Elephant.Piece  }, { 'l', Lion.Piece      },
				{ 'c', Crocodile.Piece }, { 'z', Zebra.Piece     },
				{ 'p', Pawn.Piece      }, { 's', Superpawn.Piece },
				{ 'x', Captured.Piece  }
			}.ToImmutableDictionary();

		private static int[] fenPieceDistribution()
		{
			var pieces = new int[11];

			pieces[(int)Giraffe.Piece.Code]   =  1; pieces[(int)Monkey.Piece.Code]    = 1;
			pieces[(int)Elephant.Piece.Code]  =  2; pieces[(int)Lion.Piece.Code]      = 1;
			pieces[(int)Crocodile.Piece.Code] =  1; pieces[(int)Zebra.Piece.Code]     = 1;
			pieces[(int)Pawn.Piece.Code]      =  7; pieces[(int)Superpawn.Piece.Code] = 7;
			pieces[(int)Captured.Piece.Code]  = 14;

			return pieces;
		}

		private static CongoBoard fenAddPiece(CongoBoard board, CongoColor color, char pieceView,
			int square, ref int file, ref ImmutableList<MonkeyJump> monkeyJumps, int[] pieceCounter)
		{
			if (pieceView == 'x') {

				/* Trick how to force player to continue monkey jump (!= null).
				 * Values -1 does not cause issues with addressing. */

				if (monkeyJumps == null) { new List<MonkeyJump>().ToImmutableArray(); }
				monkeyJumps = monkeyJumps.Add(new MonkeyJump(-1, square, -1));
			}

			file++;
			pieceCounter[(int)fenPieceViews[pieceView].Code]--;

			return board.With(color, fenPieceViews[pieceView], square);
		}

		private static CongoPlayer fenGetPlayer(CongoColor color, CongoBoard board,
			string type, ImmutableList<MonkeyJump> monkeyJumps)
		{
			if (type != "a" && type != "h") { return null; }

			var playerType = type == "a" ? typeof(Ai) : typeof(Hi);

			return getPlayerByType(color, playerType, board, monkeyJumps);
		}

		private static CongoPlayer fenGetActivePlayer(string color,
			CongoPlayer whitePlayer, CongoPlayer blackPlayer)
		{
			if (color != "w" && color != "b") { return null; }

			return color == "w" ? whitePlayer : blackPlayer;
		}

		private static bool isFenPieceCountValid(int[] pieceCount)
		{
			foreach (var cnt in pieceCount) {
				if (cnt < 0) { return false; }
			}

			return true;
		}

		public static CongoGame FromFen(string fen)
		{
			var whitePieceCounter = fenPieceDistribution();
			var blackPieceCounter = fenPieceDistribution();
			var seps = new char[] { '/' };
			var sfen = fen.Split(seps, StringSplitOptions.RemoveEmptyEntries);
			ImmutableList<MonkeyJump> whiteMonkeyJumps = null;
			ImmutableList<MonkeyJump> blackMonkeyJumps = null;

			if (sfen.Length != 10) { return null; }

			var board = CongoBoard.Empty;

			for (int rank = 0; rank < CongoBoard.Empty.Size; rank++) { // ranks
				int file = 0;

				for (int i = 0; i < sfen[rank].Length; i++) {
					if (sfen[rank][i] >= '1' && sfen[rank][i] <= '7') {
						file += sfen[rank][i] - '0'; // skip empty squares
					} else if (fenPieceSignatures.ToUpper().IndexOf(sfen[rank][i]) >= 0) { // white pieces
						board = fenAddPiece(board, White.Color, (char)(sfen[rank][i] - 'A' + 'a'),
							rank * board.Size + file, ref file, ref whiteMonkeyJumps,
							whitePieceCounter);
					} else if (fenPieceSignatures.ToLower().IndexOf(sfen[rank][i]) >= 0) { // black pieces
						board = fenAddPiece(board, Black.Color, sfen[rank][i],
							rank * board.Size + file, ref file, ref blackMonkeyJumps,
							blackPieceCounter);
					} else {
						return null;
					}
				}

				if (file > board.Size || (whiteMonkeyJumps != null && blackMonkeyJumps != null) ||
					!isFenPieceCountValid(whitePieceCounter) || !isFenPieceCountValid(blackPieceCounter))
					{ return null; }
			}

			var whitePlayer = fenGetPlayer(White.Color, board, sfen[7], whiteMonkeyJumps);
			var blackPlayer = fenGetPlayer(Black.Color, board, sfen[8], blackMonkeyJumps);
			var activePlayer = fenGetActivePlayer(sfen[9], whitePlayer, blackPlayer);

			if (whitePlayer == null || blackPlayer == null || activePlayer == null) { return null; }

			var monkeyJumps = activePlayer.Color.IsWhite() ? whiteMonkeyJumps : blackMonkeyJumps;

			return new CongoGame(null, null, monkeyJumps, board, activePlayer, whitePlayer, blackPlayer);
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
				{ typeof(Captured), "x" },
				{ typeof(Ai),       "a" }, { typeof(Hi),        "h" },
				{ typeof(White),    "w" }, { typeof(Black),     "b"}
			};

			for (int rank = 0; rank < game.Board.Size; rank++) {
				int cnt = 0;
				for (int file = 0; file < game.Board.Size; file++) {
					var square = rank * game.Board.Size + file;
					var piece = game.Board.GetPiece(square);

					if (typeViews.ContainsKey(piece.GetType())) { // player piece
						
						if (cnt > 0) { result += cnt.ToString(); }
						cnt = 0;

						var repr = game.Board.IsPieceWhite(square)
							? typeViews[piece.GetType()].ToUpper()
							: typeViews[piece.GetType()].ToLower();
						result += repr;
					} else { // ground or river
						cnt++;
					}
				}

				if (cnt > 0) { result += cnt.ToString(); }
				result += sep;
			}

			result += typeViews[game.WhitePlayer.GetType()] + sep;
			result += typeViews[game.BlackPlayer.GetType()] + sep;
			result += typeViews[game.ActivePlayer.Color.GetType()];

			return result;
		}

		#endregion

		#region Instance members

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

		public CongoPlayer WhitePlayer => whitePlayer;

		public CongoPlayer BlackPlayer => blackPlayer;

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

	#endregion
}
