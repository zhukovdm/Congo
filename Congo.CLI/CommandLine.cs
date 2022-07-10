using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Grpc.Core;
using Grpc.Net.Client;
using Congo.Core;
using Congo.Server;
using Congo.Utils;

namespace Congo.CLI
{
    public sealed class TextPresenter
    {
        public static readonly ImmutableList<string> SquareViews =
            new List<string> {
                "a7", "b7", "c7", "d7", "e7", "f7", "g7",
                "a6", "b6", "c6", "d6", "e6", "f6", "g6",
                "a5", "b5", "c5", "d5", "e5", "f5", "g5",
                "a4", "b4", "c4", "d4", "e4", "f4", "g4",
                "a3", "b3", "c3", "d3", "e3", "f3", "g3",
                "a2", "b2", "c2", "d2", "e2", "f2", "g2",
                "a1", "b1", "c1", "d1", "e1", "f1", "g1",
            }.ToImmutableList();

        private static readonly ImmutableList<Type> pieceTypes =
            new Type[] {
                typeof(Ground), typeof(River), typeof(Elephant), typeof(Zebra),
                typeof(Giraffe), typeof(Crocodile), typeof(Pawn),
                typeof(Superpawn), typeof(Lion), typeof(Monkey),
        }.ToImmutableList();

        private static readonly ImmutableDictionary<Type, string> pieceViews =
            new Dictionary<Type, string>() {
                { typeof(Ground),   "-" }, { typeof(River),     "+" },
                { typeof(Elephant), "e" }, { typeof(Zebra),     "z" },
                { typeof(Giraffe),  "g" }, { typeof(Crocodile), "c" },
                { typeof(Pawn),     "p" }, { typeof(Superpawn), "s" },
                { typeof(Lion),     "l" }, { typeof(Monkey),    "m" },
            }.ToImmutableDictionary();

        private const string boardShow = "board";
        private const string gameShow = "game";
        private const string movesShow = "moves";
        private const string playersShow = "players";

        public static readonly ImmutableHashSet<string> SupportedShows =
            new HashSet<string>() {
                boardShow,
                playersShow,
                movesShow,
                gameShow
            }.ToImmutableHashSet();

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

        public static string GetMoveView(CongoMove move)
            => "(" + SquareViews[move.Fr] + "," + SquareViews[move.To] + ")";

        private readonly TextWriter writer;

        public TextPresenter(TextWriter writer)
        {
            this.writer = writer;
        }

        private void showPlayer(CongoBoard board, CongoColor color, CongoPlayer activePlayer)
        {
            var activeRepr = color == activePlayer.Color
                ? "*"
                : " ";

            var colorRepr = color.IsWhite()
                ? "white"
                : "black";

            var counter = countPieces(board, color);

            writer.Write($" {activeRepr} {colorRepr}");
            for (int i = 2; i < pieceTypes.Count; i++) {
                var pieceRepr = color.IsWhite()
                    ? pieceViews[pieceTypes[i]].ToUpper()
                    : pieceViews[pieceTypes[i]].ToLower();
                writer.Write($" {counter[i]}{pieceRepr}");
            }
            writer.WriteLine();
        }

        public void ShowTransition(CongoGame game)
        {
            writer.WriteLine();
            writer.WriteLine($" transition {GetMoveView(game.TransitionMove)}");
        }

        public void ShowNetworkGameId(long gameId)
        {
            writer.WriteLine();
            writer.WriteLine($" network gameId {gameId}");
        }

        public void ShowBoard(CongoGame game)
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

        public void ShowPlayers(CongoGame game)
        {
            writer.WriteLine();
            showPlayer(game.Board, White.Color, game.ActivePlayer);
            showPlayer(game.Board, Black.Color, game.ActivePlayer);
        }

        public void ShowMoves(CongoGame game)
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

        public void ShowGame(CongoGame game)
        {
            ShowBoard(game);
            ShowPlayers(game);
            ShowMoves(game);
        }

