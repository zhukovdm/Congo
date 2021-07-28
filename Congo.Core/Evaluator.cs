namespace Congo.Core
{
	public static class Evaluator
	{
		public static int Basic(CongoGame game)
		{
			if (game.IsWin()) { return game.WhitePlayer.HasLion ? 1 : -1; }
			return 0;
		}
	}
}
