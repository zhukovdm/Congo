using Congo.Core;
using Grpc.Core;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;

namespace Congo.Server.Services;

internal static class CongoState
{
    private static long id = 0;
    private static readonly string dataPath = "Data" + Path.DirectorySeparatorChar;
    private static readonly string mainDbPath = dataPath + "main.db";
    private static readonly string mainDbDataSource = string.Format(@"Data Source={0}", mainDbPath);

    internal static readonly ConcurrentDictionary<long, object> lockDb;

    #region SQL

    #region execute query

    private static void executeNonQuery(string dataSource, string commandText, string[] pars, object[] objs)
    {
        using var conn = new SqliteConnection(dataSource);
        conn.Open();
        using var command = conn.CreateCommand();
        command.CommandText = commandText;

        for (int i = 0; i < pars.Length; ++i) {
            command.Parameters.AddWithValue(pars[i], objs[i]);
        }

        command.ExecuteNonQuery();
        conn.Close();
    }

    private static object executeScalarQuery(string dataSource, string commandText, string[] pars, object[] objs)
    {
        using var conn = new SqliteConnection(dataSource);
        conn.Open();
        using var command = conn.CreateCommand();
        command.CommandText = commandText;

        for (int i = 0; i < pars.Length; ++i) {
            command.Parameters.AddWithValue(pars[i], objs[i]);
        }

        var result = command.ExecuteScalar();
        conn.Close();

        return result;
    }

    #endregion

    #region games commands

    private static string commandTextCreateGamesTable()
        => @"CREATE TABLE IF NOT EXISTS games(
                [gameId] INTEGER NOT NULL PRIMARY KEY,
                [firstFen] NVARCHAR(100) NOT NULL,
                [lastFen] NVARCHAR(100) NOT NULL
            );
            INSERT INTO games (gameId, firstFen, lastFen) VALUES
            (
                0,
                'gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/w/-1',
                'gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/w/-1'
            );";

    private static string commandTextGetMaxGameId()
        => @"SELECT MAX(gameId) FROM games;";

    private static string commandTextPostFen()
        => @"INSERT INTO games (gameId, firstFen, lastFen) VALUES ($gameId, $fen, $fen);";

    private static string commandTextGetFirstFen()
        => @"SELECT firstFen FROM games WHERE gameId = $gameId;";

    private static string commandTextGetLatestFen()
        => @"SELECT lastFen FROM games WHERE gameId = $gameId;";

    private static string commandTextSetLatestFen()
        => @"UPDATE games SET lastFen = $lastFen WHERE gameId = $gameId;";

    private static string commandTextGetGameIdentifiers()
        => @"SELECT gameId FROM games ORDER BY gameId ASC;";

    #endregion

    #region moves commands

    private static string commandTextCreateMovesTable()
        =>  @"CREATE TABLE IF NOT EXISTS moves(
                [moveId] INTEGER NOT NULL PRIMARY KEY,
                [fromSquare] INTEGER NOT NULL,
                [toSquare] INTEGER NOT NULL
            );";

    private static string commandTextGetMaxMoveId()
        => @"SELECT MAX(moveId) FROM moves;";

    private static string commandTextAppendMove()
        => @"INSERT INTO moves (moveId, fromSquare, toSquare) VALUES ($moveId, $fromSquare, $toSquare);";

    private static string commandTextGetMovesAfter()
        => @"SELECT moveId, fromSquare, toSquare FROM moves WHERE moveId > $moveId ORDER BY moveId ASC;";

    #endregion

    #endregion

    private static string getGameDbDataSource(long gameId)
        => string.Format(@"Data Source={0}", dataPath + gameId + ".db");

    private static void addLocks()
    {
        using var conn = new SqliteConnection(mainDbDataSource);
        conn.Open();
        using var command = conn.CreateCommand();
        command.CommandText = commandTextGetGameIdentifiers();

        var result = command.ExecuteReader();

        while (result.Read()) {
            lockDb[result.GetInt32(0)] = new();
        }
    }

    static CongoState()
    {
        lockDb = new();

        if (!Directory.Exists(dataPath)) {
            Directory.CreateDirectory(dataPath);
        }

        if (!File.Exists(mainDbPath)) {
            executeNonQuery(mainDbDataSource, commandTextCreateGamesTable(), Array.Empty<string>(), Array.Empty<object>());
        }

        if (!File.Exists(dataPath + "0" + ".db")) {
            executeNonQuery(getGameDbDataSource(0), commandTextCreateMovesTable(), Array.Empty<string>(), Array.Empty<object>());
        }

        addLocks();
        id = (long)executeScalarQuery(mainDbDataSource, commandTextGetMaxGameId(), Array.Empty<string>(), Array.Empty<object>());
    }

    internal static long CreateGame(string fen)
    {
        var newId = Interlocked.Increment(ref id);

        lockDb[newId] = new();
        executeNonQuery(mainDbDataSource, commandTextPostFen(), new string[] { "$gameId", "$fen" }, new object[] { newId, fen });
        executeNonQuery(getGameDbDataSource(newId), commandTextCreateMovesTable(), Array.Empty<string>(), Array.Empty<object>());

        return newId;
    }

    internal static string GetFirstFen(long gameId)
        => (string)executeScalarQuery(mainDbDataSource, commandTextGetFirstFen(), new string[] { "$gameId" }, new object[] { gameId });

    internal static string GetLatestFen(long gameId)
        => (string)executeScalarQuery(mainDbDataSource, commandTextGetLatestFen(), new string[] { "$gameId" }, new object[] { gameId });

    internal static void SetLatestFen(long gameId, string lst)
        => executeNonQuery(mainDbDataSource, commandTextSetLatestFen(), new string[] { "$gameId", "$lastFen" }, new object[] { gameId, lst });

    internal static long AppendMove(long gameId, CongoMove move)
    {
        var source = getGameDbDataSource(gameId);
        var result = executeScalarQuery(source, commandTextGetMaxMoveId(), Array.Empty<string>(), Array.Empty<object>());
        var nextId = (result != DBNull.Value) ? (long)result + 1 : 0;
        executeNonQuery(source, commandTextAppendMove(), new string[] { "$moveId", "$fromSquare", "$toSquare" }, new object[] { nextId, move.Fr, move.To });

        return nextId;
    }

    internal static List<DbMove> GetDbMovesAfter(long gameId, long moveId)
    {
        var moves = new List<DbMove>();

        using var conn = new SqliteConnection(getGameDbDataSource(gameId));
        conn.Open();
        using var command = conn.CreateCommand();
        command.CommandText = commandTextGetMovesAfter();
        command.Parameters.AddWithValue("$moveId", moveId);

        var result = command.ExecuteReader();

        while (result.Read()) {
            var fr = result.GetInt32(1);
            var to = result.GetInt32(2);
            moves.Add(new DbMove() { Fr = result.GetInt32(1), To = result.GetInt32(2) });
        }

        return moves;
    }
}

