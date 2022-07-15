using Congo.Core;
using Congo.Utils;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using CongoClient = Congo.Server.CongoGrpc.CongoGrpcClient;

namespace Congo.CLI
{
    public class ExitException : Exception { }

    public abstract class CongoCommandLine
    {
        #region create game

        private static CongoCommandLine createLocalGame(CongoArgs args)
        {
            var game = args.IsGameStandard()
                ? CongoGame.Standard()
                : CongoFen.FromFen(args.GetMaybeGameValue());

            return new LocalCommandLine(game, args);
        }

        private static CongoCommandLine createNetworkGame(CongoArgs args)
        {
            var channel = GrpcPrimitives.CreateGrpcChannel(args.GetMaybeHost(), args.GetMaybePort());
            var client = new CongoClient(channel);

            long gameId = -1;
            var game = args.GetMaybeGameValue();

            if (args.IsGameStandard() || args.IsGameValidCongoFen()) {
                gameId = GrpcRoutines.PostFen(client, game);
            }

            if (args.IsGameValidId()) { gameId = long.Parse(game); }

            GrpcRoutines.ConfirmGameId(client, gameId);

            return new NetworkCommandLine(channel, client, args, gameId);
        }

        public static CongoCommandLine Create(CongoArgs args)
            => args.IsPlaceLocal() ? createLocalGame(args) : createNetworkGame(args);

        #endregion

        protected CongoArgs args;
        protected CongoGame game;
        protected CongoUser whiteUser, blackUser;
        protected TextReader reader;
        protected TextWriter writer;
        protected TextReporter reporter;
        protected TextPresenter presenter;

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
            return args.IsPlayerAi(color) ? new Ai(algo) : new Hi(algo);
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
            => game.GetActiveUser(whiteUser, blackUser).Advise(game);

        private CongoMove getHiValidMove()
        {
            CongoMove move = null;
            var showDelegates = new Dictionary<string, Action<CongoGame>>()
            {
                { TextPresenter.GameLiteral, presenter.ShowGame },
                { TextPresenter.BoardLiteral, presenter.ShowBoard },
                { TextPresenter.MovesLiteral, presenter.ShowMoves },
                { TextPresenter.PlayersLiteral, presenter.ShowPlayers },
            };

            do {
                var command = getUserCommand();

                switch (command[0]) {

                    case Verifier.AdviseLiteral:
                        move = game.GetActiveUser(whiteUser, blackUser).Advise(game);
                        reporter.ReportAdvisedMove(move);
                        move = null;
                        break;

                    case Verifier.ExitLiteral:
                        throw new ExitException();

                    case Verifier.HelpLiteral:
                        if (command.Length == 1) { command = new string[] { Verifier.HelpLiteral, Verifier.HelpLiteral, }; }
                        reporter.ReportHelpFile(command[1]);
                        break;

                    case Verifier.MoveLiteral:
                        move = new CongoMove(MovePresenter.SquareViews.IndexOf(command[1]),
                                             MovePresenter.SquareViews.IndexOf(command[2]));
                        break;

                    case Verifier.ShowLiteral:
                        showDelegates[command[1]].Invoke(game);
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
            return game.GetActiveUser(whiteUser, blackUser) is Ai
                ? getAiValidMove()
                : getHiValidMove();
        }

        #endregion

        #region public interface

        public bool End() => game.HasEnded();

        public void ReportResult() => reporter.ReportResult(game);

        public abstract CongoCommandLine Init();

        public abstract void Step();

        #endregion
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
        private long moveId = -1;
        private readonly long gameId;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly GrpcChannel channel;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly CongoClient client;

        private CongoGame getNetworkMove()
        {
            bool predicate(CongoGame game)
                => game.GetActiveUser(whiteUser, blackUser) is Net && !game.HasEnded();

            var newGame = GrpcRoutines.GetLatestGame(client, gameId);

            if (predicate(newGame)) {
                int cnt = 0;

                // wait for opponent's move(s)
                do {
                    if (cnt == 0) { writer.WriteLine(); writer.Write(' '); }
                    writer.Write('.');
                    cnt = (cnt + 1) % 30;
                    Thread.Sleep(1000);
                    newGame = GrpcRoutines.GetLatestGame(client, gameId);
                } while (predicate(newGame));

                writer.WriteLine();
            }

            moveId = GrpcRoutinesCli.SyncMoves(client, gameId, moveId, presenter);
            return newGame;
        }

        

        public NetworkCommandLine(GrpcChannel channel, CongoClient client, CongoArgs args, long gameId)
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
            presenter.ShowBoard(GrpcRoutines.GetFirstGame(client, gameId));
            moveId = GrpcRoutinesCli.SyncMoves(client, gameId, moveId, presenter);
            game = GrpcRoutines.GetLatestGame(client, gameId);
            presenter.ShowBoard(game);
            presenter.ShowPlayers(game);

            return this;
        }

        public override void Step()
        {
            if (game.GetActiveUser(whiteUser, blackUser) is not Net) {
                var move = getValidMove();
                game = game.Transition(move);
                moveId = GrpcRoutines.PostMove(client, gameId, moveId, move);
            }

            else {
                game = getNetworkMove();
                presenter.ShowBoard(game);
            }
        }
    }
}
