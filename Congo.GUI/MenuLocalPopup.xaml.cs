using Congo.Core;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace Congo.GUI
{
    public partial class MenuLocalPopup : MenuBasePopup
    {
        #region event control

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(buttonConfirm);
        }

        private void radioButtonFen_Checked(object sender, RoutedEventArgs e)
        {
            textBoxFen.Visibility = Visibility.Visible;
            textBoxFen.Focus();
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

            NetworkPack = null;

            DialogResult = true;
            Close();
        }

        #endregion

        public MenuLocalPopup()
        {
            InitializeComponent();
        }
    }
}
