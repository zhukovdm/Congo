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
        public static readonly ImmutableDictionary<Type, string> type2suffix = new Dictionary<Type, string>
        {
            { typeof(Crocodile), "crocodile" }, { typeof(Elephant), "elephant" },
            { typeof(Giraffe), "giraffe" }, { typeof(Lion), "lion" }, { typeof(Monkey), "monkey" },
            { typeof(Pawn), "pawn" }, { typeof(Superpawn), "super-pawn" }, { typeof(Zebra), "zebra" }
        }.ToImmutableDictionary();

        public static Canvas addMoveFrBorder(this Canvas tile)
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

        public static Canvas addMoveToBorder(this Canvas tile)
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

        public static Canvas addStandardBorder(this Canvas tile)
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

        public static Canvas addImage(this Canvas tile, CongoColor color, Type type)
        {
            var pfx = (color == White.Color) ? "white" : "black";
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
        private int moveFrom;
        private bool ai; // atomic read/write

        private void replaceTile(int idx, Canvas tile)
        {
            wrapPanelCongoBoard.Children.RemoveAt(idx);
            wrapPanelCongoBoard.Children.Insert(idx, tile);
        }

        private IEnumerable<CongoMove> getMovesFrom()
        {
            return from move in game.ActivePlayer.Moves
                   where move.Fr == moveFrom
                   select move;
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

        private Canvas selectEmptyTile(int idx)
            => getEmptyTile(idx).addMoveToBorder();

        private Canvas unselectEmptyTile(int idx)
            => getEmptyTile(idx).addStandardBorder();

        private Canvas createImageTile(CongoColor color, int idx)
        {
            return getEmptyTile(idx)
                .addImage(color, game.Board.GetPiece(idx).GetType())
                .addStandardBorder();
        }

        private Canvas selectImageTile(CongoColor color, int idx)
        {
            var tile = getEmptyTile(idx).addImage(color, game.Board.GetPiece(idx).GetType());
            return game.ActivePlayer.Color == color
                ? tile.addMoveFrBorder()
                : tile.addMoveToBorder();
        }

        private Canvas unselectImageTile(CongoColor color, int idx)
            => createImageTile(color, idx);

        private void selectTilesPossibleMoves()
        {
            foreach (var move in getMovesFrom()) {

                Canvas newTile = game.Board.IsOccupied(move.To)
                    ? selectImageTile(game.Opponent.Color, move.To)
                    : selectEmptyTile(move.To);

                replaceTile(move.To, newTile);
            }
        }

        private void unselectTilesPossibleMoves()
        {
            foreach (var move in getMovesFrom()) {

                Canvas newTile = game.Board.IsOccupied(move.To)
                    ? unselectImageTile(game.Opponent.Color, move.To)
                    : unselectEmptyTile(move.To);

                replaceTile(move.To, newTile);
            }
        }

        private void moveTile(int Fr, int To)
        {
            var newTile = getEmptyTile(To)
                .addImage(game.ActivePlayer.Color, game.Board.GetPiece(Fr).GetType())
                .addStandardBorder();
            replaceTile(Fr, getEmptyTile(Fr).addStandardBorder());
            replaceTile(To, newTile);
        }

        /// <summary>
        /// Handler for clicks on all game graphical tiles.
        /// </summary>
        private void tile_Click(object sender, RoutedEventArgs e)
        {
            if (ai) { return; }

            switch (state) {

                case State.INIT:
                    break;

                case State.FR:

                    if (sender is Canvas oldTileFrom) {

                        moveFrom = int.Parse((string)oldTileFrom.Tag);

                        // act on non-empty tiles with friendly piece
                        if (game.Board.IsOccupied(moveFrom) && game.Board.IsFriendlyPiece(game.ActivePlayer.Color, moveFrom)) {

                            // activate tile
                            replaceTile(moveFrom, selectImageTile(game.ActivePlayer.Color, moveFrom));

                            // possible move ~ empty tile or opponent's piece
                            selectTilesPossibleMoves();

                            state = State.TO;
                        }
                    }
                    break;

                case State.TO:

                    if (sender is Canvas oldTileTo) {

                        var moveTo = int.Parse((string)oldTileTo.Tag);

                        // reset
                        if (moveFrom == moveTo) {

                            replaceTile(moveFrom, unselectImageTile(game.ActivePlayer.Color, moveFrom));
                            unselectTilesPossibleMoves();

                            state = State.FR;
                        }

                        // move is valid ~> move
                        else {
                            var result = false;
                            foreach (var move in getMovesFrom()) { result |= move.To == moveTo; }

                            if (result) {

                                unselectTilesPossibleMoves();

                                // replace target tile with moved
                                moveTile(moveFrom, moveTo);

                                if (game.ActivePlayer.Color == White.Color) {
                                    borderWhitePlayer.BorderBrush = Brushes.Transparent;
                                    borderBlackPlayer.BorderBrush = Brushes.Red;
                                } else {
                                    borderWhitePlayer.BorderBrush = Brushes.Red;
                                    borderBlackPlayer.BorderBrush = Brushes.Transparent;
                                }
                                game = game.Transition(game.ActivePlayer.Accept(new CongoMove(moveFrom, moveTo)));

                                listBoxMoves.Items.Add("(" + moveViews[moveFrom] + ", " + moveViews[moveTo] + ")");
                                listBoxMoves.ScrollIntoView(listBoxMoves.Items[listBoxMoves.Items.Count - 1]);

                                state = game.HasEnded() ? State.END : State.FR;

                                if (state == State.END) {
                                    MessageBox.Show($"{game.Opponent.Color} wins.");
                                    localMenuButton.IsEnabled = true;
                                    networkMenuButton.IsEnabled = true;
                                }
                            }

                            // otherwise, do nothing
                        }
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

        private void exitMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void buttonAdvice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonAiMove_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Draws pieces of a certain color via replacing prepared tiles.
        /// </summary>
        private void drawPieces(CongoColor color)
        {
            var e = game.Board.GetEnumerator(color);
            while (e.MoveNext()) { replaceTile(e.Current, createImageTile(color, e.Current)); }
        }

        private void initBoard()
        {
            wrapPanelCongoBoard.Children.RemoveRange(0, wrapPanelCongoBoard.Children.Count);

            for (int i = 0; i < boardSize; ++i) {
                wrapPanelCongoBoard.Children.Add(getEmptyTile(i).addStandardBorder());
            }
        }

        private void initGame()
        {
            state = State.FR;
            localMenuButton.IsEnabled = false;
            networkMenuButton.IsEnabled = false;

            initBoard();
            drawPieces(White.Color);
            drawPieces(Black.Color);

            var border = game.ActivePlayer.Color == White.Color
                ? borderWhitePlayer
                : borderBlackPlayer;
            border.BorderBrush = Brushes.Red;
        }

        public MainWindow()
        {
            InitializeComponent();

            state = State.INIT;

            dockPanelWhiteColor.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(whiteColorCode);
            dockPanelBlackColor.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(blackColorCode);

            initBoard();
        }
    }
}
