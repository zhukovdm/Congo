using Congo.Core;
using System.Windows.Controls;
using System.Windows.Media;

namespace Congo.GUI
{
    internal class UserPanelWrapper
    {
        private static readonly SolidColorBrush activeBrush = Brushes.Red;
        private static readonly SolidColorBrush inactiveBrush = Brushes.Transparent;

        private readonly Border borderWhite, borderBlack;

        public UserPanelWrapper(Border borderWhite, Border borderBlack)
        {
            this.borderWhite = borderWhite;
            this.borderBlack = borderBlack;
        }

        public void Init()
        {
            borderWhite.BorderBrush = inactiveBrush;
            borderBlack.BorderBrush = inactiveBrush;
        }

        public void Draw(CongoGame game)
        {
            var w = game.ActivePlayer.IsWhite();

            borderWhite.BorderBrush = w ? activeBrush : inactiveBrush;
            borderBlack.BorderBrush = w ? inactiveBrush : activeBrush;
        }

        public void Reset() => Init();
    }
}
