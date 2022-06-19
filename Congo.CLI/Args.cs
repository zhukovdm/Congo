using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Congo.Core;

namespace Congo.CLI
{
    public class CongoArgs
    {
        public static class Parser
        {
            #region Acceptors

            private static string[] AcceptLocalPlace(string arg)
                => (arg == "local")
                    ? new string[] { arg } : null;

            private static string[] AcceptLocalBoard(string arg)
                => (arg == "standard") || (CongoFen.FromFen(arg) != null)
                    ? new string[] { arg } : null;

            private static string[] AcceptNetworkPlace(string arg)
                => (arg == "network")
                    ? new string[] { arg } : null;

            private static string[] AcceptNetworkBoard(string arg)
                => (arg == "standard") || (CongoFen.FromFen(arg) != null) || Utils.UserInput.IsBoardIdValid(arg)
                    ? new string[] { arg } : null;

            private static string[] AcceptNetworkHost(string arg)
                => Utils.UserInput.IsIpAddressValid(arg)
                    ? new string[] { arg } : null;

            private static string[] AcceptNetworkPort(string arg)
                => Utils.UserInput.IsPortValid(arg)
                    ? new string[] { arg } : null;

            private static string[] AcceptPlayer(string arg)
            {
                var spl = arg.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                return (spl.Length == 2) && Utils.UserInput.IsUserNameValid(spl[0]) && (spl[1] == "random" || spl[1] == "negamax")
                    ? spl : null;
            }

            #endregion

            private static ImmutableDictionary<string, ImmutableArray<string>> TryParse(
                string[] args, Dictionary<string, Func<string, string[]>> acceptors, int cnt)
            {
                var result = new Dictionary<string, ImmutableArray<string>>();

                // at least count (repeated are not handled here)
                if (args.Length != cnt) { return null; }

                foreach (var arg in args) {
                    var spl = arg.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (spl.Length != 2) { return null; }

                    // unknown option
                    if (!acceptors.ContainsKey(spl[0])) { return null; }

                    // already seen option
                    if (result.ContainsKey(spl[0])) { return null; }

                    // predicate is not satisfied
                    var acc = acceptors[spl[0]].Invoke(spl[1]);
                    if (acc == null) { return null; }

                    result[spl[0]] = acc.ToImmutableArray();
                }

                return result.ToImmutableDictionary();
            }

            /// <summary>
            /// Try to fit either network or local arguments. Fail on both is
            /// reported via exception.
            /// </summary>
            public static CongoArgs Parse(string[] args)
            {
                var localAcceptors = new Dictionary<string, Func<string, string[]>>
                {
                    { "--place", AcceptLocalPlace },
                    { "--board", AcceptLocalBoard },
                    { "--white", AcceptPlayer },
                    { "--black", AcceptPlayer },
                };

                var networkAcceptors = new Dictionary<string, Func<string, string[]>>
                {
                    { "--place", AcceptNetworkPlace },
                    { "--host",  AcceptNetworkHost  },
                    { "--port",  AcceptNetworkPort  },
                    { "--board", AcceptNetworkBoard },
                    { "--white", AcceptPlayer },
                    { "--black", AcceptPlayer },
                };

                ImmutableDictionary<string, ImmutableArray<string>> result = null;

                if (result == null) { result = TryParse(args, localAcceptors, localAcceptors.Count); }

                if (result == null) {
                    result = TryParse(args, networkAcceptors, networkAcceptors.Count - 1);
                    if (result != null) {
                        result = (result.ContainsKey("--white") && result.ContainsKey("--black")) ? null : result;
                    }
                }

                if (result == null) { throw new ArgumentException("Malformed arguments detected."); }

                return new CongoArgs(result);
            }
        }

        private static string GetHandle(CongoColor color)
        {
            return color == White.Color ? "--white" : "--black";
        }

        private readonly ImmutableDictionary<string, ImmutableArray<string>> parsedArgs;

        private CongoArgs(ImmutableDictionary<string, ImmutableArray<string>> parsedArgs)
        {
            this.parsedArgs = parsedArgs;
        }

        public bool IsGameLocal()
            => parsedArgs["--place"][0] == "local";

        public bool IsGameNetwork()
            => !IsGameLocal();

        public bool IsBoardStandard()
            => parsedArgs["--board"][0] == "standard";

        public string GetBoardArg()
            => parsedArgs["--board"][0];

        public bool IsPlayerAi(CongoColor color)
            => parsedArgs[GetHandle(color)][0] == "ai";

        public bool IsPlayerHi(CongoColor color)
            => parsedArgs[GetHandle(color)][0] == "hi";

        public string GetAdvisingRef(CongoColor color)
            => parsedArgs[GetHandle(color)][1];
    }
}