public class CongoGrpcService : CongoGrpc.CongoGrpcBase
{
    private readonly ILogger<CongoGrpcService> logger;

    private static Task<GetDbMovesAfterReply> getDbMovesAfter(long gameId, long moveId)
    {
        var reply = new GetDbMovesAfterReply();

        if (CongoState.lockDb.TryGetValue(gameId, out var l)) {
            lock (l) {
                reply.Moves.AddRange(CongoState.GetDbMovesAfter(gameId, moveId));
            }
        }

        return Task.FromResult(reply);
    }

    public CongoGrpcService(ILogger<CongoGrpcService> logger)
    {
        this.logger = logger;
    }

    public override Task<PostFenReply> PostFen(PostFenRequest request, ServerCallContext context)
    {
        string fen = null;

        if (request.Fen == "standard") { fen = CongoFen.ToFen(CongoGame.Standard()); }
        if (CongoFen.FromFen(request.Fen) is not null) { fen = request.Fen; }

        long gameId = -1;

        if (fen is not null) {
            gameId = CongoState.CreateGame(fen);
            logger.LogInformation("Game {fen} created with gameId {response}.", fen, gameId);
        }

        return Task.FromResult(new PostFenReply { GameId = gameId });
    }

    public override Task<PostMoveReply> PostMove(PostMoveRequest request, ServerCallContext context)
    {
        long moveId = -1;

        if (CongoState.lockDb.TryGetValue(request.GameId, out var l)) {
            lock (l) {
                var game = CongoFen.FromFen(CongoState.GetLatestFen(request.GameId));
                var move = game.ActivePlayer.Accept(new CongoMove(request.Fr, request.To));

                if (move != null) {
                    var fen = CongoFen.ToFen(game.Transition(move));
                    CongoState.SetLatestFen(request.GameId, fen);
                    moveId = CongoState.AppendMove(request.GameId, move);
                }
            }
        }

        return Task.FromResult(new PostMoveReply { MoveId = moveId });
    }

    public override Task<GetFirstFenReply> GetFirstFen(GetFirstFenRequest request, ServerCallContext context)
    {
        var fen = string.Empty;

        if (CongoState.lockDb.TryGetValue(request.GameId, out var l)) {
            lock (l) {
                fen = CongoState.GetFirstFen(request.GameId);
            }
        }

        return Task.FromResult(new GetFirstFenReply { Fen = fen });
    }

    public override Task<GetLatestFenReply> GetLatestFen(GetLatestFenRequest request, ServerCallContext context)
    {
        var fen = string.Empty;

        if (CongoState.lockDb.TryGetValue(request.GameId, out var l)) {
            lock (l) {
                fen = CongoState.GetLatestFen(request.GameId);
            }
        }

        return Task.FromResult(new GetLatestFenReply { Fen = fen });
    }

    public override Task<CheckGameIdReply> CheckGameId(CheckGameIdRequest request, ServerCallContext context)
        => Task.FromResult(new CheckGameIdReply { Exist = CongoState.lockDb.TryGetValue(request.GameId, out _) });

    public override Task<GetDbMovesAfterReply> GetDbMovesAfter(GetDbMovesAfterRequest request, ServerCallContext context)
        => getDbMovesAfter(request.GameId, request.MoveId);
}
