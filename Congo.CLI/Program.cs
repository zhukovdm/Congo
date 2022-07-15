using Congo.Core;
using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Congo.CLI
{
    class Program
    {
        private static void reportException(List<string> messages)
        {
            foreach (var message in messages) {
                Console.WriteLine();
                Console.WriteLine(message);
            }
        }

        static void Main(string[] args)
        {
            CongoGame.Initialize();

            try {
                var ui = CongoCommandLine.Create(CongoArgs.Parser.Parse(args)).Init();
                while (!ui.End()) { ui.Step(); }
                ui.ReportResult();
            }

            catch (ExitException) {
                reportException(new() { "The program is terminated..." });
            }

            catch (ArgumentException ex) {
                reportException(new() { "Argument exception: " + ex.Message });
            }

            catch (RpcException ex) {
                reportException(new()
                {
                    $"gRPC exception: StatusCode={ex.StatusCode}.",
                    ex.Status.Detail
                });
            }

            catch (Exception ex) {
                reportException(new() { "Unhandled exception: " + ex.Message });
            }
        }
    }
}
