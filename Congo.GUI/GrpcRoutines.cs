using Congo.Core;
using Congo.Server;
using System.Collections.Generic;
using System.Linq;

namespace Congo.GUI
{
    internal static class GrpcRoutines
    {
        public static long PostFen(CongoGrpc.CongoGrpcClient client, string fen)
            => client.PostFen(new PostFenRequest() { Fen = fen }).GameId;

        public static long PostMove(CongoGrpc.CongoGrpcClient client, long gameId, CongoMove move)
            => client.PostMove(new PostMoveRequest() { GameId = gameId, Fr = move.Fr, To = move.To }).MoveId;

        public static bool CheckGameId(CongoGrpc.CongoGrpcClient client, long gameId)
            => client.CheckGameId(new CheckGameIdRequest() { GameId = gameId }).Exist;

        private static string getLatestFen(CongoGrpc.CongoGrpcClient client, long gameId)
            => client.GetLatestFen(new GetLatestFenRequest() { GameId = gameId }).Fen;

        public static CongoGame GetLatestGame(CongoGrpc.CongoGrpcClient client, long gameId)
            => CongoFen.FromFen(getLatestFen(client, gameId));

        public static IEnumerable<CongoMove> GetMovesAfter(CongoGrpc.CongoGrpcClient client, long gameId, long moveId)
        {
            return client.GetDbMovesAfter(new GetDbMovesAfterRequest() { GameId = gameId, MoveId = moveId })
                .Moves
                .Select(x => new CongoMove(x.Fr, x.To));
        }
    }
}
