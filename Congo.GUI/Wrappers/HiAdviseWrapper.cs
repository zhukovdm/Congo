using Congo.Core;
using Congo.Utils;
using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal sealed class HiAdviseWrapper
    {
        private readonly Button buttonAdvise;
        private readonly TextBlock textBlockAdvise;

        public HiAdviseWrapper(Button buttonAdvise, TextBlock textBlockAdvise)
        {
            this.buttonAdvise = buttonAdvise;
            this.textBlockAdvise = textBlockAdvise;
        }

        public void Begin()
        {
            buttonAdvise.IsEnabled = false;
            textBlockAdvise.Text = string.Empty;
        }

        public void End(CongoMove move)
        {
            buttonAdvise.IsEnabled = true;
            textBlockAdvise.Text = MovePresenter.GetMoveView(move);
        }
    }
}
