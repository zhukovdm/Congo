using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Congo.Core;
using Grpc.Net.Client;
using Congo.Server;

namespace Congo.GUI
{
    /// <summary>
    /// Interaction logic for MenuLocalPopup.xaml
    /// </summary>
    public partial class MenuLocalPopup : Window, IPlayable
    {
        public CongoGame Game { get; set; }
        public CongoUser WhiteUser { get; set; }
        public CongoUser BlackUser { get; set; }
        public CongoNetworkPack NetworkPack { get; set; }

        private void Esc_PushButton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Close(); }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(buttonConfirm);
        }

        private void radioButtonFen_Checked(object sender, RoutedEventArgs e)
        {
            textBoxFen.Visibility = Visibility.Visible;
        }

        private void radioButtonFen_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxFen.Visibility = Visibility.Hidden;
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (radioButtonFen.IsChecked == true && CongoFen.FromFen(textBoxFen.Text) == null) {
                textBoxFen.BorderBrush = Brushes.Red;
                return;
            }

            textBoxFen.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");

            Game = radioButtonStandardGame.IsChecked == true
                ? CongoGame.Standard()
                : CongoFen.FromFen(textBoxFen.Text);

            var white_algo = (radioButtonWhiteNegamax.IsChecked == true)
                ? Algorithm.Negamax
                : (AlgorithmDelegate) Algorithm.Random;

            WhiteUser = (radioButtonWhiteHi.IsChecked == true)
                ? new Hi(white_algo)
                : new Ai(white_algo);

            var black_algo = (radioButtonBlackNegamax.IsChecked == true)
                ? Algorithm.Negamax
                : (AlgorithmDelegate) Algorithm.Random;

            BlackUser = (radioButtonBlackHi.IsChecked == true)
                ? new Hi(black_algo)
                : new Ai(black_algo);

            DialogResult = true;
            Close();
        }

        public MenuLocalPopup()
        {
            InitializeComponent();
            PreviewKeyDown += new KeyEventHandler(Esc_PushButton);
        }
    }
}
