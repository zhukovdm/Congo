using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using Congo.Server;
using Congo.Core;

namespace Congo.CLI
{
    /// <summary>
    /// The class implements terminal-based user interface for both local
    /// and network games.
    /// </summary>
    public abstract class CongoCommandLine
    {
        private static readonly string resourcesFolder = "Resources\\";
        private static readonly string textFileExt = ".txt";
        private static readonly TextReader reader = Console.In;
        private static readonly TextWriter writer = Console.Out;

        private delegate string[] VerifyCommandDelegate(string[] input);

        private static readonly ImmutableDictionary<string, VerifyCommandDelegate> supportedCommands =
            new Dictionary<string, VerifyCommandDelegate>() {
                { "advise", verifyAdviseCommand },
                { "allow",  verifyAllowCommand  },
                { "exit",   verifyExitCommand   },
                { "help",   verifyHelpCommand   },
                { "move",   verifyMoveCommand   },
                { "show",   verifyShowCommand   }
            }.ToImmutableDictionary();

        private static readonly ImmutableList<string> squareViews =
            new List<string> {
                "a7", "b7", "c7", "d7", "e7", "f7", "g7",
                "a6", "b6", "c6", "d6", "e6", "f6", "g6",
                "a5", "b5", "c5", "d5", "e5", "f5", "g5",
                "a4", "b4", "c4", "d4", "e4", "f4", "g4",
                "a3", "b3", "c3", "d3", "e3", "f3", "g3",
                "a2", "b2", "c2", "d2", "e2", "f2", "g2",
                "a1", "b1", "c1", "d1", "e1", "f1", "g1"
            }.ToImmutableList();

        private static readonly ImmutableList<Type> pieceTypes = new Type[] {
            typeof(Ground), typeof(River), typeof(Elephant), typeof(Zebra),
            typeof(Giraffe), typeof(Crocodile), typeof(Pawn), typeof(Superpawn),
            typeof(Lion), typeof(Monkey)
        }.ToImmutableList();

        private static readonly ImmutableDictionary<Type, string> pieceViews =
            new Dictionary<Type, string>() {
                { typeof(Ground),   "-" }, { typeof(River),     "+" },
                { typeof(Elephant), "e" }, { typeof(Zebra),     "z" },
                { typeof(Giraffe),  "g" }, { typeof(Crocodile), "c" },
                { typeof(Pawn),     "p" }, { typeof(Superpawn), "s" },
                { typeof(Lion),     "l" }, { typeof(Monkey),    "m" }
            }.ToImmutableDictionary();

        private delegate void ShowDelegate(CongoGame game);

        private static readonly ImmutableDictionary<string, ShowDelegate> supportedShows =
            new Dictionary<string, ShowDelegate> {
                { "board", showBoard }, { "players", showPlayers },
                { "moves", showMoves }, { "game",    showGame    }
            }.ToImmutableDictionary();

        private static string GetMoveView(CongoMove move)
            => "(" + squareViews[move.Fr] + "," + squareViews[move.To] + ")";

        private static string ReadTextFile(string filename)
        {
            try {
                return File.ReadAllText(resourcesFolder + filename + textFileExt);
            } catch (Exception) { return null; }
        }

        protected static void ReportAllowedCommands(List<string> allowedCommands)
            => writer.WriteLine($" Allowed commands are {string.Join(", ", allowedCommands.ToArray())}.");

        private static void ReportHelpFile(string helpFile)
            => writer.Write(helpFile);

        private static void ReportEmptyCommand()
            => writer.WriteLine(" Input command is an empty string. Try again.");

        private static void ReportNotSupportedCommand(string command)
            => writer.WriteLine($" Command {command} is not supported. Consult \"help help\".");

        private static void ReportWrongCommandFormat(string command)
            => writer.WriteLine($" Wrong command format. Consult \"help {command}\".");

