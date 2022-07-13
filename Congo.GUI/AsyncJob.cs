using Congo.Core;
using System.Threading;
using System.Windows.Controls;

namespace Congo.GUI
{
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
    }

    internal sealed class AsyncAdvise : AsyncJob
    {
        private readonly bool sleep;
        private readonly CongoUser User;

        public readonly CongoGame Game;
        public CongoMove Move { get; private set; }

        public AsyncAdvise(CongoGame game, CongoUser user, bool sleep)
        {
            Game = game;
            User = user;
            this.sleep = sleep;
        }

        public override AsyncJob Run()
        {
            pauseEvent.Wait();
            if (sleep) { Thread.Sleep(1000); } // avoid too fast user switching

            Move = User.Advise(Game);

            return this;
        }

        public override void Cancel() => Algorithm.Cancel();
    }

    /*
    internal sealed class AsyncNetMove : AsyncJob
    {
        private volatile bool cancel = false;

        public AsyncNetMove() { }
    }
    */
}
