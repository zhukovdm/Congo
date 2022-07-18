using System.IO;

namespace Congo.CLI
{
    internal sealed class GreetView
    {
        private static GreetView _instance;

        private GreetView() { }

        public static GreetView GetInstance() => _instance ??= new GreetView();

        public override string ToString()
        {
            StringWriter writer = new();

            writer.WriteLine(@"   ____                        ");
            writer.WriteLine(@"  / ___|___  _ __   __ _  ___  ");
            writer.WriteLine(@" | |   / _ \| '_ \ / _` |/ _ \ ");
            writer.WriteLine(@" | |__| (_) | | | | (_| | (_) |");
            writer.WriteLine(@"  \____\___/|_| |_|\__, |\___/ ");
            writer.WriteLine(@"                   |___/       ");

            return writer.ToString();
        }
    }

    public abstract class HelpView
    {
        private static string decorateRow(int idx, string row)
            => (idx % 2 == 0 ? " " : "    ") + row;

        protected static string toView(string name, string description, string usage)
        {
            string[] rows = new[]
            {
                "NAME", name, "DESCRIPTION", description, "USAGE", usage,
            };

            StringWriter writer = new();

            for (int i = 0; i < rows.Length; ++i) {
                writer.WriteLine(decorateRow(i, rows[i]));
            }

            return writer.ToString();
        }
    }

    internal sealed class AdviseHelpView : HelpView
    {
        private static AdviseHelpView _instance;

        private AdviseHelpView() { }

        public static HelpView GetInstance() => _instance ??= new AdviseHelpView();

        public override string ToString()
            => toView("advise", "Advises next move based on a chosen algorithm.", "advise");
    }

    internal sealed class ExitHelpView : HelpView
    {
        private static ExitHelpView _instance;

        private ExitHelpView() { }

        public static HelpView GetInstance() => _instance ??= new ExitHelpView();

        public override string ToString()
            => toView("exit", "Exits the game.", "exit");
    }

    internal sealed class HelpHelpView : HelpView
    {
        private static HelpHelpView _instance;

        private HelpHelpView() { }

        public static HelpView GetInstance() => _instance ??= new HelpHelpView();

        public override string ToString()
            => toView("help", "Shows help manual for a chosen command.", "help advise | exit | help | move | show");
    }

    internal sealed class MoveHelpView : HelpView
    {
        private static MoveHelpView _instance;

        private MoveHelpView() { }

        public static HelpView GetInstance() => _instance ??= new MoveHelpView();

        public override string ToString()
            => toView("move", "Moves a certain piece from a square to a square.", "move [a-g][1-7] [a-g][1-7]");
    }

    internal sealed class ShowHelpView : HelpView
    {
        private static ShowHelpView _instance;

        private ShowHelpView() { }

        public static HelpView GetInstance() => _instance ??= new ShowHelpView();

        public override string ToString()
            => toView("show", "Shows game statistics.", "show board | players | moves | game");
    }
}
