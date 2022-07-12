using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal sealed class ControlMenuWrapper : IPanelWrapper
    {
        private readonly MenuItem pause, cancel;

        public ControlMenuWrapper(MenuItem pause, MenuItem cancel)
        {
            this.pause = pause;
            this.cancel = cancel;
        }

        public void Init()
        {
            pause.IsEnabled = false;
            cancel.IsEnabled = false;
        }

        public void Reset() => Init();
    }
}
