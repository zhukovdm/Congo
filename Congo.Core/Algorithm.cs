using System;
using System.Collections.Immutable;
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

		private struct Eval
		{
			public readonly int Score;
			public readonly CongoMove Move;

			public Eval(int score, CongoMove move) { Score = score; Move = move; }

			public Eval With(int score) => new Eval(score, Move);
		}

		private static Eval Max(Eval e1, Eval e2) => e1.Score >= e2.Score ? e1 : e2;

		/*
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
		*/

		private static Eval negamaxSingleThread(CongoGame game,
			ImmutableArray<CongoMove> moves, int alpha, int beta, int depth)
		{
			var eval = new Eval(-Evaluator.INF, null);
			
			// recursion bottom
			if (game.HasEnded() || depth <= 0) {
				var score = game.Predecessor.ActivePlayer.Color.IsWhite()
					? Evaluator.Material(game)
					: -Evaluator.Material(game);

				return new Eval(score, game.TransitionMove);
			}

			for (int i = 0; i < moves.Length; i++) {
				var newGame = game.Transition(moves[i]);

				// ordinary move, active player changes the color
				if (newGame.ActivePlayer.Color != game.ActivePlayer.Color) {
					eval = Max(eval, negamaxSingleThread(
						newGame, newGame.ActivePlayer.Moves, -beta, -alpha, depth - 1));
				}

				// multiple monkey jump, no color change
				else {
					eval = Max(eval, negamaxSingleThread(
						newGame, newGame.ActivePlayer.Moves, alpha, beta, depth - 1));
				}

				// fail-hard beta cut-off
				if (eval.Score >= beta) { eval = eval.With(beta); break; }

				// alpha update
				if (eval.Score > alpha) { alpha = eval.Score; }
			}

			// root game or monkey jump
			var condition = depth == negamaxDepth ||
				game.Predecessor.ActivePlayer.Color == game.ActivePlayer.Color;

			return condition ? eval : eval.With(-eval.Score);
		}

		private static CongoMove negamaxMultiThread(CongoGame game, int depth)
		{
			return null;
		}

		private static readonly int negamaxDepth = 1;
		
		public static CongoMove Negamax(CongoGame game)
		{

			/* negamax recursion bottom assumes game predecessor
			 * and non-zero depth at first call */

			if (game.HasEnded() || negamaxDepth <= 0) { return null; }

			var eval = negamaxSingleThread(game, game.ActivePlayer.Moves,
				-Evaluator.INF, Evaluator.INF, negamaxDepth);
			
			return eval.Move;
		}

		public static CongoMove IterDeep(CongoGame game) => Rnd(game);
	}
}
