using System;
using Grpc.Core;
using Congo.Core;

namespace Congo.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            CongoGame.Initialize();

            try {
                var ui = CongoCommandLine.Create(CongoArgs.Parser.Parse(args)).Init();
                while (!ui.End()) { ui.Step(); }
                ui.ReportResult();
            }

            catch (ExitException) {
                Console.WriteLine("The program is terminated...");
            }

            catch (ArgumentException ex) {
                Console.WriteLine("Argument exception: " + ex.Message);
            }

            catch (RpcException ex) {
                Console.WriteLine("gRPC exception: " + ex.Status.Detail);
            }

            catch (Exception ex) {
                Console.WriteLine("Unhandled exception: " + ex.Message);
            }
        }
    }
}
