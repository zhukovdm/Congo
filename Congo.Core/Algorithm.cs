using System;

namespace Congo.Core
{
	public static class Algorithm
	{
		private static Random rnd = new Random();

		public static CongoMove Rnd(CongoGame game)
		{
			var upperBound = game.ActivePlayer.Moves.Length;
			return game.ActivePlayer.Moves[rnd.Next(upperBound)];
		}

		public static CongoMove MiniMax(CongoGame game)
		{
			return Rnd(game);
		}

		public static CongoMove NegaMax(CongoGame game)
		{
			return Rnd(game);
		}

		public static CongoMove IterDeep(CongoGame game)
		{
			return Rnd(game);
		}
	}
}
