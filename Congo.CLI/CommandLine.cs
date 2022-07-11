using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using Grpc.Net.Client;
using Congo.Core;
using Congo.Server;
using Congo.Utils;

namespace Congo.CLI
{
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
            var channel = GrpcPrimitives.CreateRpcChannel(args.GetMaybeHost(), args.GetMaybePort());
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
