using System.Linq;

namespace Congo.Utils
{
    public class UserInput
    {
        public static bool IsUserNameValid(string name)
        {
            return name.Length > 0
                && name.All(char.IsLetterOrDigit);
        }

        public static bool IsIpAddressHolderValid(string holder)
        {
            return holder.Length > 0
                && holder.Length < 4
                && holder.All(char.IsDigit)
                && int.TryParse(holder, out var result)
                && result >= 0
                && result <= 255;
        }

        public static bool IsPortAddressValid(string port)
        {
            return port.Length == 5
                && port.All(char.IsDigit)
                && int.TryParse(port, out var result)
                && result >= 49152
                && result <= 65535;
        }
    }
}
