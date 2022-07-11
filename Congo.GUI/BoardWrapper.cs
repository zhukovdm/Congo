using Congo.Core;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Congo.GUI
{
    internal sealed class BoardWrapper : BaseWrapper
    {
        public const double TileSize = 82.0;
        private const string riverColorCode = "#65b9f8";
        private const string groundColorCode = "#67de79";
        private const string castleColorCode = "#f2d377";
        private static readonly int boardSize = CongoBoard.Size * CongoBoard.Size;
        private readonly WrapPanel panel;
        private readonly MouseButtonEventHandler handler;

        /// <summary>
        /// Constructs empty tile with background and <b>event handler</b>.
        /// @note Event handler is a reason the method cannot be static.
        /// </summary>
        private Canvas createEmptyTile(int idx)
        {
            string code;

            if (idx >= 21 && idx < 28) {
                code = riverColorCode;
            }

            else {
                code = (idx % 7) switch
                {
                    0 or
                    1 or
                    5 or
                    6 => groundColorCode,
                    _ => castleColorCode,
                };
            }

            var canvas = new Canvas
            {
                Width = TileSize,
                Height = TileSize,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString(code),
                Tag = idx.ToString()
            };
            canvas.MouseUp += handler;

            return canvas;
        }

        private void replaceTile(int idx, Canvas tile)
        {
            panel.Children.RemoveAt(idx);
            panel.Children.Insert(idx, tile);
        }

        private void drawPieces(CongoBoard board, CongoColor color)
        {
            var e = board.GetEnumerator(color);

            while (e.MoveNext()) {
                var tile = createEmptyTile(e.Current)
                    .WithPiece(color, board.GetPiece(e.Current).GetType())
                    .WithStandardBorder();
                replaceTile(e.Current, tile);
            }
        }

        /// <summary>
        /// Draw selections on the tiles based on game state.
        /// @note Selections are possible only if <b>state == State.TO</b>!
        /// </summary>
        private void drawSelect(CongoGame game, MainState state, int fr)
        {
            if (state == MainState.TO) {
                foreach (var move in game.GetMovesFrom(fr)) {

                    // excludes monkey interrupt vs. ordinary selection
                    if (move.Fr != move.To) {
                        panel.Children[move.To] = ((Canvas)panel.Children[move.To]).WithMoveToBorder();
                    }
                }

                var tile = (Canvas)panel.Children[fr];

                panel.Children[fr] = (game.FirstMonkeyJump == null)
                    ? tile.WithMoveFrBorder()
                    : tile.WithMoveToBorder();
            }
        }

        public BoardWrapper(WrapPanel panel, MouseButtonEventHandler handler)
        {
            this.panel = panel;
            this.handler = handler;
        }

        public override void Init()
        {
            panel.Children.RemoveRange(0, panel.Children.Count);

            for (int i = 0; i < boardSize; ++i) {
                panel.Children.Add(createEmptyTile(i).WithStandardBorder());
            }
        }

        public void Draw(CongoGame game, MainState state, int fr)
        {
            Reset();
            drawPieces(game.Board, White.Color);
            drawPieces(game.Board, Black.Color);
            drawSelect(game, state, fr);
        }

        public override void Reset() => Init();
    }
}
