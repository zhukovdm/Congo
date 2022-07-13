using Congo.Core;
using Congo.Utils;
using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal sealed class AiAdviseWrapper
    {
        private readonly MenuItem menuItemCancel;

        public AiAdviseWrapper(MenuItem menuItemCancel)
        {
            this.menuItemCancel = menuItemCancel;
        }

        public void Begin()
        {
            menuItemCancel.IsEnabled = true;
        }

        public void End()
        {
            menuItemCancel.IsEnabled = false;
        }
    }

    internal sealed class HiAdviseWrapper
    {
        private readonly Button buttonAdvise;
        private readonly MenuItem menuItemCancel;
        private readonly TextBlock textBlockAdvise;

        public HiAdviseWrapper(MenuItem menuItemCancel, Button buttonAdvise, TextBlock textBlockAdvise)
        {
            this.buttonAdvise = buttonAdvise;
            this.menuItemCancel = menuItemCancel;
            this.textBlockAdvise = textBlockAdvise;
        }

        public void Begin()
        {
            buttonAdvise.IsEnabled = false;
            menuItemCancel.IsEnabled = true;
            textBlockAdvise.Text = string.Empty;
        }

        public void End(CongoMove move)
        {
            buttonAdvise.IsEnabled = true;
            menuItemCancel.IsEnabled = false;
            textBlockAdvise.Text = MovePresenter.GetMoveView(move);
        }
    }
}
