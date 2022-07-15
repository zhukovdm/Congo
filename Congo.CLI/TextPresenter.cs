using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Congo.Core;
using Congo.Server;
using Congo.Utils;

namespace Congo.CLI
{
    public sealed class TextPresenter
    {
        private static readonly ImmutableList<Type> pieceTypes =
            new Type[] {
                typeof(Ground), typeof(River), typeof(Elephant), typeof(Zebra),
                typeof(Giraffe), typeof(Crocodile), typeof(Pawn),
                typeof(Superpawn), typeof(Lion), typeof(Monkey),
        }.ToImmutableList();

        private static readonly ImmutableDictionary<Type, string> pieceViews =
            new Dictionary<Type, string>() {
                { typeof(Ground),   "-" }, { typeof(River),     "+" },
                { typeof(Elephant), "e" }, { typeof(Zebra),     "z" },
                { typeof(Giraffe),  "g" }, { typeof(Crocodile), "c" },
                { typeof(Pawn),     "p" }, { typeof(Superpawn), "s" },
                { typeof(Lion),     "l" }, { typeof(Monkey),    "m" },
            }.ToImmutableDictionary();

        public const string BoardLiteral = "board";
        public const string GameLiteral = "game";
        public const string MovesLiteral = "moves";
        public const string PlayersLiteral = "players";

        public static readonly ImmutableHashSet<string> SupportedShows =
            new HashSet<string>() {
                BoardLiteral,
                PlayersLiteral,
                MovesLiteral,
                GameLiteral
            }.ToImmutableHashSet();

        private static int[] countPieces(CongoBoard board, CongoColor color)
        {
            var counter = new int[pieceTypes.Count];
            var enumerator = board.GetEnumerator(color);

            while (enumerator.MoveNext()) {
                var type = board.GetPiece(enumerator.Current).GetType();
                ++counter[pieceTypes.IndexOf(type)];
            }

            return counter;
        }

        private readonly TextWriter writer;

        public TextPresenter(TextWriter writer)
        {
            this.writer = writer;
        }

        private void showPlayer(CongoBoard board, CongoColor color, CongoPlayer activePlayer)
        {
            var activeRepr = color == activePlayer.Color
                ? "*"
                : " ";

            var colorRepr = color.IsWhite()
                ? "white"
                : "black";

            var counter = countPieces(board, color);

            writer.Write($" {activeRepr} {colorRepr}");
            for (int i = 2; i < pieceTypes.Count; i++) {
                var pieceRepr = color.IsWhite()
                    ? pieceViews[pieceTypes[i]].ToUpper()
                    : pieceViews[pieceTypes[i]].ToLower();
                writer.Write($" {counter[i]}{pieceRepr}");
            }
            writer.WriteLine();
        }

        public void ShowTransition(CongoGame game)
        {
            writer.WriteLine();
            writer.WriteLine($" transition {MovePresenter.GetMoveView(game.TransitionMove)}");
        }

        public void ShowNetworkGameId(long gameId)
        {
            writer.WriteLine();
            writer.WriteLine($" network gameId {gameId}");
        }

        public void ShowBoard(CongoGame game)
        {
            writer.WriteLine();
            var upperBound = CongoBoard.Size * CongoBoard.Size;

            for (int square = 0; square < upperBound; square++) {
                if (square % CongoBoard.Size == 0) {
                    writer.Write($" {CongoBoard.Size - square / CongoBoard.Size}  ");
                }

                var pv = pieceViews[game.Board.GetPiece(square).GetType()];
                if (game.Board.IsFirstMovePiece(square)) pv = pv.ToUpper();
                writer.Write(" " + pv);
                if (square % CongoBoard.Size == CongoBoard.Size - 1) writer.WriteLine();
            }

            writer.WriteLine();
            writer.Write(" /  ");
            for (int i = 0; i < CongoBoard.Size; i++) {
                writer.Write(" " + ((char)('a' + i)).ToString());
            }
            writer.WriteLine();
        }

        public void ShowPlayers(CongoGame game)
        {
            writer.WriteLine();
            showPlayer(game.Board, White.Color, game.ActivePlayer);
            showPlayer(game.Board, Black.Color, game.ActivePlayer);
        }

        public void ShowMoves(CongoGame game)
        {
            writer.WriteLine();
            int cnt = 0;
            foreach (var move in game.ActivePlayer.Moves) {
                var repr = " " + MovePresenter.GetMoveView(move);
                if (cnt + repr.Length > 40) {
                    cnt = 0;
                    writer.WriteLine();
                }
                cnt += repr.Length;
                writer.Write(repr);
            }
            writer.WriteLine();
        }

        public void ShowGame(CongoGame game)
        {
            ShowBoard(game);
            ShowPlayers(game);
            ShowMoves(game);
        }

        public void ShowStep(CongoGame game)
        {
            ShowTransition(game);
            ShowBoard(game);
        }

        public void ShowNetworkTransitions(ICollection<DbMove> moves)
        {
            if (moves.Count > 0) {
                writer.WriteLine();
                writer.WriteLine(" transitions " + string.Join(" -> ", moves.Select(move => MovePresenter.GetMoveView(new CongoMove(move.Fr, move.To)))));
            }
        }
    }
}
