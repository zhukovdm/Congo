using Grpc.Net.Client;
using System;
using System.Net.Http;

namespace Congo.Utils
{
    public static class GrpcPrimitives
    {
        public static GrpcChannel CreateGrpcChannel(string host, string port)
        {
            // currently, ssl certificate is not supported!
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            return GrpcChannel.ForAddress("https://" + host + ":" + port, new GrpcChannelOptions { HttpHandler = httpHandler });
        }
    }

    public class CongoServerResponseException : Exception
    {
        public CongoServerResponseException(string message) : base(message) { }
    }
}
