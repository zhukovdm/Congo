using System.Linq;

namespace Congo.Utils
{
    /// <summary>
    /// Class containing supporting methods used in different parts of the project.
    /// </summary>
    public static class UserInput
    {
        private static readonly string zero = "0";
        private static readonly string localhostValue = "localhost";

        /// <summary>
        /// Verifies IPv4 address holder is an integer number between 0 and 255.
        /// Trailing zeros are <b>not</b> allowed.
        /// </summary>
        private static bool isValidIpAddressHolder(string holder)
        {
            return holder is not null
                && holder.Length > 0
                && holder.Length < 4
                && (holder == zero || !holder.StartsWith('0'))
                && holder.All(char.IsDigit)
                && int.TryParse(holder, out var result) // obtain result, parsing always succeeds
                && result >= 0
                && result <= 255;
        }

        /// <summary>
        /// Verifies entered game id, pattern [0-9]+
        /// Trailing zeros are <b>not</b> allowed.
        /// </summary>
        public static bool IsValidBoardId(string id)
        {
            return id is not null
                && id.Length > 0
                && (id == zero || !id.StartsWith('0'))
                && id.All(char.IsDigit);
        }

        /// <summary>
        /// Verifies the input is a valid IPv4 address in <b>127.0.0.0</b>
        /// format. <b>localhost</b> is also acceptable.
        /// </summary>
        public static bool IsValidHost(string addr)
        {
            if (addr is null) { return false; }

            if (addr == localhostValue) { return true; }

            var spl = addr.Split(new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);

            var result = spl.Length == 4;
            foreach (var s in spl) { result &= isValidIpAddressHolder(s); }

            return result;
        }

        /// <summary>
        /// Verifies entered port is any number between 1024 and 65535.
        /// Trailing zeros are <b>not</b> allowed.
        /// </summary>
        public static bool IsValidPort(string port)
        {
            return port is not null
                && port.Length >= 4
                && port.Length <= 5
                && !port.StartsWith('0')
                && port.All(char.IsDigit)
                && int.TryParse(port, out var result) // obtain result, parsing always succeeds
                && result >= 1024
                && result <= 65535;
        }
    }
}