        private static void ReportNotAllowedCommand(string command)
            => writer.WriteLine($" Command {command} is not allowed. Consult \"allow\".");

        private static void ReportAdvisedMove(CongoMove move)
            => writer.WriteLine($" Advised move is { GetMoveView(move) }.");

        private static void ReportWrongMove()
        {
            writer.WriteLine(" Entered move is wrong. Consult \"show moves\".");
        }

        /// <summary>
        /// Counts pieces of all kinds by a color.
        /// </summary>
        private static int[] countPieces(CongoBoard board, CongoColor color)
        {
            var counter = new int[pieceTypes.Count];
            var enumerator = board.GetEnumerator(color);

            while (enumerator.MoveNext()) {
                var type = board.GetPiece(enumerator.Current).GetType();
                ++counter[pieceTypes.IndexOf(type)];
            }

            return counter;
        }

        private static void showTransition(CongoGame game)
        {
            writer.WriteLine();
            writer.WriteLine($" transition { GetMoveView(game.TransitionMove) }");
        }

        protected static void showNetworkGameId(long gameId)
        {
            writer.WriteLine();
            writer.WriteLine($" Network gameId {gameId}");
        }

        protected static void showBoard(CongoGame game)
        {
            writer.WriteLine();
            var upperBound = CongoBoard.Size * CongoBoard.Size;

            for (int square = 0; square < upperBound; square++) {
                if (square % CongoBoard.Size == 0) {
                    writer.Write($" {CongoBoard.Size - square / CongoBoard.Size}  ");
                }

                var pv = pieceViews[game.Board.GetPiece(square).GetType()];
                if (game.Board.IsFirstMovePiece(square)) pv = pv.ToUpper();
                writer.Write(" " + pv);
                if (square % CongoBoard.Size == CongoBoard.Size - 1) writer.WriteLine();
            }

            writer.WriteLine();
            writer.Write(" /  ");
            for (int i = 0; i < CongoBoard.Size; i++) {
                writer.Write(" " + ((char)('a' + i)).ToString());
            }
            writer.WriteLine();
        }

        private static void showPlayer(CongoBoard board, CongoColor color, CongoPlayer activePlayer)
        {
            var activeRepr = color == activePlayer.Color ? "*" : " ";
            var colorRepr  = color.IsWhite() ? "white" : "black";
            var counter    = countPieces(board, color);

            writer.Write($" {activeRepr} {colorRepr}");
            for (int i = 2; i < pieceTypes.Count; i++) {
                var pieceRepr = color.IsWhite()
                    ? pieceViews[pieceTypes[i]].ToUpper()
                    : pieceViews[pieceTypes[i]].ToLower();
                writer.Write($" {counter[i]}{pieceRepr}");
            }
            writer.WriteLine();
        }

        protected static void showPlayers(CongoGame game)
        {
            writer.WriteLine();
            showPlayer(game.Board, White.Color, game.ActivePlayer);
            showPlayer(game.Board, Black.Color, game.ActivePlayer);
        }

        private static void showMoves(CongoGame game)
        {
            writer.WriteLine();
            int cnt = 0;
            foreach (var move in game.ActivePlayer.Moves) {
                var repr = " " + GetMoveView(move);
                if (cnt + repr.Length > 40) {
                    cnt = 0;
                    writer.WriteLine();
                }
                cnt += repr.Length;
                writer.Write(repr);
            }
            writer.WriteLine();
        }

        private static void showGame(CongoGame game)
        {
            showBoard(game);
            showPlayers(game);
            showMoves(game);
        }

        /// <summary>
        /// Predicate applied on the input shall detect a malformed command.
        /// </summary>
        private static string[] verifyCommand(Func<string[], bool> predicate, string[] input)
        {
            if (predicate(input)) {
                ReportWrongCommandFormat(input[0]);
                input = null;
            }
            
            return input;
        }

