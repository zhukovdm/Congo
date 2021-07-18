using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
	public enum HeuristicCode
	{
		Random, MiniMax, NegaMax, IterDeep
	}

	public static class Algorithm
	{
		private delegate CongoMove D(CongoGame game);

		private static Random rnd = new Random();

		private static ImmutableDictionary<HeuristicCode, D> heuristics =
			new Dictionary<HeuristicCode, D> {
				{ HeuristicCode.Random,   new D(RandomChoise) },
				{ HeuristicCode.MiniMax,  new D(MiniMax)      },
				{ HeuristicCode.NegaMax,  new D(NegaMax)      },
				{ HeuristicCode.IterDeep, new D(IterDeep)	  }
			}.ToImmutableDictionary();
		

		private static CongoMove RandomChoise(CongoGame game)
		{
			var upperBound = game.ActivePlayer.Moves.Length;
			return game.ActivePlayer.Moves[rnd.Next(upperBound)];
		}

		private static CongoMove MiniMax(CongoGame game)
		{
			return RandomChoise(game);
		}

		private static CongoMove NegaMax(CongoGame game)
		{
			return RandomChoise(game);
		}

		private static CongoMove IterDeep(CongoGame game)
		{
			return RandomChoise(game);
		}

		public static CongoMove GetMove(CongoGame game, HeuristicCode heuristic)
			=> heuristics[heuristic].Invoke(game);
	}
}
