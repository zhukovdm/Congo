namespace Congo.Core
{
	public abstract class CongoUserCommand { }

	public sealed class AdviseCommand : CongoUserCommand
	{
		public readonly HeuristicCode Heuristic;

		public AdviseCommand(HeuristicCode heuristic)
		{
			Heuristic = heuristic;
		}
	}

	public sealed class ExitCommand : CongoUserCommand { }

	public sealed class PlayCommand : CongoUserCommand
	{
		public readonly PlayerCode White;
		public readonly PlayerCode Black;
		public PlayCommand(PlayerCode white, PlayerCode black)
		{
			White = white;
			Black = black;
		}
	}

	public sealed class MoveCommand : CongoUserCommand
	{
		public readonly int Fr;
		public readonly int To;
		public MoveCommand(int fr, int to)
		{
			Fr = fr;
			To = to;
		}
	}

	public sealed class ShowCommand : CongoUserCommand { }
}
