using System.Net.Http;
using Grpc.Net.Client;

namespace Congo.Utils
{
    public static class GrpcPrimitives
    {
        public static GrpcChannel CreateRpcChannel(string host, string port)
        {
            // currently, ssl certificate is not supported!
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            return GrpcChannel.ForAddress("https://" + host + ":" + port, new GrpcChannelOptions { HttpHandler = httpHandler });
        }
    }
}
