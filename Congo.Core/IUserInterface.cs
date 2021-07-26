namespace Congo.Core
{
	public interface ICongoUserInterface
	{
		CongoMove GetHiMove(CongoGame game);
		void ReportWrongHiMove();
	}
}
