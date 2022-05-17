using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Congo.Core;

namespace TileExtensions
{
    public static class TileExtensions
    {
        private static readonly double tileSize = Congo.GUI.MainWindow.tileSize;

        private static readonly ImmutableDictionary<Type, string> type2suffix = new Dictionary<Type, string>
        {
            { typeof(Giraffe), "giraffe" }, { typeof(Monkey), "monkey" },
            { typeof(Crocodile), "crocodile" }, { typeof(Zebra), "zebra" },
            { typeof(Lion), "lion" }, { typeof(Elephant), "elephant" },
            { typeof(Pawn), "pawn" }, { typeof(Superpawn), "super-pawn" }, 
        }.ToImmutableDictionary();

        public static Canvas WithMoveFrBorder(this Canvas tile)
        {
            var border = new Border
            {
                Width = tileSize,
                Height = tileSize,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(3)
            };
            tile.Children.Add(border);

            return tile;
        }

        public static Canvas WithMoveToBorder(this Canvas tile)
        {
            var border = new Border
            {
                Width = tileSize,
                Height = tileSize,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(3)
            };
            tile.Children.Add(border);

            return tile;
        }

        public static Canvas WithStandardBorder(this Canvas tile)
        {
            var border = new Border
            {
                Width = tileSize,
                Height = tileSize,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            tile.Children.Add(border);

            return tile;
        }

        public static Canvas WithPiece(this Canvas tile, CongoColor color, Type type)
        {
            var pfx = color.IsWhite() ? "white" : "black";
            var sfx = type2suffix[type];
            var ext = ".png";

            var image = new Image
            {
                Width = tileSize,
                Height = tileSize,
                Source = new BitmapImage(new Uri("/Congo.GUI;component/Resources/" + pfx + "-" + sfx + ext, UriKind.Relative))
            };

            tile.Children.Add(image);

            return tile;
        }
    }
}

namespace Congo.GUI
{
    using TileExtensions;

    enum State : int
    {
        INIT,
        FR,
        TO,
        END
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly double tileSize = 82.0;
        private static readonly int boardSize = CongoBoard.Empty.Size * CongoBoard.Empty.Size;
        private static readonly string whiteColorCode  = "#914800";
        private static readonly string blackColorCode  = "#000000";
        private static readonly string riverColorCode  = "#65b9f8";
        private static readonly string groundColorCode = "#67de79";
        private static readonly string castleColorCode = "#f2d377";

        private static readonly ImmutableList<string> moveViews =
            new List<string> {
                "a7", "b7", "c7", "d7", "e7", "f7", "g7",
                "a6", "b6", "c6", "d6", "e6", "f6", "g6",
                "a5", "b5", "c5", "d5", "e5", "f5", "g5",
                "a4", "b4", "c4", "d4", "e4", "f4", "g4",
                "a3", "b3", "c3", "d3", "e3", "f3", "g3",
                "a2", "b2", "c2", "d2", "e2", "f2", "g2",
                "a1", "b1", "c1", "d1", "e1", "f1", "g1"
            }.ToImmutableList();

        private CongoGame game;
        private CongoUser white;
        private CongoUser black;
        private State state;

        // TODO: move position should be abstracted away
        private int moveFr;

        // NOTE: atomic r/w on bool
        private bool ai;

        private IEnumerable<CongoMove> getMovesFr(int fr)
        {
            return from move in game.ActivePlayer.Moves
                   where move.Fr == fr
                   select move;
        }

        private void replaceTile(int idx, Canvas tile)
        {
            panelCongoBoard.Children.RemoveAt(idx);
            panelCongoBoard.Children.Insert(idx, tile);
        }

        /// <summary>
        /// Constructs empty tile with background and <b>event handler</b>.
        /// @note Event handler is a reason the method cannot be static.
        /// </summary>
        private Canvas getEmptyTile(int idx)
        {
            string code;
            if (idx >= 21 && idx < 28) {
                code = riverColorCode;
            }
            else {
                switch (idx % 7) {
                    case 0:
                    case 1:
                    case 5:
                    case 6:
                        code = groundColorCode;
                        break;
                    default:
                        code = castleColorCode;
                        break;
                }
            }

            var canvas = new Canvas
            {
                Width = tileSize,
                Height = tileSize,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString(code)
            };

            canvas.Tag = idx.ToString();
            canvas.MouseUp += tile_Click;
            return canvas;
        }

        private Canvas createImageTile(CongoColor color, int idx)
        {
            return getEmptyTile(idx)
                .WithPiece(color, game.Board.GetPiece(idx).GetType())
                .WithStandardBorder();
        }

        private string getMoveView(CongoMove move)
        {
            if (move is MonkeyJump jump) {
                return "(" + moveViews[jump.Fr] + ", " + moveViews[jump.Bt] + ", " + moveViews[jump.To] + ")";
            }

            else {
                return "(" + moveViews[move.Fr] + ", " + moveViews[move.To] + ")";
            }
        }

        private void appendMove(CongoMove move)
        {
            listBoxMoves.Items.Add(getMoveView(move));
            listBoxMoves.ScrollIntoView(listBoxMoves.Items[listBoxMoves.Items.Count - 1]);
        }

