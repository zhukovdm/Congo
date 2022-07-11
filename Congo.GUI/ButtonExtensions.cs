using System.Windows;
using System.Windows.Controls;

namespace Congo.GUI
{
    public static class ButtonExtensions
    {
        public static void PerformClick(this Button button)
            => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, button));
    }
}
