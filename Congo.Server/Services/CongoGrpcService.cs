using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Grpc.Core;
using Congo.Core;

namespace Congo.Server.Services;

public enum Color : int { White, Black }

internal static class CongoState
{
    private static long _id = 1;
    private static readonly string dataPath = "Data" + Path.DirectorySeparatorChar;
    private static readonly string dbPath = dataPath + "main.db";
    private static readonly string connString = string.Format(@"Data Source={0}", dbPath);
    private static readonly ConcurrentDictionary<long, object> lockDb;

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
        => @"SELECT MAX(id) FROM games";

    private static string getInsertFenCommandText()
        => @"INSERT INTO games (fst, lst) VALUES ($fen, $fen)";

    #endregion

    #region CSV

    private static void createCsv(long id)
    {
        var csvFile = dataPath + id + ".csv";
        using var sw = File.CreateText(csvFile);
        sw.WriteLine("fr,to,bt");
    }

    #endregion

    static CongoState()
    {
        lockDb = new();
        if (!Directory.Exists(dataPath)) { Directory.CreateDirectory(dataPath); }

        if (!File.Exists(dbPath)) {
            executeNonQuery(createTableCommandText(), Array.Empty<string>(), Array.Empty<object>());
            createCsv(_id);
        }

        else {
            _id = executeScalarQuery<long>(getMaxIdCommandText(), Array.Empty<string>(), Array.Empty<object>());
        }
    }

    /// <summary>
    /// @note This method does not use transactions, because SQLite buffer is
    /// intended to be used by at most one transaction and other could timeout
    /// in between. Better decide new @b id manually.
    /// </summary>
    public static long Create(string fen)
    {
        var id = Interlocked.Increment(ref _id);

        executeNonQuery(getInsertFenCommandText(), new string[] { "$fen" }, new object[] { fen });
        lockDb[id] = new();
        createCsv(id);

        return id;
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

        long id = -1;
        if (fen is not null) {
            id = CongoState.Create(fen);
            logger.LogInformation("Game {fen} created with id {response}.", fen, id);
        }

        return Task.FromResult(new AssignReply { Id = id });
    }

    public override Task<MoveReply> Move(MoveRequest request, ServerCallContext context)
    {
        string fen = "";
        /*
        if (lockDb.TryGetValue(request.Id, out var l)) {
            lock (l) {
                var game = CongoFen.FromFen(getFenFromDb(request.Id));
                var move = game.ActivePlayer.Accept(new CongoMove(request.Fr, request.To));

                if (move != null) {
                    fen = CongoFen.ToFen(game.Transition(move));

                    // TODO: update "game" database with latest fen

                    // TODO: update "next" database with new move
                }
            }
        }
        */
        return Task.FromResult(new MoveReply { Fen = fen });
    }

    public override Task<GetReply> Get(GetRequest request, ServerCallContext context)
    {
        string fen = "";
        /*
        if (lockDb.TryGetValue(request.Id, out var l)) {
            lock (l) {

            }
        }
        */
        return Task.FromResult(new GetReply { Fen = fen });
    }
}