        /// <summary>
        /// Event handler for clicks on all board tiles.
        /// </summary>
        private void tile_Click(object sender, RoutedEventArgs e)
        {
            if (ai) { return; }

            switch (state) {

                case State.INIT:
                    break;

                case State.FR:

                    if (sender is Canvas tileFrom) {
                        moveFr = int.Parse((string)tileFrom.Tag);
                        if (game.Board.IsOccupied(moveFr) &&
                            game.Board.IsFriendlyPiece(game.ActivePlayer.Color, moveFr)) {
                            state = State.TO;
                            drawGame();
                        }
                    }
                    break;

                case State.TO:

                    if (sender is Canvas tileTo) {

                        var moveTo = int.Parse((string)tileTo.Tag);
                        var move = game.ActivePlayer.Accept(new CongoMove(moveFr, moveTo));

                        /* Move exists, also includes monkey jump interrupt,
                         * when Fr == To! */
                        if (move != null) {
                            game = game.Transition(move);

                            var option = (move is MonkeyJump) ? State.TO : State.FR;
                            state = game.HasEnded() ? State.END : option;

                            // very dangerous row
                            if (move is MonkeyJump) { moveFr = moveTo; }

                            appendMove(move);
                            drawGame();
                        }

                        // no move, but reset
                        else if (moveFr == moveTo) {
                            state = State.FR;
                            drawGame();
                        }

                        else { /* do nothing */ }
                    }
                    break;

                case State.END:
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void localMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MenuLocalPopup();
            if (dialog.ShowDialog() == true) {
                game = dialog.game;
                white = dialog.white;
                black = dialog.black;
                initGame();
            }
        }

        private void networkMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MenuNetworkPopup().ShowDialog();
        }

        private void pauseMenuButton_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();

        private void resetMenuButton_Click(object sender, RoutedEventArgs e) => resetGame();

        private void exitMenuButton_Click(object sender, RoutedEventArgs e) => exitGame();

        private void buttonAdvice_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();

        private void buttonAiMove_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();

        private void initEmptyBoard()
        {
            panelCongoBoard.Children.RemoveRange(0, panelCongoBoard.Children.Count);

            for (int i = 0; i < boardSize; ++i) {
                panelCongoBoard.Children
                    .Add(getEmptyTile(i)
                    .WithStandardBorder());
            }
        }

        private void resetBoard()
            => initEmptyBoard();

        private void drawPieces(CongoColor color)
        {
            var e = game.Board.GetEnumerator(color);
            while (e.MoveNext()) {
                replaceTile(e.Current, createImageTile(color, e.Current));
            }
        }

        /// <summary>
        /// TODO: drawSelect()
        /// @note Selections are possible only if <b>state == State.TO</b>!
        /// </summary>
        private void drawSelect()
        {
            if (state == State.TO) {
                foreach (var move in getMovesFr(moveFr)) {

                    // excludes monkey interrupt vs. ordinary selection
                    if (move.Fr != move.To) {
                        panelCongoBoard.Children[move.To] = ((Canvas)panelCongoBoard.Children[move.To]).WithMoveToBorder();
                    }
                }

                panelCongoBoard.Children[moveFr] = (game.FirstMonkeyJump == null)
                    ? ((Canvas)panelCongoBoard.Children[moveFr]).WithMoveFrBorder()
                    : ((Canvas)panelCongoBoard.Children[moveFr]).WithMoveToBorder();
            }
        }

        private void drawBoard()
        {
            initEmptyBoard();
            drawPieces(White.Color);
            drawPieces(Black.Color);
            drawSelect();
        }

        private void setPlayerBorder()
        {
            var w = game.ActivePlayer.Color.IsWhite();

            borderWhitePlayer.BorderBrush = w ? Brushes.Red : Brushes.Transparent;
            borderBlackPlayer.BorderBrush = w ? Brushes.Transparent : Brushes.Red;
        }

        /// <summary>
        /// Draw user interface based on @b game and @b state class members.
        /// </summary>
        private void drawGame()
        {
            drawBoard();
            setPlayerBorder();

            if (state == State.END) {
                borderWhitePlayer.BorderBrush = Brushes.Transparent;
                borderBlackPlayer.BorderBrush = Brushes.Transparent;

                var c = game.Opponent.Color.IsWhite() ? "White" : "Black";
                MessageBox.Show($"{c} wins.");
                localMenuButton.IsEnabled = true;
                networkMenuButton.IsEnabled = true;
            }
        }

        private void initGame()
        {
            localMenuButton.IsEnabled = false;
            networkMenuButton.IsEnabled = false;

            if (game.HasEnded()) { state = State.END; }

            else if (game.FirstMonkeyJump != null) { state = State.TO; }

            else { state = State.FR; }

            drawGame();
        }

        private void resetGui()
        {
            resetBoard();
            borderWhitePlayer.BorderBrush = Brushes.Transparent;
            borderBlackPlayer.BorderBrush = Brushes.Transparent;
            listBoxMoves.Items.Clear();
            textBlockStatus.Text = "";
            textBlockAdvice.Text = "";
            localMenuButton.IsEnabled = true;
            networkMenuButton.IsEnabled = true;
        }

        private void resetGame()
        {
            // code entities
            game = null;
            white = null;
            black = null;
            state = State.INIT;

            resetGui();
        }

        private void exitGame()
            => Application.Current.Shutdown(); // TODO: Congo.Gui.WainWindow.exitGame() shall be smarter for network games!

        public MainWindow()
        {
            InitializeComponent();

            ai = false;

            panelWhitePlayer.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(whiteColorCode);
            panelBlackPlayer.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(blackColorCode);

            resetGame();
        }
    }
}
