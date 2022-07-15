using Congo.Core;
using Congo.Server;
using Congo.Utils;
using System.Collections.Generic;
using System.Linq;

using CongoClient = Congo.Server.CongoGrpc.CongoGrpcClient;

namespace Congo.CLI
{
    internal static class GrpcRoutines
    {
        private static string getLatestFen(CongoClient client, long gameId)
        {
            var fen = client.GetLatestFen(new GetLatestFenRequest() { GameId = gameId }).Fen;

            if (fen == string.Empty) {
                throw new CongoServerResponseException($"Latest fen for gameId {gameId} cannot be obtained.");
            }

            return fen;
        }

        public static long PostFen(CongoClient client, string fen)
        {
            var gameId = client.PostFen(new PostFenRequest() { Fen = fen }).GameId;

            if (gameId < 0) {
                throw new CongoServerResponseException($"New game with {fen} cannot be created.");
            }

            return gameId;
        }

        public static long PostMove(CongoClient client, long gameId, long oldId, CongoMove move)
        {
            var newId = client.PostMove(new PostMoveRequest() { GameId = gameId, Fr = move.Fr, To = move.To }).MoveId;

            if (newId < 0) {
                throw new CongoServerResponseException($"Move {MovePresenter.GetMoveView(move)} is invalid.");
            }

            if (newId != oldId + 1) {
                throw new CongoServerResponseException($"Move is submitted, but game state is not consistent.");
            }

            return newId;
        }

        public static bool CheckGameId(CongoClient client, long gameId)
            => client.CheckGameId(new CheckGameIdRequest() { GameId = gameId }).Exist;

        public static string GetFirstFen(CongoClient client, long gameId)
        {
            var fen = client.GetFirstFen(new GetFirstFenRequest() { GameId = gameId }).Fen;

            if (fen == string.Empty) {
                throw new CongoServerResponseException($"First fen for gameId {gameId} cannot be obtained.");
            }

            return fen;
        }

        public static CongoGame GetLatestGame(CongoClient client, long gameId)
            => CongoFen.FromFen(getLatestFen(client, gameId));

        public static IEnumerable<CongoMove> GetMovesAfter(CongoClient client, long gameId, long moveId)
        {
            return from move in client.GetDbMovesAfter(new GetDbMovesAfterRequest() { GameId = gameId, MoveId = moveId }).Moves
                   select new CongoMove(move.Fr, move.To);
        }
    }
}
