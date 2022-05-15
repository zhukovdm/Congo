using System.Linq;

namespace Congo.Utils
{
    /// <summary>
    /// Class containing supporting methods used in different parts
    /// of the project.
    /// </summary>
    public static class UserInput
    {
        /// <summary>
        /// Verifies user name contains only alphanumeric chars, [A-Za-z0-9]+
        /// </summary>
        public static bool IsUserNameValid(string name)
        {
            return name.Length > 0
                && name.All(char.IsLetterOrDigit);
        }

        /// <summary>
        /// Verifies IPv4 address holder is an integer number between 0 and 255.
        /// </summary>
        public static bool IsIpAddressHolderValid(string holder)
        {
            return holder.Length > 0
                && holder.Length < 4
                && holder.All(char.IsDigit)
                && int.TryParse(holder, out var result)
                && result >= 0
                && result <= 255;
        }

        /// <summary>
        /// Verifies the input is a valid IPv4 address in "127.0.0.0" format.
        /// </summary>
        public static bool IsIpAddressValid(string ip)
        {
            var spl = ip.Split(new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);

            var result = spl.Length == 4;
            foreach (var s in spl) { result &= IsIpAddressHolderValid(s); }

            return result;
        }

        /// <summary>
        /// Verifies entered port is any number between 1024 and 65535.
        /// </summary>
        public static bool IsPortValid(string port)
        {
            return port.Length == 5
                && port.All(char.IsDigit)
                && int.TryParse(port, out var result)
                && result >= 1024
                && result <= 65535;
        }

        /// <summary>
        /// Verifies entered game id, [0-9]+
        /// </summary>
        public static bool IsBoardIdValid(string id)
        {
            return id.Length > 0
                && id.All(char.IsDigit);
        }
    }
}
