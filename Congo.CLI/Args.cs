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
        private const string placeOption = "--place";
        private const string whiteOption = "--white";
        private const string blackOption = "--black";
        private const string gameOption = "--game";
        private const string hostOption = "--host";
        private const string portOption = "--port";

        private const string localValue = "local";
        private const string networkValue = "network";
        private const string aiValue = "ai";
        private const string hiValue = "hi";
        private const string randomValue = "random";
        private const string negamaxValue = "negamax";
        private const string standardValue = "standard";

        public static class Parser
        {
            #region argument acceptors

            private static string[] acceptLocalPlace(string arg)
            {
                return arg == localValue
                    ? new string[] { arg }
                    : null;
            }

            private static string[] acceptNetworkPlace(string arg)
            {
                return arg == networkValue
                    ? new string[] { arg }
                    : null;
            }

            private static string[] acceptLocalGame(string arg)
            {
                return arg == standardValue || CongoFen.FromFen(arg) != null
                    ? new string[] { arg }
                    : null;
            }

            private static string[] acceptNetworkGame(string arg)
            {
                return arg == standardValue || CongoFen.FromFen(arg) != null || Utils.UserInput.IsValidGameId(arg)
                    ? new string[] { arg }
                    : null;
            }

            private static string[] acceptPlayer(string arg)
            {
                var spl = arg.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                return (spl.Length == 2) && (spl[0] == aiValue || spl[0] == hiValue) && (spl[1] == randomValue || spl[1] == negamaxValue)
                    ? spl
                    : null;
            }

            private static string[] acceptHost(string arg)
            {
                return Utils.UserInput.IsValidHost(arg)
                    ? new string[] { arg }
                    : null;
            }

            private static string[] acceptPort(string arg)
            {
                return Utils.UserInput.IsValidPort(arg)
                    ? new string[] { arg }
                    : null;
            }

            #endregion

            private static ParsedDict tryParse(string[] args, AcceptDict acceptors, int cnt)
            {
                var result = new Dictionary<string, ImmutableArray<string>>();

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
                    { placeOption, acceptLocalPlace },
                    { whiteOption, acceptPlayer },
                    { blackOption, acceptPlayer },
                    { gameOption, acceptLocalGame },
                };

                var networkAcceptors = new AcceptDict
                {
                    { placeOption, acceptNetworkPlace },
                    { whiteOption, acceptPlayer },
                    { blackOption, acceptPlayer },
                    { gameOption, acceptNetworkGame },
                    { hostOption, acceptHost },
                    { portOption, acceptPort },
                };

                var result = tryParse(args, localAcceptors, localAcceptors.Count);

                if (result is null) {

                    /* network configuration should contain either white 
                     * or black player configuration, but not both!
                     */
                    result = tryParse(args, networkAcceptors, networkAcceptors.Count - 1); // only 5 
                    result = (result?.ContainsKey(whiteOption) == true && result?.ContainsKey(blackOption) == true)
                        ? null
                        : result;
                }

                if (result is null) { throw new ArgumentException("Malformed arguments detected."); }

                return new CongoArgs(result);
            }
        }

        private readonly ParsedDict parsedArgs;

        private static string getColorHandle(CongoColor color)
        {
            return color == White.Color
                ? whiteOption
                : blackOption;
        }

        private string getMaybeValue(string key, int idx)
        {
            return parsedArgs.ContainsKey(key) && parsedArgs[key].Length > idx
                ? parsedArgs[key][idx]
                : null;
        }

        private bool isExpectedValue(string key, int idx, string value)
            => getMaybeValue(key, idx) == value;

        private bool isPlayerType(CongoColor color, string type)
            => isExpectedValue(getColorHandle(color), 0, type);

        private CongoArgs(ParsedDict parsedArgs)
        {
            this.parsedArgs = parsedArgs;
        }

        public bool IsPlaceLocal()
            => isExpectedValue(placeOption, 0, localValue);

        public bool IsPlaceNetwork()
            => isExpectedValue(placeOption, 0, networkValue);

        public bool IsGameStandard()
            => isExpectedValue(gameOption, 0, standardValue);

        public bool IsGameValidCongoFen()
            => CongoFen.FromFen(getMaybeValue(gameOption, 0)) is not null;

        public bool IsGameValidId()
            => Utils.UserInput.IsValidGameId(getMaybeValue(gameOption, 0));

        public string GetMaybeGameValue()
            => getMaybeValue(gameOption, 0);

        public bool IsPlayerAi(CongoColor color)
            => isPlayerType(color, aiValue);

        public bool IsPlayerHi(CongoColor color)
            => isPlayerType(color, hiValue);

        public CongoColor GetLocalPlayerColor()
        {
            return parsedArgs.ContainsKey(whiteOption)
                ? White.Color
                : Black.Color;
        }

        public AlgorithmDelegate GetAdvisingDelegate(CongoColor color)
        {
            AlgorithmDelegate result = Algorithm.Random;

            var val = getMaybeValue(getColorHandle(color), 1);
            if (val is not null && val == negamaxValue) {
                result = Algorithm.Negamax;
            }

            return result;
        }

        public string GetMaybeHost()
            => getMaybeValue(hostOption, 0);

        public string GetMaybePort()
            => getMaybeValue(portOption, 0);
    }
}
