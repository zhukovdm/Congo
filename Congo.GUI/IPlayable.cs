using Congo.Core;
using Congo.Server;
using Grpc.Net.Client;
using System.Collections.Generic;

using CongoClient = Congo.Server.CongoGrpc.CongoGrpcClient;

namespace Congo.GUI
{
    public class NetPack
    {
        public long GameId { get; set; }
        public long MoveId { get; set; }
        public GrpcChannel Channel { get; set; }
        public CongoClient Client { get; set; }

        public NetPack Clone()
        {
            return new()
            {
                GameId = GameId,
                MoveId = MoveId,
                Channel = Channel,
                Client = Client,
            };
        }
    }

    public class PopupPack
    {
        public NetPack NetPack { get; set; }
        public ICollection<DbMove> Moves { get; set; }
    }

    public interface IPlayable
    {
        CongoGame Game { get; set; }
        CongoUser WhiteUser { get; set; }
        CongoUser BlackUser { get; set; }
        PopupPack PopupPack { get; set; }
    }
}
