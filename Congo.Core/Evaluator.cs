namespace Congo.Core
{
	public static class Evaluator
	{
		public static int Basic(CongoGame game)
		{
			return game.ActivePlayerColor.IsWhite() ? 1 : -1;
		}
	}
}
