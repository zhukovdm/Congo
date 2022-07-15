using CongoClient = Congo.Server.CongoGrpc.CongoGrpcClient;

namespace Congo.CLI
{
    internal static class GrpcRoutinesCli
    {
        public static long SyncMoves(CongoClient client, long gameId, long moveId, TextPresenter presenter)
        {
            var moves = GrpcRoutines.GetDbMovesAfter(client, gameId, moveId);
            presenter.ShowNetworkTransitions(moves);
            return moveId + moves.Count;
        }
    }
}
