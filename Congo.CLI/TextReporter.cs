using Congo.Core;
using Congo.Utils;
using System;
using System.IO;

namespace Congo.CLI
{
    public sealed class TextReporter
    {
        private const string whiteView = "white";
        private const string blackView = "black";

        private readonly TextWriter _writer;

        public TextReporter(TextWriter writer)
        {
            _writer = writer;
        }

        public void Greet()
        {
            _writer.WriteLine();
            _writer.Write(GreetView.GetInstance());
        }

        public void ReportHelpView(HelpView view) => _writer.Write(view);

        public void ReportEmptyCommand()
            => _writer.WriteLine(" Input command is an empty string. Try again.");

        public void ReportNotSupportedCommand(string command)
            => _writer.WriteLine($" Command {command} is not supported. Consult \"help help\".");

        public void ReportWrongCommandFormat(string command)
            => _writer.WriteLine($" Wrong command format. Consult \"help {command}\".");

        public void ReportAdvisedMove(CongoMove move)
            => _writer.WriteLine($" Advised move is {MovePresenter.GetMoveView(move)}.");

        public void ReportWrongMove()
            => _writer.WriteLine(" Entered move is wrong. Consult \"show moves\".");

        public void ReportResult(CongoGame game)
        {
            var winner = game.WhitePlayer.HasLion
                ? whiteView
                : blackView;

            _writer.WriteLine();
            _writer.WriteLine($" {winner} wins.");
            _writer.WriteLine();
        }
    }
}
