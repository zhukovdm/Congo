using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public static Canvas addActiveBorder(this Canvas tile)
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

        public static Canvas addPassiveBorder(this Canvas tile)
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
    using EmptyTiles = ImmutableArray<Canvas>;
    using ImageTiles = ImmutableDictionary<Type, ImmutableArray<Canvas>>;

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

        private readonly EmptyTiles emptyTiles;
        private readonly ImageTiles activeWhiteTiles;
        private readonly ImageTiles activeBlackTiles;
        private readonly ImageTiles passiveWhiteTiles;
        private readonly ImageTiles passiveBlackTiles;

        private CongoGame game;
        private CongoUser white;
        private CongoUser black;

        private void tile_Click(object sender, RoutedEventArgs e)
        {
            /*
            var canvas = (Canvas)sender;
            foreach (var elem in canvas.Children)
            {
                var b = elem as Border;
                if (b is Border)
                {
                    string code = "#a000cc";

                    b.BorderBrush = (b.BorderBrush == Brushes.Black)
                        ? (SolidColorBrush)new BrushConverter().ConvertFromString(code)
                        : Brushes.Black;

                    b.BorderThickness = (b.BorderThickness == new Thickness(1))
                        ? new Thickness(5)
                        : new Thickness(1);
                }
            }
            */
        }

        private void localMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MenuLocalPopup();
            if (dialog.ShowDialog() == true) {
                game = dialog.game;
                white = dialog.white;
                black = dialog.black;
                init();
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
            while (e.MoveNext()) {
                var type = game.Board.GetPiece(e.Current).GetType();
                var tiles = (color == White.Color)
                    ? passiveWhiteTiles
                    : passiveBlackTiles;
                wrapPanelCongoBoard.Children.RemoveAt(e.Current);
                wrapPanelCongoBoard.Children.Insert(e.Current, tiles[type][e.Current]);
            }
        }

        private void drawBoard()
        {
            drawPieces(White.Color);
            drawPieces(Black.Color);
        }

        private void init()
        {
            localMenuButton.IsEnabled = false;
            networkMenuButton.IsEnabled = false;
            drawBoard();
        }

        private void step()
        {
            
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

        private EmptyTiles generateEmptyPassiveTiles()
        {
            var tiles = new Canvas[boardSize];
            for (int i = 0; i < boardSize; ++i) { tiles[i] = getEmptyTile(i).addPassiveBorder(); }
            return tiles.ToImmutableArray();
        }

        private ImageTiles generatePassiveImageTiles(CongoColor color)
        {
            var result = new Dictionary<Type, EmptyTiles>();

            var types = TileExtensions.type2suffix.Keys;
            foreach (var type in types) {
                var set = new List<Canvas>();
                for (int i = 0; i < boardSize; ++i) {
                    set.Add(getEmptyTile(i).addImage(color, type).addPassiveBorder());
                }
                result[type] = set.ToImmutableArray();
            }

            return result.ToImmutableDictionary();
        }

        public MainWindow()
        {
            InitializeComponent();

            dockPanelWhiteColor.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(whiteColorCode);
            dockPanelBlackColor.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(blackColorCode);

            emptyTiles = generateEmptyPassiveTiles();
            passiveWhiteTiles = generatePassiveImageTiles(White.Color);
            passiveBlackTiles = generatePassiveImageTiles(Black.Color);

            foreach (var tile in emptyTiles) { wrapPanelCongoBoard.Children.Add(tile); }
        }
    }
}
