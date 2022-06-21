using Congo.Server.Services;

namespace Congo.WebApplication1
{
    public class Program
    {
        private static void Initialize()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CongoState).TypeHandle);
        }

        public static void Main(string[] args)
        {
            Initialize();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc();
            var app = builder.Build();
            app.MapGrpcService<CongoGrpcService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}
