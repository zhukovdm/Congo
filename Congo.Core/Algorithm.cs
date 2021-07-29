using System;
using System.Threading.Tasks;

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

		private static int negamaxGetScore(CongoGame game, int alpha, int beta, int depth)
		{
			int score = -Evaluator.INF;

			if (game.HasEnded() || depth == 0) {
				score = Evaluator.Material(game);
				return game.Opponent.Color.IsWhite() ? score : -score;
			}

			var moves = game.ActivePlayer.Moves;
			for (int i = 0; i < moves.Length; i++) {
				var newGame = game.Transition(moves[i]);
				score = Math.Max(score, negamaxGetScore(newGame, -beta, -alpha, depth - 1));

				// fail-hard beta cut-off, no opponent's alpha update (--beta = alpha)
				if (score >= beta) { return -beta; }

				// alpha update
				if (score > alpha) { alpha = score; }
			}

			// the better for the active player -> the worser for its opponent
			return -score;
		}

		private static (int, CongoMove) negamaxSingleThread(CongoGame game, int depth)
		{
			int bestScore = -Evaluator.INF; // also alpha!
			int beta = Evaluator.INF;
			CongoGame bestGame = null;
			
			// corner cases, final board or bad depth
			if (game.HasEnded() || depth == 0) { return (bestScore, null); }

			var moves = game.ActivePlayer.Moves;
			for (int i = 0; i < moves.Length; i++) {
				var newGame = game.Transition(moves[i]);
				var newScore = negamaxGetScore(newGame, -beta, -bestScore, depth - 1);

				// newScore cannot exceed beta = +Inf, no cut-off

				if (bestScore < newScore) {
					bestScore = newScore;
					bestGame = newGame;
				}
			}

			return (bestScore, bestGame?.TransitionMove);
		}

		private static CongoMove negamaxMultiThread(CongoGame game, int depth)
		{
			return null;
		}

		private static readonly int negamaxDepth = 5;
		
		public static CongoMove Negamax(CongoGame game)
		{
			var (_, move) = negamaxSingleThread(game, negamaxDepth);
			return move;
		}

		public static CongoMove IterDeep(CongoGame game) => Rnd(game);
	}
}
