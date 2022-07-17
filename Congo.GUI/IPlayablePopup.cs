using Congo.Core;
using Congo.Server;
using Grpc.Net.Client;
using System.Collections.Generic;

using CongoClient = Congo.Server.CongoGrpc.CongoGrpcClient;

namespace Congo.GUI
{
    public class NetPack
    {
        public long GameId { get; }
        public long MoveId { get; }
        public GrpcChannel Channel { get; }
        public CongoClient Client { get; }

        public NetPack(long gameId, long moveId, GrpcChannel channel, CongoClient client)
        {
            GameId = gameId;
            MoveId = moveId;
            Channel = channel;
            Client = client;
        }

        public NetPack WithMoveId(long moveId) => new(GameId, moveId, Channel, Client);
    }

    public class PopupPack
    {
        public NetPack NetPack { get; }
        public ICollection<DbMove> DbMoves { get; }
        public PopupPack(NetPack netPack, ICollection<DbMove> moves)
        {
            NetPack = netPack;
            DbMoves = moves;
        }
    }

    public interface IPlayablePopup
    {
        CongoGame Game { get; }
        CongoUser WhiteUser { get; }
        CongoUser BlackUser { get; }
        PopupPack PopupPack { get; }
    }
}
