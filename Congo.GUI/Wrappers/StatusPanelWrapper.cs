using Congo.Core;
using Congo.Utils;
using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal class StatusPanelWrapper : IPanelWrapper
    {
        private readonly ListBox moves;
        private readonly TextBlock gameId, status;

        public StatusPanelWrapper(TextBlock gameId, ListBox moves, TextBlock status)
        {
            this.moves = moves;
            this.gameId = gameId;
            this.status = status;
        }

        public void Init()
        {
            gameId.Text = string.Empty;
            status.Text = string.Empty;
        }

        public void Reset() { }

        public void AppendMove(CongoMove move)
        {
            moves.Items.Add(MovePresenter.GetMoveView(move));
            moves.ScrollIntoView(moves.Items[^1]);
        }
    }
}
