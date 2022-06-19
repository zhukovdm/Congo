using System;
using Congo.Core;

namespace Congo.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            CongoGame.Initialize();

            try {
                var ui = CongoCommandLine.Create(CongoArgs.Parser.Parse(args));
                while (!ui.End()) { ui.Step(); }
                ui.ReportResult();
            }

            catch (ArgumentException ex) {
                Console.WriteLine(ex.Message);
            }

            catch (Exception ex) {
                Console.WriteLine("Unhandled exception: " + ex.Message);
            }
        }
    }
}
