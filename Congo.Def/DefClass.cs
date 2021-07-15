namespace Congo.Def {

	public class UserCommand : IUserCommand { }

	public sealed class HelpUserCommand : UserCommand {
		public readonly string Command;
		public HelpUserCommand(string command) {
			Command = command;
		}
	}

	public sealed class PlayUserCommand : UserCommand {
		public readonly string White;
		public readonly string Black;
		public PlayUserCommand(string white, string black) {
			White = white; Black = black;
		}
	}

	public sealed class AdviseUserCommand : UserCommand {
		public readonly int Heuristic;
		public AdviseUserCommand(int heuristic) {
			Heuristic = heuristic;
		}
	}

	public sealed class MoveUserCommand : UserCommand {
		public readonly int Fr;
		public readonly int To;
		public MoveUserCommand(int fr, int to) {
			Fr = fr; To = to;
		}
	}

}