        public void ShowStep(CongoGame game)
        {
            ShowTransition(game);
            ShowBoard(game);
        }

        public void ShowNetworkTransitions(GetDbMovesReply reply)
        {
            writer.WriteLine();
            writer.WriteLine(" transitions " + string.Join(" -> ", reply.Moves.Select(x => GetMoveView(new CongoMove(x.Fr, x.To)))));
        }

        public void ExecuteShowCommand(string token, CongoGame game)
        {
            switch (token) {

                case boardShow:
                    ShowBoard(game);
                    break;

                case playersShow:
                    ShowPlayers(game);
                    break;

                case movesShow:
                    ShowMoves(game);
                    break;

                case gameShow:
                    ShowGame(game);
                    break;

                default:
                    throw new InvalidOperationException(GetType().FullName);
            }
        }
    }

    public sealed class TextReporter
    {
        private static readonly string resourcesFolder = "Resources" + Path.DirectorySeparatorChar;
        private static readonly string textFileExt = ".txt";
        private static readonly string greetFile = "greet";
        private static readonly string whiteView = "white";
        private static readonly string blackView = "black";

        private static string readTextFile(string filename)
        {
            try {
                return File.ReadAllText(resourcesFolder + filename + textFileExt);
            }
            catch (Exception) { return null; }
        }

        private readonly TextWriter writer;

        public TextReporter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Greet()
        {
            writer.WriteLine();
            writer.Write(readTextFile(greetFile));
        }

        public void ReportHelpFile(string helpFile)
            => writer.Write(readTextFile(helpFile));

        public void ReportEmptyCommand()
            => writer.WriteLine(" Input command is an empty string. Try again.");

        public void ReportNotSupportedCommand(string command)
            => writer.WriteLine($" Command {command} is not supported. Consult \"help help\".");

        public void ReportWrongCommandFormat(string command)
            => writer.WriteLine($" Wrong command format. Consult \"help {command}\".");

        public void ReportAdvisedMove(CongoMove move)
            => writer.WriteLine($" Advised move is {TextPresenter.GetMoveView(move)}.");

        public void ReportWrongMove()
            => writer.WriteLine(" Entered move is wrong. Consult \"show moves\".");

        public void ReportResult(CongoGame game)
        {
            var winner = game.WhitePlayer.HasLion
                ? whiteView
                : blackView;

            writer.WriteLine();
            writer.WriteLine($" {winner} wins.");
            writer.WriteLine();
        }
    }

    internal static class Verifier
    {
        public const string AdviseLiteral = "advise";
        public const string ExitLiteral = "exit";
        public const string HelpLiteral = "help";
        public const string MoveLiteral = "move";
        public const string ShowLiteral = "show";

        public delegate string[] VerifyCommandDelegate(string[] input);

        public static readonly ImmutableDictionary<string, VerifyCommandDelegate> SupportedCommands =
            new Dictionary<string, VerifyCommandDelegate>() {
                { AdviseLiteral, verifyAdviseCommand },
                { ExitLiteral, verifyExitCommand },
                { HelpLiteral, verifyHelpCommand },
                { MoveLiteral, verifyMoveCommand },
                { ShowLiteral, verifyShowCommand },
            }.ToImmutableDictionary();

        #region verify methods

        private static string[] verifyCommand(Func<string[], bool> predicate, string[] input)
        {
            return predicate(input)
                ? null
                : input;
        }

        private static string[] verifyAdviseCommand(string[] input)
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
                => arr.Length != 2 || !SupportedCommands.ContainsKey(arr[1]);
            return verifyCommand(predicate, input);
        }

