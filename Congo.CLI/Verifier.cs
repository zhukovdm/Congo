using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.CLI
{
    internal static class Verifier
    {
        public const string AdviseLiteral = "advise";
        public const string ExitLiteral = "exit";
        public const string HelpLiteral = "help";
        public const string MoveLiteral = "move";
        public const string ShowLiteral = "show";

        public delegate string[] VerifyCommandDelegate(string[] input);

        public static readonly ImmutableDictionary<string, VerifyCommandDelegate> SupportedCommands =
            new Dictionary<string, VerifyCommandDelegate>() {
                { AdviseLiteral, verifyAdviseCommand },
                { ExitLiteral, verifyExitCommand },
                { HelpLiteral, verifyHelpCommand },
                { MoveLiteral, verifyMoveCommand },
                { ShowLiteral, verifyShowCommand },
            }.ToImmutableDictionary();

        private static string[] verifyCommand(Func<string[], bool> predicate, string[] input)
        {
            return predicate(input)
                ? null
                : input;
        }

        private static string[] verifyAdviseCommand(string[] input)
        {
            static bool predicate(string[] arr) => arr.Length != 1;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyExitCommand(string[] input)
        {
            static bool predicate(string[] arr) => arr.Length != 1;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyHelpCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 2 || !SupportedCommands.ContainsKey(arr[1]);
            return verifyCommand(predicate, input);
        }

        private static string[] verifyMoveCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 3 || TextPresenter.SquareViews.IndexOf(arr[1]) < 0 || TextPresenter.SquareViews.IndexOf(arr[1]) < 0;
            return verifyCommand(predicate, input);
        }

        private static string[] verifyShowCommand(string[] input)
        {
            static bool predicate(string[] arr)
                => arr.Length != 2 || !TextPresenter.SupportedShows.Contains(arr[1]);
            return verifyCommand(predicate, input);
        }
    }
}
