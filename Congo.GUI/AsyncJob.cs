using Congo.Core;
using System.Threading;

namespace Congo.GUI
{
    internal abstract class AsyncJob
    {
        protected static ManualResetEventSlim waitEvent, pauseEvent;

        static AsyncJob() { waitEvent = new(true); pauseEvent = new(true); }

        protected bool abandon = false;

        public static void Wait() => waitEvent.Wait();

        public static void Acquire()
        {
            waitEvent.Wait();
            waitEvent.Reset();
        }

        public static void Release() => waitEvent.Set();

        public static void Pause() => pauseEvent.Reset();

        public static void Resume() => pauseEvent.Set();

        public bool IsAbandoned() => abandon == true;

        public abstract AsyncJob Run();

        public abstract void Cancel();

        public abstract void Abandon();
    }

    internal sealed class AsyncAdvise : AsyncJob
    {
        public CongoGame Game;
        public CongoUser White, Black;
        public CongoMove Move;

        public AsyncAdvise(CongoGame game, CongoUser white, CongoUser black)
        {
            Game = game;
            White = white;
            Black = black;
        }

        public override AsyncJob Run()
        {
            pauseEvent.Wait();

            // avoid too fast user switching
            if (White is Ai && Black is Ai) { Thread.Sleep(1000); }

            var user = Game.ActivePlayer.IsWhite() ? White : Black;
            Move = user.Advise(Game);

            return this;
        }

        public override void Cancel() => Algorithm.Cancel();

        public override void Abandon()
        {
            abandon = true;
            Algorithm.Cancel();
        }
    }
}
