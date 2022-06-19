using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
    /// <summary>
    /// Simplified Fen format
    ///  - 7x ranks,
    ///  - 1x active player color,
    ///  - 1x first monkey jump from
    ///     <code>rank/rank/rank/rank/rank/rank/rank/color/jump</code>
    /// 
    /// Fen for standard board
    ///     <code>gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/w/-1</code>
    /// </summary>
    public static class CongoFen
    {
        private static readonly int fenLength = 9;
        private static readonly int activePlayerIdx = 7;
        private static readonly int monkeyJumpIdx = 8;
        private static readonly string pieceSignatures = "gmelczps";

        private static readonly ImmutableDictionary<char, CongoPiece> view2piece =
            new Dictionary<char, CongoPiece>() {
                { 'g', Giraffe.Piece   }, { 'm', Monkey.Piece    },
                { 'e', Elephant.Piece  }, { 'l', Lion.Piece      },
                { 'c', Crocodile.Piece }, { 'z', Zebra.Piece     },
                { 'p', Pawn.Piece      }, { 's', Superpawn.Piece }
            }.ToImmutableDictionary();

        private static CongoColor GetActivePlayerColor(string color)
        {
            if (color != "w" && color != "b") { return null; }

            return color == "w" ? White.Color : Black.Color;
        }

        private static MonkeyJump GetFirstMonkeyJump(string input)
        {
            var upperBound = CongoBoard.Size * CongoBoard.Size;

            if (int.TryParse(input, out int from) && from >= -1 && from < upperBound) {
                return new MonkeyJump(from, -1, -1);
            }

            return null;
        }

        private static CongoBoard AddPiece(CongoBoard board, CongoColor color,
            char pieceView, int square, ref int file)
        {
            ++file;

            return board.With(color, view2piece[pieceView], square);
        }

        /// <summary>
        /// Deserialize game from <b>Fen</b> string. This method does not check
        /// if amount of pieces is satisfactory. The conversion is straight
        /// forward char -> piece.
        /// </summary>
        public static CongoGame FromFen(string fen)
        {
            var seps = new char[] { '/' };
            var sfen = fen.Split(seps, StringSplitOptions.RemoveEmptyEntries);

            CongoColor activePlayerColor;
            MonkeyJump whiteFirstMonkeyJump;
            MonkeyJump blackFirstMonkeyJump;

            if (sfen.Length != fenLength) { return null; }

            // parse color of the active player
            activePlayerColor = GetActivePlayerColor(sfen[activePlayerIdx]);
            if (activePlayerColor == null) { return null; }

            // parse possible position of the first monkey jump
            var firstMonkeyJump = GetFirstMonkeyJump(sfen[monkeyJumpIdx]);
            if (firstMonkeyJump == null) { return null; }
            if (firstMonkeyJump.Fr == -1) { firstMonkeyJump = null; }

            var board = CongoBoard.Empty;

            // parse board rank-by-rank
            for (int rank = 0; rank < CongoBoard.Size; ++rank) {
                int file = 0;

                for (int i = 0; i < sfen[rank].Length; ++i) {

                    // rank overflow
                    if (file >= CongoBoard.Size) { return null; }

                    // skip empty squares
                    if (sfen[rank][i] >= '1' && sfen[rank][i] <= '7') {
                        file += sfen[rank][i] - '0';
                    }

                    // white pieces
                    else if (pieceSignatures.ToUpper().Contains(sfen[rank][i])) {
                        board = AddPiece(board, White.Color, (char)(sfen[rank][i] - 'A' + 'a'),
                            rank * CongoBoard.Size + file, ref file);
                    }

                    // black pieces
                    else if (pieceSignatures.ToLower().Contains(sfen[rank][i])) {
                        board = AddPiece(board, Black.Color, sfen[rank][i],
                            rank * CongoBoard.Size + file, ref file);
                    }

                    // unknown signature
                    else { return null; }
                }

                // invalid input
                if (file != CongoBoard.Size) { return null; }
            }

            whiteFirstMonkeyJump = activePlayerColor.IsWhite() ? firstMonkeyJump : null;
            blackFirstMonkeyJump = activePlayerColor.IsBlack() ? firstMonkeyJump : null;

            var whitePlayer = new CongoPlayer(White.Color, board, whiteFirstMonkeyJump);
            var blackPlayer = new CongoPlayer(Black.Color, board, blackFirstMonkeyJump);
            if (whitePlayer == null || blackPlayer == null) { return null; }

            var activePlayer = activePlayerColor.IsWhite() ? whitePlayer : blackPlayer;

            return CongoGame.Unattached(board, whitePlayer, blackPlayer, activePlayer, firstMonkeyJump);
        }

        /// <summary>
        /// Serialize current game to @b Fen string. The conversion is straight
        /// forward piece -> char.
        /// </summary>
        public static string ToFen(CongoGame game)
        {
            var result = "";
            var sep = "/";

            var typeViews = new Dictionary<Type, string>() {
                { typeof(Elephant), "e" }, { typeof(Zebra),     "z" },
                { typeof(Giraffe),  "g" }, { typeof(Crocodile), "c" },
                { typeof(Pawn),     "p" }, { typeof(Superpawn), "s" },
                { typeof(Lion),     "l" }, { typeof(Monkey),    "m" },
                { typeof(White),    "w" }, { typeof(Black),     "b" }
            };

            for (int rank = 0; rank < CongoBoard.Size; ++rank) {
                int cnt = 0;

                for (int file = 0; file < CongoBoard.Size; ++file) {
                    var square = rank * CongoBoard.Size + file;
                    var piece = game.Board.GetPiece(square);

                    // animals
                    if (typeViews.ContainsKey(piece.GetType())) {

                        if (cnt > 0) { result += cnt.ToString(); }
                        
                        cnt = 0;
                        var repr = game.Board.IsWhitePiece(square)
                            ? typeViews[piece.GetType()].ToUpper()
                            : typeViews[piece.GetType()].ToLower();

                        result += repr;
                    }

                    // ground or river
                    else { cnt++; }
                }

                if (cnt > 0) { result += cnt.ToString(); }
                result += sep;
            }

            result += typeViews[game.ActivePlayer.Color.GetType()] + sep;

            result += game.FirstMonkeyJump == null
                ? (-1).ToString()
                : game.FirstMonkeyJump.Fr.ToString();

            return result;
        }
    }
}
