using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Congo.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UIElement tileFactory(int idx)
        {
            double size = 82;

            string code;
            if (idx >= 21 && idx < 28)
            {
                code = "#65b9f8";
            }
            else
            {
                switch (idx % 7)
                {
                    case 0:
                    case 1:
                    case 5:
                    case 6:
                        code = "#67de79";
                        break;
                    default:
                        code = "#f2d377";
                        break;
                }
            }

            var canvas = new Canvas
            {
                Width = size,
                Height = size,
                Background = (SolidColorBrush) new BrushConverter().ConvertFromString(code)
            };
            canvas.MouseUp += tile_Transform;

            var image = new Image
            {
                Width = size,
                Height = size,
                Source = new BitmapImage(new Uri(@"/Congo.GUI;component/Resources/pictures/white-crocodile.png", UriKind.Relative))
            };

            var border = new Border
            {
                Width = size,
                Height = size,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            canvas.Tag = idx.ToString();
            canvas.Children.Add(image);
            canvas.Children.Add(border);

            return canvas;
        }

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 49; i++)
            {
                panelCongoBoard.Children.Add(tileFactory(i));
            }
        }

        private void tile_Transform(object sender, RoutedEventArgs e)
        {
            var canvas = (Canvas)sender;
            foreach (var elem in canvas.Children)
            {
                var b = elem as Border;
                if (b is Border)
                {
                    b.BorderBrush = (b.BorderBrush == Brushes.Black) ? Brushes.Red : Brushes.Black;
                    b.BorderThickness = (b.BorderThickness == new Thickness(1)) ? new Thickness(5) : new Thickness(1);
                }
            }
        }

        private void exitMenuButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
