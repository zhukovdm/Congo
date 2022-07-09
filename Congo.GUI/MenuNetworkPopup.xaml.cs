using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Grpc.Net.Client;
using Congo.Core;
using Congo.Server;
using Congo.Utils;

namespace Congo.GUI
{
    public partial class MenuNetworkPopup : Window, IPlayable
    {
        public CongoGame Game { get; private set; }
        public CongoUser White { get; private set; }
        public CongoUser Black { get; private set; }

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox && !textBox.IsReadOnly && e.KeyboardDevice.IsKeyDown(Key.Tab)) {
                textBox.SelectAll();
            }
        }

        static MenuNetworkPopup()
        {
            EventManager.RegisterClassHandler(typeof(TextBox), GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
        }

        private void textBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox) {
                textBox.BorderBrush = (textBox.Text == string.Empty)
                    ? Brushes.Red
                    : (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            }
        }

        private void esc_PushButton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Close(); }
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

        private void radioButtonId_Checked(object sender, RoutedEventArgs e)
        {
            textBoxId.Visibility = Visibility.Visible;
            textBoxId.Focus();
        }

        private void radioButtonId_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxId.Visibility = Visibility.Hidden;
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            string error;
            var errorBuf = new StringWriter();

            #region check user input

            if (radioButtonFen.IsChecked == true && CongoFen.FromFen(textBoxFen.Text) == null) {
                errorBuf.WriteLine("Fen is selected, but valid CongoFen string is not provided.");
            }

            if (radioButtonId.IsChecked == true && !UserInput.IsBoardIdValid(textBoxId.Text)) {
                errorBuf.WriteLine("Id is selected, but valid gameId is not provided.");
            }

            if (!UserInput.IsIpAddressValid(textBoxHost.Text)) {
                errorBuf.WriteLine("Entered host address is invalid.");
            }

            if (!UserInput.IsPortValid(textBoxPort.Text)) {
                errorBuf.WriteLine("Entered port is invalid.");
            }

            error = errorBuf.ToString();

            if (error != string.Empty) {
                MessageBox.Show(error, "Wrong input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            #region create network primitives

            GrpcChannel channel = null;
            CongoGrpc.CongoGrpcClient client = null;

            // no exceptions could arise while creating channel and client
            if (errorBuf.ToString() == string.Empty) {
                channel = NetworkPrimitives.CreateRpcChannel(textBoxHost.Text, textBoxPort.Text);
                client = new CongoGrpc.CongoGrpcClient(channel);
            }

            #endregion

            #region check game

            long gameId = -1;

            try {

                if (radioButtonStandard.IsChecked == true) {
                    gameId = client.PostBoard(new PostBoardRequest() { Board = CongoFen.ToFen(CongoGame.Standard()) }).GameId;
                }

                if (radioButtonFen.IsChecked == true) {
                    gameId = client.PostBoard(new PostBoardRequest() { Board = textBoxFen.Text }).GameId;
                }
            } catch (Exception) {
                errorBuf.WriteLine("New board cannot be posted on the server.");
            }

            if (radioButtonId.IsChecked == true) {
                gameId = long.Parse(textBoxId.Text);
            }

            try {
                if (!client.CheckGameId(new CheckGameIdRequest() { GameId = gameId}).Exist) {
                    errorBuf.WriteLine("gameId doesn't exist on the server.");
                }
            } catch (Exception) {
                errorBuf.WriteLine("gameId cannot be checked on the server.");
            }

            #endregion

            error = errorBuf.ToString();

            if (error != string.Empty) {
                MessageBox.Show(error, "Wrong communication", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public MenuNetworkPopup()
        {
            InitializeComponent();
            PreviewKeyDown += new KeyEventHandler(esc_PushButton);
        }
    }
}
