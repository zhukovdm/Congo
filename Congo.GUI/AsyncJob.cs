using Congo.Core;
using Congo.Server;
using Congo.Utils;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace Congo.GUI
{
    internal enum NetStatus { Ok, GrpcError, ServerRequestError };

    internal abstract class AsyncJob
    {
        protected static ManualResetEventSlim pauseEvent;

        static AsyncJob() { pauseEvent = new(true); }

        private static void Pause(MenuItem item)
        {
            pauseEvent.Reset();
            item.Header = "Resu_me";
        }

        public static void Resume(MenuItem item)
        {
            pauseEvent.Set();
            item.Header = "_Pause";
        }

        public static void Invert(MenuItem item)
        {
            if (pauseEvent.IsSet) { Pause(item); } else { Resume(item); }
        }

        public abstract AsyncJob Run();

        public abstract void Cancel();

        public abstract void Abandon();
    }

    internal sealed class AsyncAdvise : AsyncJob
    {
        private readonly bool sleep;
        private readonly CongoUser user;

        public CongoGame Game { get; private set; }

        public CongoMove Move { get; private set; }

        public AsyncAdvise(CongoGame game, CongoUser user, bool sleep)
        {
            Game = game;
            this.user = user;
            this.sleep = sleep;
        }

        public override AsyncJob Run()
        {
            pauseEvent.Wait();
            if (sleep) { Thread.Sleep(1000); } // helps to avoid too fast ai-ai switching

            Move = user.Advise(Game);

            return this;
        }

        public override void Cancel() => Algorithm.Cancel();

        public override void Abandon() => Algorithm.Cancel();
    }

    internal sealed class AsyncNetMove : AsyncJob
    {
        private volatile bool abandon;
        private readonly CongoMove move;
        private readonly CongoUser white, black;

        public CongoGame Game { get; private set; }

        public NetPack NetPack { get; private set; }

        public ICollection<DbMove> Moves { get; private set; }

        public NetStatus Status { get; private set; }

        public string ErrorMessage { get; private set; }

        public AsyncNetMove(NetPack netPack, CongoMove move, CongoUser white, CongoUser black)
        {
            abandon = false;
            this.move = move;
            NetPack = netPack;
            this.white = white;
            this.black = black;
            Status = NetStatus.Ok;
        }

        public override AsyncJob Run()
        {
            try {
                if (move is not null) {
                    NetPack.MoveId = GrpcRoutines.PostMove(NetPack.Client, NetPack.GameId, NetPack.MoveId, move);
                }

                CongoUser u;
                do {

                    Thread.Sleep(1000);
                    Game = GrpcRoutines.GetLatestGame(NetPack.Client, NetPack.GameId);
                    u = Game.GetActiveUser(white, black);

                } while (!abandon && u is Net && !Game.HasEnded());

                Moves = GrpcRoutines.GetDbMovesAfter(NetPack.Client, NetPack.GameId, NetPack.MoveId);
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

        public override void Abandon() => abandon = true;
    }
}
