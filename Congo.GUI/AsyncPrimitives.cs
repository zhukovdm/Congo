using System.ComponentModel;

namespace Congo.GUI
{
    internal static class AsyncPrimitives
    {
        public static BackgroundWorker GetWorker(DoWorkEventHandler doer, RunWorkerCompletedEventHandler completer)
        {
            var worker = new BackgroundWorker();

            worker.DoWork += doer;
            worker.RunWorkerCompleted += completer;

            return worker;
        }
    }
}
