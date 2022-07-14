using Congo.Server;

namespace Congo.GUI
{
    internal static class GrpcRoutines
    {
        public static long PostFen(CongoGrpc.CongoGrpcClient client, string fen)
            => client.PostFen(new PostFenRequest() { Fen = fen }).GameId;

        public static bool CheckGameId(CongoGrpc.CongoGrpcClient client, long gameId)
            => client.CheckGameId(new CheckGameIdRequest() { GameId = gameId }).Exist;

        public static string GetFirstFen(CongoGrpc.CongoGrpcClient client, long gameId)
            => client.GetFirstFen(new GetFirstFenRequest() { GameId = gameId }).Fen;
    }
}
