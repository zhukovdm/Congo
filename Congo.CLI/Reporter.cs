using System;
using System.IO;
using Congo.Core;

namespace Congo.CLI
{
    public sealed class TextReporter
    {
        private const string greetFile = "greet";
        private const string whiteView = "white";
        private const string blackView = "black";
        private const string textFileExt = ".txt";
        private static readonly string resourcesFolder = "Resources" + Path.DirectorySeparatorChar;

        private static string readTextFile(string filename)
        {
            try {
                return File.ReadAllText(resourcesFolder + filename + textFileExt);
            }
            catch (Exception) { return null; }
        }

        private readonly TextWriter writer;

        public TextReporter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Greet()
        {
            writer.WriteLine();
            writer.Write(readTextFile(greetFile));
        }

        public void ReportHelpFile(string helpFile)
            => writer.Write(readTextFile(helpFile));

        public void ReportEmptyCommand()
            => writer.WriteLine(" Input command is an empty string. Try again.");

        public void ReportNotSupportedCommand(string command)
            => writer.WriteLine($" Command {command} is not supported. Consult \"help help\".");

        public void ReportWrongCommandFormat(string command)
            => writer.WriteLine($" Wrong command format. Consult \"help {command}\".");

        public void ReportAdvisedMove(CongoMove move)
            => writer.WriteLine($" Advised move is {TextPresenter.GetMoveView(move)}.");

        public void ReportWrongMove()
            => writer.WriteLine(" Entered move is wrong. Consult \"show moves\".");

        public void ReportResult(CongoGame game)
        {
            var winner = game.WhitePlayer.HasLion
                ? whiteView
                : blackView;

            writer.WriteLine();
            writer.WriteLine($" {winner} wins.");
            writer.WriteLine();
        }
    }
}
