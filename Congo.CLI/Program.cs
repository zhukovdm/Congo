using Congo.Core;
using Congo.Utils;
using Grpc.Core;
using System;

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
                Console.WriteLine($"gRPC exception: StatusCode={ex.StatusCode}.");
                Console.WriteLine("See detailed description at https://grpc.github.io/grpc/csharp/api/Grpc.Core.StatusCode.html.");
            }

            catch (CongoServerResponseException ex) {
                Console.WriteLine("Server response exception: " + ex.Message);
            }

            catch (Exception ex) {
                Console.WriteLine("Unhandled exception: " + ex.Message);
            }
        }
    }
}
