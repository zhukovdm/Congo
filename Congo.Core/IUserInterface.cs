namespace Congo.Core
{
	public interface ICongoUserInterface
	{
		void Greet();
		void Show(CongoBoard board);
		CongoUserCommand GetUserCommand();
		CongoUserCommand ForceStart();
		void ReportAdvice(CongoMove move, HeuristicCode heuristic);
		void ReportExit(string message);
	}
}
