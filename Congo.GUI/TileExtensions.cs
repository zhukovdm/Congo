using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Congo.Core;

namespace Congo.GUI
{
    public static class TileExtensions
    {
        private const double accentBoardThickness = 5.0;
        private const double standardBoardThickness = 1.0;
        private static readonly double tileSize = MainWindow.tileSize;

        private static readonly ImmutableDictionary<Type, string> type2suffix = new Dictionary<Type, string>
        {
            { typeof(Superpawn), "super-pawn" }, { typeof(Monkey), "monkey" },
            { typeof(Crocodile), "crocodile"  }, { typeof(Zebra),  "zebra"  },
            { typeof(Elephant),  "elephant"   }, { typeof(Lion),   "lion"   },
            { typeof(Giraffe),   "giraffe"    }, { typeof(Pawn),   "pawn"   }
        }.ToImmutableDictionary();

        private static Canvas WithBorder(this Canvas tile, Brush brush, double thickness)
        {
            var border = new Border
            {
                Width = tileSize,
                Height = tileSize,
                BorderBrush = brush,
                BorderThickness = new Thickness(thickness)
            };
            _ = tile.Children.Add(border);

            return tile;
        }

        public static Canvas WithMoveFrBorder(this Canvas tile)
            => tile.WithBorder(Brushes.White, accentBoardThickness);

        public static Canvas WithMoveToBorder(this Canvas tile)
            => tile.WithBorder(Brushes.Red, accentBoardThickness);

        public static Canvas WithStandardBorder(this Canvas tile)
            => tile.WithBorder(Brushes.Black, standardBoardThickness);

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
            _ = tile.Children.Add(image);

            return tile;
        }
    }
}
