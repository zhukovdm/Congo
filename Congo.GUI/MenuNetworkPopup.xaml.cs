using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Congo.GUI
{
    public partial class MenuNetworkPopup : Window
    {
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

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            var msgBoxTitle = "Wrong input";

            var boxes = new List<TextBox> {
                textBoxUserName, textBoxIp1, textBoxIp2, textBoxIp3, textBoxIp4, textBoxPort
            };

            foreach (var b in boxes) {
                if (b.Text == string.Empty) {
                    var text = "Please, fill in all options.";
                    MessageBox.Show(text, msgBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            string spec = "";

            if (!Utils.UserInput.IsUserNameValid(textBoxUserName.Text)) {
                spec += "User name " + textBoxUserName.Text + " is invalid." + System.Environment.NewLine;
            }

            var ips = new List<string> {
                textBoxIp1.Text, textBoxIp2.Text, textBoxIp3.Text, textBoxIp4.Text
            };

            for (int i = 0; i < ips.Count; i++) {
                if (!Utils.UserInput.IsIpAddressHolderValid(ips[i])) {
                    spec += "IP address holder " + (i + 1) + ": " + ips[i] + " is invalid." + System.Environment.NewLine;
                }
            }

            if (!Utils.UserInput.IsPortValid(textBoxPort.Text)) {
                spec += "Port address " + textBoxPort.Text + " is invalid." + System.Environment.NewLine;
            }

            if (spec != string.Empty) {
                MessageBox.Show(spec, msgBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO: socket & network connection
        }

        private void TextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox) {
                textBox.BorderBrush = (textBox.Text == string.Empty)
                    ? Brushes.Red
                    : (SolidColorBrush)new BrushConverter().ConvertFromString("#FFABADB3");
            }
        }

        private void Esc_PushButton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Close(); }
        }

        public MenuNetworkPopup()
        {
            InitializeComponent();
            textBoxUserName.Focus();
            PreviewKeyDown += new KeyEventHandler(Esc_PushButton);
        }
    }
}
