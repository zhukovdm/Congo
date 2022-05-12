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
using System.Windows.Shapes;

namespace Congo.GUI
{
    /// <summary>
    /// Interaction logic for MenuNetworkPopup.xaml
    /// </summary>
    public partial class MenuNetworkPopup : Window
    {
        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !textBox.IsReadOnly && e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                textBox.SelectAll();
            }
        }

        static MenuNetworkPopup()
        {
            EventManager.RegisterClassHandler(typeof(TextBox), GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
        }

        private void esc_PushButton(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Close(); }
        }

        public MenuNetworkPopup()
        {
            InitializeComponent();
            textBoxUserName.Focus();
            PreviewKeyDown += new KeyEventHandler(esc_PushButton);
        }
    }
}
