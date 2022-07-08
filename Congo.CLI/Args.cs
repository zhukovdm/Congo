using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Congo.Core;

using ParsedDict = System.Collections.Immutable.ImmutableDictionary<string, System.Collections.Immutable.ImmutableArray<string>>;
using AcceptDict = System.Collections.Generic.Dictionary<string, System.Func<string, string[]>>;

namespace Congo.CLI
{
    public class CongoArgs
    {
        private static readonly ImmutableDictionary<string, AlgorithmDelegate> supportedAlgorithms =
            new Dictionary<string, AlgorithmDelegate> {
                { "random",  Algorithm.Random  },
                { "negamax", Algorithm.Negamax }
            }.ToImmutableDictionary();

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

            private static ParsedDict tryParse(string[] args, AcceptDict acceptors, int cnt)
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
                var localAcceptors = new AcceptDict
                {
                    { "--place", AcceptLocalPlace },
                    { "--board", AcceptLocalBoard },
                    { "--white", AcceptPlayer },
                    { "--black", AcceptPlayer },
                };

                var networkAcceptors = new AcceptDict
                {
                    { "--place", AcceptNetworkPlace },
                    { "--host",  AcceptNetworkHost  },
                    { "--port",  AcceptNetworkPort  },
                    { "--board", AcceptNetworkBoard },
                    { "--white", AcceptPlayer },
                    { "--black", AcceptPlayer },
                };

                ParsedDict result = null;

                if (result == null) { result = tryParse(args, localAcceptors, localAcceptors.Count); }

                if (result == null) {
                    result = tryParse(args, networkAcceptors, networkAcceptors.Count - 1);
                    if (result != null) {
                        result = (result.ContainsKey("--white") && result.ContainsKey("--black")) ? null : result;
                    }
                }

                if (result == null) { throw new ArgumentException("Malformed arguments detected."); }

                return new CongoArgs(result);
            }
        }

        private static string getHandle(CongoColor color)
            => color == White.Color ? "--white" : "--black";

        private readonly ParsedDict parsedArgs;

        private CongoArgs(ParsedDict parsedArgs)
        {
            this.parsedArgs = parsedArgs;
        }

        public bool IsGameLocal()
            => parsedArgs["--place"][0] == "local";

        public bool IsGameNetwork()
            => parsedArgs["--place"][0] == "network";

        public bool IsBoardStandard()
            => parsedArgs["--board"][0] == "standard";

        public bool IsBoardValidCongoFen()
            => CongoFen.FromFen(parsedArgs["--board"][0]) != null;

        public bool IsBoardValidNetId()
            => Utils.UserInput.IsBoardIdValid(parsedArgs["--board"][0]);

        public string GetBoardArg()
            => parsedArgs["--board"][0];

        public bool IsPlayerAi(CongoColor color)
            => parsedArgs[getHandle(color)][0] == "ai";

        public bool IsPlayerHi(CongoColor color)
            => parsedArgs[getHandle(color)][0] == "hi";

        public CongoColor GetLocalPlayerColor()
            => parsedArgs.ContainsKey("--white") ? White.Color : Black.Color;

        public AlgorithmDelegate GetAdvisingDelegate(CongoColor color)
            => supportedAlgorithms[parsedArgs[getHandle(color)][1]];

        public AlgorithmDelegate GetRandomAdvisingDelegate()
            => supportedAlgorithms["random"];

        public string GetHost()
            => parsedArgs["--host"][0];

        /// <summary>
        /// @note Parsing helps to avoid trailing zeroes.
        /// </summary>
        public string GetPort()
            => int.Parse(parsedArgs["--port"][0]).ToString();
    }
}
