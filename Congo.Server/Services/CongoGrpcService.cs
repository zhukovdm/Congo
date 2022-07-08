using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Grpc.Core;
using Congo.Core;

namespace Congo.Server.Services;

internal static class CongoState
{
    private static long id = 0;
    private static readonly string dataPath = "Data" + Path.DirectorySeparatorChar;
    private static readonly string mainDbPath = dataPath + "main.db";
    private static readonly string mainDbDataSource = string.Format(@"Data Source={0}", mainDbPath);

    internal static readonly ConcurrentDictionary<long, object> lockDb;

    #region SQL

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

    #region games commands

    /// <summary>
    /// Create database with @b games table and ensure at least one row.
    /// Avoid edge cases with empty db.
    /// </summary>
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
            );
        ";

    private static string commandTextGetMaxGameId()
        => @"SELECT MAX(gameId) FROM games;";

    private static string commandTextPostFen()
        => @"INSERT INTO games (gameId, firstFen, lastFen) VALUES ($gameId, $fen, $fen);";

    private static string commandTextGetLastFen()
        => @"SELECT lastFen FROM games WHERE gameId = $gameId;";

    private static string commandTextSetLastFen()
        => @"UPDATE games SET lastFen = $lastFen WHERE gameId = $gameId;";

    private static string commandtextGetGameIdentifiers()
        => @"SELECT gameId FROM games ORDER BY gameId ASC;";

    #endregion

    #region moves commands

    private static string commandTextCreateMovesTable()
        =>  @"CREATE TABLE IF NOT EXISTS moves(
                [moveId] INTEGER NOT NULL PRIMARY KEY,
                [fromSquare] INTEGER NOT NULL,
                [toSquare] INTEGER NOT NULL
            );
        ";

    private static string commandTextGetMaxMoveId()
        => @"SELECT MAX(moveId) FROM moves;";

    private static string commandTextAppendMove()
        => @"INSERT INTO moves (moveId, fromSquare, toSquare) VALUES ($moveId, $fromSquare, $toSquare);";

    private static string commandTextGetMovesFrom()
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
        command.CommandText = commandtextGetGameIdentifiers();

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

    /// <summary>
    /// Create new game record in the database for a @b valid Fen.
    /// @note This method does not use transactions, because SQLite buffer is
    /// intended to be used by at most one transaction and other could timeout
    /// in between. Better to decide new @b id manually.
    /// </summary>
    internal static long Create(string fen)
    {
        var newId = Interlocked.Increment(ref id);

        lockDb[newId] = new();
        executeNonQuery(mainDbDataSource, commandTextPostFen(), new string[] { "$gameId", "$fen" }, new object[] { newId, fen });
        executeNonQuery(getGameDbDataSource(newId), commandTextCreateMovesTable(), Array.Empty<string>(), Array.Empty<object>());

        return newId;
    }

    /// <summary>
    /// Get latest Fen assoc. with @b gameId.
    /// @note @b gameId shall exist!
    /// </summary>
    internal static string GetLastFen(long gameId)
        => (string)executeScalarQuery(mainDbDataSource, commandTextGetLastFen(), new string[] { "$gameId" }, new object[] { gameId });

    /// <summary>
    /// Set the latest Fen assoc. with @b gameId.
    /// @note @b gameId shall exist!
    /// </summary>
    internal static void SetLastFen(long gameId, string lst)
        => executeNonQuery(mainDbDataSource, commandTextSetLastFen(), new string[] { "$gameId", "$lastFen" }, new object[] { gameId, lst });

    internal static long AppendMove(long gameId, CongoMove move)
    {
        var source = getGameDbDataSource(gameId);
        var result = executeScalarQuery(source, commandTextGetMaxMoveId(), Array.Empty<string>(), Array.Empty<object>());
        var nextId = (result != DBNull.Value) ? (long)result + 1 : 0;
        executeNonQuery(source, commandTextAppendMove(), new string[] { "$moveId", "$fromSquare", "$toSquare" }, new object[] { nextId, move.Fr, move.To });

        return nextId;
    }

    internal static List<DbMove> GetDbMovesFrom(long gameId, int from)
    {
        var moves = new List<DbMove>();

        using var conn = new SqliteConnection(getGameDbDataSource(gameId));
        conn.Open();
        using var command = conn.CreateCommand();
        command.CommandText = commandTextGetMovesFrom();
        command.Parameters.AddWithValue("$moveId", from);

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

    public CongoGrpcService(ILogger<CongoGrpcService> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Create new record with @b request.Game in the database.
    /// @return Assigned game identifier or -1 on failure.
    /// </summary>
    public override Task<PostBoardReply> PostBoard(PostBoardRequest request, ServerCallContext context)
    {
        string fen = null;

        if (request.Board == "standard") { fen = CongoFen.ToFen(CongoGame.Standard()); }
        if (CongoFen.FromFen(request.Board) is not null) { fen = request.Board; }

        long gameId = -1;

        if (fen is not null) {
            gameId = CongoState.Create(fen);
            logger.LogInformation("Game {fen} created with gameId {response}.", fen, gameId);
        }

        return Task.FromResult(new PostBoardReply { GameId = gameId });
    }

    /// <summary>
    /// Introduce new move into current game.
    /// @return New game as Congo FEN string if move is possible.
    ///     Otherwise empty string.
    /// </summary>
    public override Task<PostMoveReply> PostMove(PostMoveRequest request, ServerCallContext context)
    {
        long moveId = -1;

        if (CongoState.lockDb.TryGetValue(request.GameId, out var l)) {
            lock (l) {
                var game = CongoFen.FromFen(CongoState.GetLastFen(request.GameId));
                var move = game.ActivePlayer.Accept(new CongoMove(request.Fr, request.To));

                if (move != null) {
                    var fen = CongoFen.ToFen(game.Transition(move));
                    CongoState.SetLastFen(request.GameId, fen);
                    moveId = CongoState.AppendMove(request.GameId, move);
                }
            }
        }

        return Task.FromResult(new PostMoveReply { MoveId = moveId });
    }

    /// <summary>
    /// Get last Congo FEN by game id.
    /// </summary>
    public override Task<GetLastFenReply> GetLastFen(GetLastFenRequest request, ServerCallContext context)
    {
        string fen = "";

        if (CongoState.lockDb.TryGetValue(request.GameId, out var l)) {
            lock (l) {
                fen = CongoState.GetLastFen(request.GameId);
            }
        }

        return Task.FromResult(new GetLastFenReply { Fen = fen });
    }

    public override Task<CheckGameIdReply> CheckGameId(CheckGameIdRequest request, ServerCallContext context)
        => Task.FromResult(new CheckGameIdReply { Exist = CongoState.lockDb.TryGetValue(request.GameId, out _) });

    private static Task<GetDbMovesReply> getDbMovesFromImpl(long gameId, int from)
    {
        var reply = new GetDbMovesReply();

        if (CongoState.lockDb.TryGetValue(gameId, out var l)) {
            lock (l) {
                reply.Moves.AddRange(CongoState.GetDbMovesFrom(gameId, from));
            }
        }

        return Task.FromResult(reply);
    }

    public override Task<GetDbMovesReply> GetDbMovesFrom(GetDbMovesFromRequest request, ServerCallContext context)
        => getDbMovesFromImpl(request.GameId, request.From);

    public override Task<GetDbMovesReply> GetDbMovesAll(GetDbMovesAllRequest request, ServerCallContext context)
        => getDbMovesFromImpl(request.GameId, -1);
}
