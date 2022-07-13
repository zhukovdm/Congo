using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal sealed class AdvisePanelWrapper : IBaseWrapper
    {
        private readonly Button button;
        private readonly TextBlock textBlock;

        public AdvisePanelWrapper(Button button, TextBlock textBlock)
        {
            this.button = button;
            this.textBlock = textBlock;
        }

        public void Init()
        {
            button.IsEnabled = false;
            textBlock.Text = string.Empty;
        }
    }
}
