using Congo.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Congo.GUI
{
    public class MenuBasePopup : Window, IPlayable
    {
        private const string defaultBorderBrush = "#FFABADB3";

        public CongoGame Game { get; set; }
        public CongoUser WhiteUser { get; set; }
        public CongoUser BlackUser { get; set; }
        public PopupPack PopupPack { get; set; }

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox && !textBox.IsReadOnly && e.KeyboardDevice.IsKeyDown(Key.Tab)) {
                textBox.SelectAll();
            }
        }

        private void esc_PushButton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Close(); }
        }

        protected static void reportError(string err, string title)
        {
            _ = MessageBox.Show(err, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected void textBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox) {
                textBox.BorderBrush = (textBox.Text == string.Empty)
                    ? Brushes.Red
                    : (SolidColorBrush)new BrushConverter().ConvertFromString(defaultBorderBrush);
            }
        }

        static MenuBasePopup()
        {
            EventManager.RegisterClassHandler(typeof(TextBox), GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
        }

        public MenuBasePopup()
        {
            PreviewKeyDown += new KeyEventHandler(esc_PushButton);
        }
    }
}
