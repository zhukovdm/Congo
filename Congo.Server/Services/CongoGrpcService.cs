using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Grpc.Core;
using Congo.Core;

namespace Congo.Server.Services;

internal static class CongoState
{
    private static long id = 1;
    private static readonly string dataPath = "Data" + Path.DirectorySeparatorChar;
    private static readonly string dbPath = dataPath + "main.db";
    private static readonly string connString = string.Format(@"Data Source={0}", dbPath);

    public static readonly ConcurrentDictionary<long, object> lockDb;

    #region SQL

    private static void executeNonQuery(string commandText, string[] pars, object[] objs)
    {
        using var conn = new SqliteConnection(connString);
        conn.Open();
        using var command = conn.CreateCommand();
        command.CommandText = commandText;

        for (int i = 0; i < pars.Length; ++i) {
            command.Parameters.AddWithValue(pars[i], objs[i]);
        }

        command.ExecuteNonQuery();
        conn.Close();
    }

    private static T executeScalarQuery<T>(string commandText, string[] pars, object[] objs)
    {
        using var conn = new SqliteConnection(connString);
        conn.Open();
        using var command = conn.CreateCommand();
        command.CommandText = commandText;

        for (int i = 0; i < pars.Length; ++i) {
            command.Parameters.AddWithValue(pars[i], objs[i]);
        }

        var result = command.ExecuteScalar();
        conn.Close();

        return (T)result;
    }

    /// <summary>
    /// Create database and ensure at least one row. Avoid edge cases with empty db.
    /// </summary>
    private static string createTableCommandText()
        => @"
            CREATE TABLE IF NOT EXISTS games(
                [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                [fst] NVARCHAR(100) NOT NULL,
                [lst] NVARCHAR(100) NOT NULL
            );
            INSERT INTO games (fst, lst) VALUES (
                'gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/w/-1',
                'gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/w/-1'
            );
        ";

    private static string getMaxIdCommandText()
        => @"SELECT MAX(id) FROM games;";

    private static string getInsertFenCommandText()
        => @"INSERT INTO games (fst, lst) VALUES ($fen, $fen);";

    private static string getLstFenCommandText()
        => @"SELECT lst FROM games WHERE id = $id;";

    private static string setLstFenCommandText()
        => @"UPDATE games SET lst = $lst WHERE id = $id;";

    #endregion

    #region CSV, CRLF-terminated rows comply with rfc7111

    private static string getCsvFilePath(long id)
        => dataPath + id + ".csv";

    private static void createCsv(long id)
    {
        using var sw = File.CreateText(getCsvFilePath(id));
        sw.Write("fr,bt,to\r\n");
    }

    internal static void AppendCsv(long id, string row)
    {
        using var sw = File.AppendText(getCsvFilePath(id));
        sw.Write(row + "\r\n");
    }

    #endregion

    static CongoState()
    {
        lockDb = new();
        if (!Directory.Exists(dataPath)) { Directory.CreateDirectory(dataPath); }

        if (!File.Exists(dbPath)) {
            executeNonQuery(createTableCommandText(), Array.Empty<string>(), Array.Empty<object>());
            createCsv(id);
        }

        else {
            id = executeScalarQuery<long>(getMaxIdCommandText(), Array.Empty<string>(), Array.Empty<object>());
        }
    }

    /// <summary>
    /// @note This method does not use transactions, because SQLite buffer is
    /// intended to be used by at most one transaction and other could timeout
    /// in between. Better decide new @b id manually.
    /// </summary>
    internal static long Create(string fen)
    {
        var newId = Interlocked.Increment(ref id);

        executeNonQuery(getInsertFenCommandText(), new string[] { "$fen" }, new object[] { fen });
        lockDb[newId] = new();
        createCsv(newId);

        return newId;
    }

    internal static string GetLstFen(long gameId)
    {
        return executeScalarQuery<string>(getLstFenCommandText(), new string[] { "$id" }, new object[] { gameId });
    }

    internal static void SetLstFen(string lst, long gameId)
    {
        executeNonQuery(setLstFenCommandText(), new string[] { "$lst", "$id" }, new object[] { lst, gameId });
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
    public override Task<AssignReply> Assign(AssignRequest request, ServerCallContext context)
    {
        string fen = null;

        if (request.Game == "standard") { fen = CongoFen.ToFen(CongoGame.Standard()); }

        var game = CongoFen.FromFen(request.Game);
        if (game is not null) { fen = request.Game; }

        var id = -1L;

        if (fen is not null) {
            id = CongoState.Create(fen);
            logger.LogInformation("Game {fen} created with id {response}.", fen, id);
        }

        return Task.FromResult(new AssignReply { Id = id });
    }

    /// <summary>
    /// Introduce new move into current game.
    /// @return New game as Congo FEN string if move is possible.
    ///     Otherwise empty string.
    /// </summary>
    public override Task<MoveReply> Move(MoveRequest request, ServerCallContext context)
    {
        string fen = "";

        if (CongoState.lockDb.TryGetValue(request.Id, out var l)) {
            lock (l) {
                var game = CongoFen.FromFen(CongoState.GetLstFen(request.Id));
                var move = game.ActivePlayer.Accept(new CongoMove(request.Fr, request.To));

                if (move != null) {
                    fen = CongoFen.ToFen(game.Transition(move));
                    CongoState.SetLstFen(fen, request.Id);

                    var bt = "";
                    if (move is MonkeyJump jump) { bt = jump.Bt.ToString(); }

                    CongoState.AppendCsv(request.Id, string.Join(',', new object[] { request.Fr, bt, request.To }));
                }
            }
        }

        return Task.FromResult(new MoveReply { Fen = fen });
    }

    /// <summary>
    /// Get last Congo FEN by game id.
    /// </summary>
    public override Task<GetReply> Get(GetRequest request, ServerCallContext context)
    {
        string fen = "";

        if (CongoState.lockDb.TryGetValue(request.Id, out var l)) {
            lock (l) {
                fen = CongoState.GetLstFen(request.Id);
            }
        }

        return Task.FromResult(new GetReply { Fen = fen });
    }
}
