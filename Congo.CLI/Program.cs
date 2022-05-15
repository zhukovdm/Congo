using Congo.Core;
using System;

namespace Congo.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            CongoGame.Initialize();

            CongoCommandLine ui = null;

            try {
                ui = CongoCommandLine.Create(CongoArgs.CongoArgsParser.Parse(args));
                while (!ui.End()) { ui.Step(); }
                ui.ReportResult();
            }

            catch (ArgumentException ex) {
                Console.WriteLine(ex.Message);
            }

            catch (Exception ex) {
                Console.WriteLine("Unhandled exception: " + ex.Message);
            }

            finally {
                ui?.Dispose();
            }
        }
    }
}
