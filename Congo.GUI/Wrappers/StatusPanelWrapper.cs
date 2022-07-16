using Congo.Core;
using Congo.Server;
using Congo.Utils;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal class StatusPanelWrapper : IBaseWrapper
    {
        private readonly ListBox moves;
        private readonly TextBlock gameId, status, errorMessage;

        public StatusPanelWrapper(TextBlock gameId, ListBox moves, TextBlock status, TextBlock errorMessage)
        {
            this.moves = moves;
            this.gameId = gameId;
            this.status = status;
            this.errorMessage = errorMessage;
        }

        public void Init()
        {
            moves.Items.Clear();
            gameId.Text = string.Empty;
            status.Text = string.Empty;
            errorMessage.Text = string.Empty;
        }

        public void AppendMove(CongoMove move)
        {
            moves.Items.Add(MovePresenter.GetMoveView(move));
            moves.ScrollIntoView(moves.Items[^1]);
        }

        public void AppendMoves(ICollection<DbMove> dbMoves)
        {
            foreach (var move in dbMoves) {
                AppendMove(new CongoMove(move.Fr, move.To));
            }
        }

        public void SetId(long id) => gameId.Text = id.ToString();

        public void SetStatus(string text) => status.Text = text;

        public void SetErrorMessage(string text) => errorMessage.Text = text;
    }
}
