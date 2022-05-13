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
        /// Verifies user name contains only alphanumeric chars.
        /// </summary>
        public static bool IsUserNameValid(string name)
        {
            return name.Length > 0
                && name.All(char.IsLetterOrDigit);
        }

        /// <summary>
        /// Verifies ip address holder is a number between 0 and 255.
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
    }
}