        private static string[] verifyAdviseCommand(string[] input)
        {
            static bool predicate(string[] arr) => arr.Length != 1;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyAllowCommand(string[] input)
        {
            static bool predicate(string[] arr) => arr.Length != 1;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyExitCommand(string[] input)
        {
            static bool predicate(string[] arr) => arr.Length != 1;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyHelpCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 2 || !supportedCommands.ContainsKey(arr[1]);
            return verifyCommand(predicate, input);
        }

        private static string[] verifyMoveCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 3 || squareViews.IndexOf(arr[1]) < 0 || squareViews.IndexOf(arr[1]) < 0;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyShowCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 2 || !supportedShows.ContainsKey(arr[1]);
            return verifyCommand(predicate, input);
        }

        protected static void greet()
        {
            writer.WriteLine();
            writer.Write(ReadTextFile("greet"));
        }

        /// <summary>
        /// Try get user command until input passes any of the Verify function.
        /// </summary>
        private static string[] GetUserCommand(List<string> allowedCommands)
        {
            string[] input;
            string[] command = null;

            do {
                writer.WriteLine();
                writer.Write(" > ");
                input = reader.ReadLine().Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (input.Length > 0 && supportedCommands.ContainsKey(input[0])) {
                    if (allowedCommands.IndexOf(input[0]) >= 0) {
                        command = supportedCommands[input[0]].Invoke(input);
                    }

                    else {
                        ReportNotAllowedCommand(input[0]);
                        command = null;
                    }
                }

                else if (input.Length == 0) { ReportEmptyCommand(); }

                else { ReportNotSupportedCommand(input[0]); }

            } while (command == null);

            return command;
        }

        /// <summary>
        /// Creates local CongoGame out of the provided arguments.
        /// </summary>
        private static CongoCommandLine CreateLocal(CongoArgs args)
        {
            var game = args.IsBoardStandard()
                ? CongoGame.Standard()
                : CongoFen.FromFen(args.GetBoardArg());
            return new LocalCommandLine(game, args);
        }

        /// <summary>
        /// Creates network CongoGame out of the provided arguments.
        /// </summary>
        private static CongoCommandLine CreateNetwork(CongoArgs args)
        {
            // currently, ssl certificate is not supported!
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(
                "https://" + args.GetHost() + ":" + args.GetPort(),
                new GrpcChannelOptions { HttpHandler = httpHandler });

            var client = new CongoGrpc.CongoGrpcClient(channel);

            long gameId = -1;
            var board = args.GetBoardArg();

            // assign identifier to the unknown game
            if (args.IsBoardStandard() || args.IsBoardValidCongoFen()) {
                gameId = client.PostBoard(new PostBoardRequest() { Board = board }).GameId;
            }

            if (args.IsBoardValidNetId()) {
                gameId = long.Parse(board);
                
                if (!client.CheckGameId(new CheckGameIdRequest() { GameId = gameId }).Exist) {
                    throw new RpcException(new Status(StatusCode.NotFound, "GameId does not exist."));
                }
            }

            return new NetworkCommandLine(channel, client, args, gameId);
        }

        /// <summary>
        /// Decides local or network mode. Argument structure shall be ensured
        /// by the ArgumentParser.
        /// </summary>
        public static CongoCommandLine Create(CongoArgs args)
            => args.IsGameLocal() ? CreateLocal(args) : CreateNetwork(args);

        protected CongoGame game;
        protected CongoArgs args;
        protected CongoUser whiteUser, blackUser;

        protected CongoUser activeUser
        {
            get => game.ActivePlayer.IsWhite() ? whiteUser : blackUser;
        }

        protected CongoUser createLocalUser(CongoColor color)
        {
            var algo = args.GetAdvisingDelegate(color);
            return args.IsPlayerAi(color) ? new Ai(algo) : new Hi(algo);
        }

        protected CongoUser createMaybeNetworkUser(CongoColor color)
        {
            return (args.GetLocalPlayerColor() == color)
                ? createLocalUser(color)
                : new Net(args.GetRandomAdvisingDelegate());
        }

        /// <summary>
        /// Correctness of the generated move is ensured by the algorithm.
        /// </summary>
        private CongoMove getAiValidMove()
            => activeUser.Advise(game);

        /// <summary>
        /// Generates valid move, all entered invalid moves are ignored.
        /// </summary>
        private CongoMove GetHiValidMove()
        {
            CongoMove move = null;
            var allowedCommands = supportedCommands.Select(pair => pair.Key).ToList();

            do {
                var command = GetUserCommand(allowedCommands);

                switch (command[0]) {

                    case "advise":
                        move = activeUser.Advise(game);
                        ReportAdvisedMove(move);
                        move = null;
                        break;

                    case "allow":
                        ReportAllowedCommands(allowedCommands);
                        break;

                    case "exit":
                        throw new ArgumentException("The program is terminated...");

                    case "help":
                        ReportHelpFile(ReadTextFile(command[1]));
                        break;

                    case "move":
                        move = new CongoMove(squareViews.IndexOf(command[1]),
                                             squareViews.IndexOf(command[2]));
                        break;

                    case "show":
                        supportedShows[command[1]].Invoke(game);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                if (move != null) {
                    move = game.ActivePlayer.Accept(move);
                    if (move == null) { ReportWrongMove(); }
                }

            } while (move == null);

            return move;
        }

        /// <summary>
        /// Generates valid move based on the player kind (either Ai or Hi).
        /// </summary>
        protected CongoMove getValidMove()
            => (activeUser is Ai) ? getAiValidMove() : GetHiValidMove();

        protected void ShowStep()
        {
            showTransition(game);
            showBoard(game);
        }

        public void ReportResult()
        {
            writer.WriteLine();
            var winner = game.WhitePlayer.HasLion
                ? "white"
                : "black";
            writer.WriteLine($" {winner} wins.");
            writer.WriteLine();
        }

        public bool End() => game.HasEnded();

        /// <summary>
        /// Obtains next move from the user and generate view of the current
        /// position.
        /// </summary>
        public abstract void Step();
    }

    public class LocalCommandLine : CongoCommandLine
    {
        public LocalCommandLine(CongoGame game, CongoArgs args)
        {
            this.game = game;
            this.args = args;

            whiteUser = createLocalUser(White.Color);
            blackUser = createLocalUser(Black.Color);

            greet();
            showBoard(game);
            showPlayers(game);
        }

        public override void Step()
        {
            game = game.Transition(getValidMove());
            ShowStep();
        }
    }

    public class NetworkCommandLine : CongoCommandLine
    {
        private readonly GrpcChannel channel;
        private readonly CongoGrpc.CongoGrpcClient client;
        private readonly long gameId;
        private long moveId = -1;

        private CongoGame getNetworkMove()
        {
            int cnt = 0;

            do {
                break;
            } while (true);

            return game;
        }

        public NetworkCommandLine(GrpcChannel channel, CongoGrpc.CongoGrpcClient client, CongoArgs args, long gameId)
        {
            this.channel = channel;
            this.client = client;
            this.gameId = gameId;

            whiteUser = createMaybeNetworkUser(White.Color);
            blackUser = createMaybeNetworkUser(Black.Color);

            game = CongoFen.FromFen(client.GetLastFen(new GetLastFenRequest() { GameId = gameId }).Fen);

            greet();
            showNetworkGameId(gameId);
            showBoard(game);
            showPlayers(game);
        }

        public override void Step()
        {
            if (activeUser is not Net) {
                var move = getValidMove();
                game = game.Transition(move);
                moveId = client.PostMove(new PostMoveRequest() { GameId = gameId, Fr = move.Fr, To = move.To }).MoveId;
            }

            // TODO: continue here!

            else {
                game = getNetworkMove();
            }
        }
    }
}
