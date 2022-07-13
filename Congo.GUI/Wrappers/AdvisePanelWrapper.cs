using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal sealed class AdvisePanelWrapper : IBaseWrapper
    {
        private readonly Button button;
        private readonly TextBlock textBlock;

        private static bool isHiState(MainState state)
            => state == MainState.FR || state == MainState.TO;

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

        public void Activate(MainState oldState, MainState newState)
        {
            // state to -> to only for monkey jumps!
            if (!isHiState(oldState) || !isHiState(newState) || (oldState == MainState.TO && newState == MainState.TO)) {
                textBlock.Text = string.Empty;
            }
            button.IsEnabled = true;
        }

        public void Deactivate()
        {
            button.IsEnabled = false;
            textBlock.Text = string.Empty;
        }
    }
}
