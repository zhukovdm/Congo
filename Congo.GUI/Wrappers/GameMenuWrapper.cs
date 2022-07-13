using System.Windows.Controls;

namespace Congo.GUI.Wrappers
{
    internal class GameMenuWrapper : IBaseWrapper
    {
        private readonly MenuItem buttonLocal, buttonNetwork;

        public GameMenuWrapper(MenuItem buttonLocal, MenuItem buttonNetwork)
        {
            this.buttonLocal = buttonLocal;
            this.buttonNetwork = buttonNetwork;
        }

        public void Init()
        {
            buttonLocal.IsEnabled = true;
            buttonNetwork.IsEnabled = true;
        }

        public void Reset() => Init();

        public void Disable()
        {
            buttonLocal.IsEnabled = false;
            buttonNetwork.IsEnabled = false;
        }
    }
}
