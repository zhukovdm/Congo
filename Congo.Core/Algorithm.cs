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

		private static int negamaxGetScore(CongoGame game, int depth)
		{
			int score;

			if (game.HasEnded() || depth == 0) {
				score = Evaluator.Basic(game);
				return game.Opponent.Color.IsWhite() ? score : -score;
			}

			score = int.MinValue;

			var moves = game.ActivePlayer.Moves;
			for (int i = 0; i < moves.Length; i++) {
				var newGame = game.Transition(moves[i]);
				score = Math.Max(score, negamaxGetScore(newGame, depth - 1));
			}

			return -score;
		}

		private static (int, CongoMove) negamaxSingleThread(CongoGame game, int depth)
		{
			int bestScore = int.MinValue;
			CongoGame bestGame = null;

			// corner cases, final board or bad depth
			if (game.HasEnded() || depth == 0) { return (bestScore, null); }

			var moves = game.ActivePlayer.Moves;
			for (int i = 0; i < moves.Length; i++) {
				var newGame = game.Transition(moves[i]);
				var newScore = negamaxGetScore(newGame, depth - 1);
				if (bestScore < newScore) {
					bestScore = newScore;
					bestGame = newGame;
				}
			}

			return (bestScore, bestGame?.TransitionMove);
		}

		private static readonly int negamaxDepth = 3;
		
		public static CongoMove Negamax(CongoGame game)
		{
			var (_, move) = negamaxSingleThread(game, negamaxDepth);
			return move;
		}

		public static CongoMove IterDeep(CongoGame game) => Rnd(game);
	}
}
