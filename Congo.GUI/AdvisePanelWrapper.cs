using System.Windows.Controls;

namespace Congo.GUI
{
    internal sealed class AdvisePanelWrapper : BaseWrapper
    {
        private readonly Button button;
        private readonly TextBlock textBlock;
        public AdvisePanelWrapper(Button button, TextBlock textBlock)
        {
            this.button = button;
            this.textBlock = textBlock;
        }

        public override void Init()
        {
            button.IsEnabled = false;
            textBlock.Text = string.Empty;
        }

        public override void Reset() => Init();
    }
}
