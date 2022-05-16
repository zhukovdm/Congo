using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Congo.Core;

namespace Congo.GUI
{
    /// <summary>
    /// Interaction logic for MenuLocalPopup.xaml
    /// </summary>
    public partial class MenuLocalPopup : Window
    {
        public CongoGame game;
        public CongoUser white;
        public CongoUser black;

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
            if (radioButtonFen.IsChecked == true && CongoFen.FromFen(textBoxFen.Text) == null)
            {
                textBoxFen.BorderBrush = Brushes.Red;
                return;
            }

            textBoxFen.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");

            game = radioButtonStandardGame.IsChecked == true
                ? CongoGame.Standard()
                : CongoFen.FromFen(textBoxFen.Text);

            /* begin of weird piece of code due to C# 7.3 language limitations,
             * which is true for all .NET Framework projects.*/

            var dict = new Dictionary<string, AlgorithmDelegate>
            {
                { "negamax", Algorithm.Negamax },
                { "rnd", Algorithm.Negamax }
            };
            var white_algo = radioButtonWhiteNegamax.IsChecked == true ? dict["negamax"] : dict["rnd"];
            white = (radioButtonWhiteHi.IsChecked == true) ? new Hi(white_algo) : new Ai(white_algo) as CongoUser;
            var black_algo = radioButtonBlackNegamax.IsChecked == true ? dict["negamax"] : dict["rnd"];
            black = (radioButtonWhiteHi.IsChecked == true) ? new Hi(black_algo) : new Ai(black_algo) as CongoUser;

            // end of weird piece of code

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
