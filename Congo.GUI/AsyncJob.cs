using Congo.Core;
using Congo.Server;
using Congo.Utils;
using Grpc.Core;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Windows.Controls;

namespace Congo.GUI
{
    internal enum NetStatus { Ok, GrpcError, ServerRequestError };

    internal abstract class AsyncJob
    {
        protected static ManualResetEventSlim _pauseEvent;

        static AsyncJob() { _pauseEvent = new(true); }

        private static void Pause(MenuItem item)
        {
            _pauseEvent.Reset();
            item.Header = "Resu_me";
        }

        public static void Resume(MenuItem item)
        {
            _pauseEvent.Set();
            item.Header = "_Pause";
        }

        public static void Invert(MenuItem item)
        {
            if (_pauseEvent.IsSet) { Pause(item); } else { Resume(item); }
        }

        public abstract AsyncJob Run();

        public abstract void Cancel();

        public abstract void Abandon();
    }

    internal sealed class AsyncAdvise : AsyncJob
    {
        private readonly bool _sleep;
        private readonly CongoGame _game;
        private readonly CongoUser _user;

        public CongoGame NewGame { get; private set; }
        public CongoMove AdvisedMove { get; private set; }

        public AsyncAdvise(CongoGame game, CongoUser user, bool sleep)
        {
            _game = game;
            _user = user;
            _sleep = sleep;
        }

        public override AsyncJob Run()
        {
            _pauseEvent.Wait();

            if (_sleep) { Thread.Sleep(1000); } // helps to avoid too fast ai-ai switching

            AdvisedMove = _user.Advise(_game);
            if (AdvisedMove is null) { AdvisedMove = Algorithm.Random(_game); }

            NewGame = _game.Transition(AdvisedMove);

            return this;
        }

        public override void Cancel() => Algorithm.Cancel();

        public override void Abandon() => Algorithm.Cancel();
    }

    internal sealed class AsyncNetMove : AsyncJob
    {
        private volatile bool _abandon;
        private readonly CongoUser _white, _black;
        private readonly ImmutableList<CongoMove> _movesOut;

        public CongoGame NewGame { get; private set; }

        public NetPack NewNetPack { get; private set; }

        public ICollection<DbMove> DbMoves { get; private set; }

        public NetStatus Status { get; private set; }

        public string ErrorMessage { get; private set; }

        public AsyncNetMove(NetPack netPack, CongoUser white, CongoUser black, ImmutableList<CongoMove> movesOut)
        {
            _white = white;
            _black = black;
            _abandon = false;
            _movesOut = movesOut;
            NewNetPack = netPack;
            Status = NetStatus.Ok;
        }

        public override AsyncJob Run()
        {
            try {
                foreach (var move in _movesOut) {
                    NewNetPack = NewNetPack.WithMoveId(GrpcRoutines.PostMove(NewNetPack.Client, NewNetPack.GameId, NewNetPack.MoveId, move));
                }

                CongoUser u;
                do {

                    Thread.Sleep(500);
                    NewGame = GrpcRoutines.GetLatestGame(NewNetPack.Client, NewNetPack.GameId);
                    u = NewGame.GetActiveUser(_white, _black);

                } while (!_abandon && u is Net && !NewGame.HasEnded());

                DbMoves = GrpcRoutines.GetDbMovesAfter(NewNetPack.Client, NewNetPack.GameId, NewNetPack.MoveId);
                NewNetPack = NewNetPack.WithMoveId(NewNetPack.MoveId + DbMoves.Count);
            }
            catch (CongoServerResponseException ex) {
                Status = NetStatus.ServerRequestError;
                ErrorMessage = $"{GrpcRoutinesGui.ServerResponseErrorPrefix}{ex.Message}";
            }
            catch (RpcException ex) {
                Status = NetStatus.GrpcError;
                ErrorMessage = $"{GrpcRoutinesGui.GrpcErrorPrefix}{ex.StatusCode}";
            }

            return this;
        }

        public override void Cancel() { }

        public override void Abandon() => _abandon = true;
    }
}
