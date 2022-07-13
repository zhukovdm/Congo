using Congo.Core;
using Congo.Server;
using Grpc.Net.Client;

namespace Congo.GUI
{
    public record CongoNetworkPack
    {
        public long GameId { get; set; }
        public long MoveId { get; set; }
        public GrpcChannel Channel { get; set; }
        public CongoGrpc.CongoGrpcClient Client { get; set; }
    }

    public interface IPlayable
    {
        CongoGame Game { get; set; }
        CongoUser WhiteUser { get; set; }
        CongoUser BlackUser { get; set; }
        CongoNetworkPack NetworkPack { get; set; }
    }
}
