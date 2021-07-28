using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
	public static class CongoFen
	{
		/* Simplified Fen format
		 *   - 7 ranks,
		 *   - 2 player types,
		 *   - 1 active color,
		 *   - 1 first monkey jump from
		 *     rank/rank/rank/rank/rank/rank/rank/type/type/color 
		 * Fen for standard board
		 *     gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/h/a/w/-1
		 */

		private static readonly string pieceSignatures = "gmelczps";

		private static readonly ImmutableDictionary<char, CongoPiece> pieceViews =
			new Dictionary<char, CongoPiece>() {
				{ 'g', Giraffe.Piece   }, { 'm', Monkey.Piece    },
				{ 'e', Elephant.Piece  }, { 'l', Lion.Piece      },
				{ 'c', Crocodile.Piece }, { 'z', Zebra.Piece     },
				{ 'p', Pawn.Piece      }, { 's', Superpawn.Piece }
			}.ToImmutableDictionary();

		private static CongoColor getActivePlayerColor(string color)
		{
			if (color != "w" && color != "b") { return null; }

			return color == "w" ? White.Color : Black.Color;
		}

		private static MonkeyJump getFirstMonkeyJump(string input)
		{
			var upperBound = CongoBoard.Empty.Size * CongoBoard.Empty.Size;
			if (int.TryParse(input, out int from) && from >= -1 && from < upperBound) {
				return new MonkeyJump(from, -1, -1);
			}

			return null;
		}

		private static CongoBoard addPiece(CongoBoard board, CongoColor color,
			char pieceView, int square, ref int file)
		{
			file++;

			return board.With(color, pieceViews[pieceView], square);
		}

		private static CongoPlayer getPlayer(CongoBoard board,
			string type, CongoColor color, MonkeyJump firstMonkeyJump)
		{
			if (type != "a" && type != "h") { return null; }

			var playerType = type == "a" ? typeof(Ai) : typeof(Hi);

			return CongoPlayer.GetByType(board, playerType, color, firstMonkeyJump);
		}

		public static CongoGame FromFen(string fen)
		{
			var seps = new char[] { '/' };
			var sfen = fen.Split(seps, StringSplitOptions.RemoveEmptyEntries);

			CongoColor activePlayerColor;
			MonkeyJump whiteFirstMonkeyJump;
			MonkeyJump blackFirstMonkeyJump;

			if (sfen.Length != 11) { return null; }

			activePlayerColor = getActivePlayerColor(sfen[9]);
			if (activePlayerColor == null) { return null; }

			var firstMonkeyJump = getFirstMonkeyJump(sfen[10]);
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
					else if (pieceSignatures.ToUpper().IndexOf(sfen[rank][i]) >= 0) {
						board = addPiece(board, White.Color, (char)(sfen[rank][i] - 'A' + 'a'),
							rank * board.Size + file, ref file);
					}

					// black pieces
					else if (pieceSignatures.ToLower().IndexOf(sfen[rank][i]) >= 0) {
						board = addPiece(board, Black.Color, sfen[rank][i],
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

			var whitePlayer = getPlayer(board, sfen[7], White.Color, whiteFirstMonkeyJump);
			var blackPlayer = getPlayer(board, sfen[8], Black.Color, blackFirstMonkeyJump);
			if (whitePlayer == null || blackPlayer == null) { return null; }

			var activePlayer = activePlayerColor.IsWhite() ? whitePlayer : blackPlayer;

			return CongoGame.Unattached(board, whitePlayer, blackPlayer, activePlayer, firstMonkeyJump);
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

			result += game.FirstMonkeyJump == null
				? (-1).ToString()
				: game.FirstMonkeyJump.Fr.ToString();

			return result;
		}
	}
}