        private static string[] verifyMoveCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 3 || TextPresenter.SquareViews.IndexOf(arr[1]) < 0 || TextPresenter.SquareViews.IndexOf(arr[1]) < 0;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyShowCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 2 || !TextPresenter.SupportedShows.Contains(arr[1]);
            return verifyCommand(predicate, input);
        }

        #endregion
    }

    public abstract class CongoCommandLine
    {
        #region create game

        private static CongoCommandLine createLocalGame(CongoArgs args)
        {
            var game = args.IsBoardStandard()
                ? CongoGame.Standard()
                : CongoFen.FromFen(args.GetMaybeBoardValue());

            return new LocalCommandLine(game, args);
        }

        private static CongoCommandLine createNetworkGame(CongoArgs args)
        {
            var channel = NetworkPrimitives.CreateRpcChannel(args.GetMaybePort(), args.GetMaybeHost());
            var client = new CongoGrpc.CongoGrpcClient(channel);

            long gameId = -1;
            var board = args.GetMaybeBoardValue();

            if (args.IsBoardStandard() || args.IsBoardValidCongoFen()) {
                gameId = client.PostBoard(new PostBoardRequest() { Board = board }).GameId;
            }

            if (args.IsBoardValidId()) {
                gameId = long.Parse(board);
                
                if (!client.CheckGameId(new CheckGameIdRequest() { GameId = gameId }).Exist) {
                    throw new RpcException(new Status(StatusCode.NotFound, "GameId does not exist."));
                }
            }

            return new NetworkCommandLine(channel, client, args, gameId);
        }

        public static CongoCommandLine Create(CongoArgs args)
        {
            return args.IsGameLocal()
                ? createLocalGame(args)
                : createNetworkGame(args);
        }

        #endregion

        protected CongoArgs args;
        protected CongoGame game;
        protected CongoUser whiteUser, blackUser;
        protected TextReader reader;
        protected TextWriter writer;
        protected TextReporter reporter;
        protected TextPresenter presenter;

        protected CongoUser activeUser
        {
            get => game.ActivePlayer.IsWhite()
                ? whiteUser
                : blackUser;
        }

        public CongoCommandLine()
        {
            reader = Console.In;
            writer = Console.Out;
            reporter = new TextReporter(writer);
            presenter = new TextPresenter(writer);
        }

        #region create user

        protected CongoUser createLocalUser(CongoColor color)
        {
            var algo = args.GetAdvisingDelegate(color);
            return args.IsPlayerAi(color)
                ? new Ai(algo)
                : new Hi(algo);
        }

        protected CongoUser createMaybeNetworkUser(CongoColor color)
        {
            return (args.GetLocalPlayerColor() == color)
                ? createLocalUser(color)
                : new Net(args.GetAdvisingDelegate(color));
        }

        #endregion

        #region get valid move

        private string[] getUserCommand()
        {
            string[] input;
            string[] command = null;
            var dict = Verifier.SupportedCommands;

            do {
                writer.WriteLine();
                writer.Write(" > ");
                input = reader.ReadLine().Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (input.Length > 0 && dict.ContainsKey(input[0])) {
                    command = dict[input[0]].Invoke(input);
                    if (command is null) { reporter.ReportWrongCommandFormat(input[0]); }
                }

                else if (input.Length == 0) { reporter.ReportEmptyCommand(); }

                else { reporter.ReportNotSupportedCommand(input[0]); }

            } while (command is null);

            return command;
        }

        private CongoMove getAiValidMove()
            => activeUser.Advise(game);

        private CongoMove getHiValidMove()
        {
            CongoMove move = null;

            do {
                var command = getUserCommand();

                switch (command[0]) {

                    case Verifier.AdviseLiteral:
                        move = activeUser.Advise(game);
                        reporter.ReportAdvisedMove(move);
                        move = null;
                        break;

                    case Verifier.ExitLiteral:
                        throw new ArgumentException("The program is terminated...");

                    case Verifier.HelpLiteral:
                        if (command.Length == 1) { command = new string[] { Verifier.HelpLiteral, Verifier.HelpLiteral, }; }
                        reporter.ReportHelpFile(command[1]);
                        break;

                    case Verifier.MoveLiteral:
                        move = new CongoMove(TextPresenter.SquareViews.IndexOf(command[1]),
                                             TextPresenter.SquareViews.IndexOf(command[2]));
                        break;

                    case Verifier.ShowLiteral:
                        presenter.ExecuteShowCommand(command[1], game);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                if (move is not null) {
                    move = game.ActivePlayer.Accept(move);
                    if (move is null) { reporter.ReportWrongMove(); }
                }

            } while (move is null);

            return move;
        }

        protected CongoMove getValidMove()
        {
            return (activeUser is Ai)
                ? getAiValidMove()
                : getHiValidMove();
        }

        #endregion

        public bool End()
            => game.HasEnded();

        public abstract CongoCommandLine Init();

        public abstract void Step();

        public void ReportResult()
            => reporter.ReportResult(game);
    }

    public sealed class LocalCommandLine : CongoCommandLine
    {
        public LocalCommandLine(CongoGame game, CongoArgs args)
        {
            this.args = args;
            this.game = game;
        }

        public override CongoCommandLine Init()
        {
            whiteUser = createLocalUser(White.Color);
            blackUser = createLocalUser(Black.Color);

            reporter.Greet();
            presenter.ShowBoard(game);
            presenter.ShowPlayers(game);

            return this;
        }

        public override void Step()
        {
            game = game.Transition(getValidMove());
            presenter.ShowStep(game);
        }
    }

    public sealed class NetworkCommandLine : CongoCommandLine
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly GrpcChannel channel;
#pragma warning restore IDE0052 // Remove unread private members

        private readonly CongoGrpc.CongoGrpcClient client;
        private readonly long gameId;
        private long moveId = -1;

        private CongoUser getGameActiveUser(CongoGame game)
            => game.ActivePlayer.IsWhite() ? whiteUser : blackUser;

        private CongoGame getFirstGame()
            => CongoFen.FromFen(client.GetFirstFen(new GetFirstFenRequest() { GameId = gameId }).Fen);

        private CongoGame getLatestGame()
            => CongoFen.FromFen(client.GetLastFen(new GetLastFenRequest() { GameId = gameId }).Fen);

        private GetDbMovesReply getLatestTransitions()
            => client.GetDbMovesFrom(new GetDbMovesFromRequest() { GameId = gameId, From = moveId });

        private void showTransitions(GetDbMovesReply reply)
        {
            presenter.ShowNetworkTransitions(reply);
            moveId += reply.Moves.Count;
        }

        private CongoGame getNetworkMove()
        {
            bool predicate(CongoGame game) => getGameActiveUser(game) is Net && !game.HasEnded();

            var newGame = getLatestGame();

            if (predicate(newGame)) {
                int cnt = 0;

                // wait for opponent's move(s)
                do {
                    if (cnt == 0) { writer.WriteLine(); writer.Write(' '); }
                    writer.Write('.');
                    cnt = (cnt + 1) % 30;
                    Thread.Sleep(1000);

                    newGame = getLatestGame();
                } while (predicate(newGame));

                writer.WriteLine();
            }

            showTransitions(getLatestTransitions());
            return newGame;
        }

        public NetworkCommandLine(GrpcChannel channel, CongoGrpc.CongoGrpcClient client, CongoArgs args, long gameId)
        {
            this.args = args;
            this.gameId = gameId;
            this.client = client;
            this.channel = channel;
        }

        public override CongoCommandLine Init()
        {
            whiteUser = createMaybeNetworkUser(White.Color);
            blackUser = createMaybeNetworkUser(Black.Color);

            reporter.Greet();
            presenter.ShowNetworkGameId(gameId);
            presenter.ShowBoard(getFirstGame());
            showTransitions(getLatestTransitions());
            game = getLatestGame();

            presenter.ShowBoard(game);
            presenter.ShowPlayers(game);

            return this;
        }

        public override void Step()
        {
            if (activeUser is not Net) {
                var move = getValidMove();
                game = game.Transition(move);
                moveId = client.PostMove(new PostMoveRequest() { GameId = gameId, Fr = move.Fr, To = move.To }).MoveId;
            }

            else {
                game = getNetworkMove();
                presenter.ShowBoard(game);
            }
        }
    }
}
