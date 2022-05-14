using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Congo.Core;

namespace Congo.CLI
{
    public class ArgumentParser
    {
        private bool IsLocalPlayValid(string arg)
            => arg == "local";

        private bool IsLocalGameValid(string arg)
            => arg == "standard" || CongoFen.FromFen(arg) != null;

        private bool IsNetworkPlayValid(string arg)
            => arg == "network";

        private bool IsNetworkGameValid(string arg)
            => arg == "standard" || Utils.UserInput.IsGameIdValid(arg);

        private bool IsPlayerValid(string arg)
        {
            var spl = arg.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return (spl.Length == 3)
                && Utils.UserInput.IsUserNameValid(spl[0])
                && (spl[1] == "ai" || spl[1] == "hi")
                && (spl[2] == "rnd" || spl[2] != "negamax");
        }

        private ImmutableDictionary<string, string> TryParse(string[] args, Dictionary<string, Func<string, bool>> predicates)
        {
            var result = new Dictionary<string, string>();

            // each option shall appear exactly once
            if (args.Length != predicates.Count) { return null; }

            foreach (var arg in args) {
                var spl = arg.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                // unknown option
                if (!predicates.ContainsKey(spl[0])) { return null; }

                // already seen option
                if (result.ContainsKey(spl[0])) { return null; }

                // predicate is not satisfied
                if (!predicates[spl[0]].Invoke(spl[1])) { return null; }

                result.Add(spl[0], spl[1]);
            }

            return result.ToImmutableDictionary();
        }

        /// <summary>
        /// Try to fit either network or local arguments.
        /// </summary>
        public ImmutableDictionary<string, string> Parse(string[] args)
        {
            ImmutableDictionary<string, string> result = null;

            var localArgs = new Dictionary<string, Func<string, bool>>
            {
                { "--play", IsLocalPlayValid },
                { "--game", IsLocalGameValid },
                { "--player1", IsPlayerValid },
                { "--player2", IsPlayerValid }
            };

            var networkArgs = new Dictionary<string, Func<string, bool>>
            {
                { "--play", IsNetworkPlayValid },
                { "--game", IsNetworkGameValid },
                { "--player", IsPlayerValid },
                { "--host", Utils.UserInput.IsIpAddressValid },
                { "--port", Utils.UserInput.IsPortValid }
            };

            if (result == null) { result = TryParse(args, localArgs); }
            if (result == null) { result = TryParse(args, networkArgs); }
            if (result == null) { throw new ArgumentException("Malformed arguments detected."); }

            return result;
        }
    }
}
