using Congo.Core;
using Congo.Server;
using Grpc.Net.Client;

namespace Congo.GUI
{
    internal record CongoNetworkPack
    {
        public long GameId { get; }
        public long MoveId { get; set; }
        public GrpcChannel Channel { get; }
        public CongoGrpc.CongoGrpcClient Client { get; }
    }

    internal interface IPlayable
    {
        CongoGame Game { get; }
        CongoUser WhiteUser { get; }
        CongoUser BlackUser { get; }
        CongoNetworkPack NetworkPack { get; }
    }
}
