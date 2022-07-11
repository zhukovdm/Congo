using Congo.Core;
using Congo.Server;
using Grpc.Net.Client;

namespace Congo.GUI
{
    internal interface IPlayable
    {
        CongoGame Game { get; }
        CongoUser WhiteUser { get; }
        CongoUser BlackUser { get; }
        long GameId { get; }
        GrpcChannel Channel { get; }
        CongoGrpc.CongoGrpcClient Client { get; }
    }
}
