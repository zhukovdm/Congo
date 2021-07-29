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

		private static (int, CongoMove) Max((int, CongoMove) pairA, (int, CongoMove) pairB)
			=> pairA.Item1 >= pairB.Item1 ? pairA : pairB;

		/* In order to simplify source code, this method combines both,
		 * searching best score and returning best move. This is the reason 
		 * why we return null or pass null to the call, or ignore returned 
		 * move. */
		private static (int, CongoMove) negamaxSingleThread(CongoGame game,
			ImmutableArray<CongoMove> moves, int alpha, int beta, int depth)
		{
			int score = -Evaluator.INF;
			CongoMove move = null;

			// recursion bottom
			if (game.HasEnded() || depth <= 0) {
				score = game.Predecessor.ActivePlayer.Color.IsWhite()
					? Evaluator.Material(game)
					: -Evaluator.Material(game);

				return (score, null); // predecessor knows moves[i]
			}

			// 
			for (int i = 0; i < moves.Length; i++) {
				var newGame = game.Transition(moves[i]);

				// ordinary move, active player changes the color
				if (newGame.ActivePlayer.Color != game.ActivePlayer.Color) {
					(score, _) = Max((score, null), negamaxSingleThread(
						newGame, newGame.ActivePlayer.Moves, -beta, -alpha, depth - 1));
				}

				// multiple monkey jump, no color change
				else {
					(score, _) = Max((score, null), negamaxSingleThread(
						newGame, newGame.ActivePlayer.Moves, alpha, beta, depth - 1));
				}

				// fail-hard beta cut-off
				if (score >= beta) { score = beta; break; }

				// alpha update
				if (score > alpha) { alpha = score; move = moves[i]; }
			}

			/* root game (no predecessor) or monkey jump (predecessor's
			 * active player is of the same color) */

			var condition = depth == negamaxDepth ||
				game.Predecessor.ActivePlayer.Color == game.ActivePlayer.Color;

			return condition ? (score, move) : (-score, move);
		}



		private static (int, CongoMove) negamaxMultiThread(CongoGame game, int depth)
		{

			/* Multi-threading is based on Thread pool. We create a task
			 * with certain and plan it to the threading pool. The total
			 * amount of tasks are chosen based on actual processor count.
			 * This is rather easy task due to board immutability. Undo
			 * moves are not necessary.
			 */

			(int, CongoMove) result;
			var moves = game.ActivePlayer.Moves;

			// avoid flooding the system
			var cpus = Math.Max(Environment.ProcessorCount - 2, 1);

			// adjust number of tasks, >= 1 move per task
			while (moves.Length / cpus == 0) cpus--;

			/* one move -> one thread */

			if (cpus == 1) {
				return negamaxSingleThread(game, game.ActivePlayer.Moves,
					-Evaluator.INF, Evaluator.INF, negamaxDepth);
			}

			/* otherwise divide moves evenly */

			var div = moves.Length / cpus;
			var rem = moves.Length % cpus;

			// cpus > 1
			var taskPool = new Task<(int, CongoMove)>[cpus - 1];

			int from = 0;

			for (int i = 0; i < cpus - 1; i++) {

				// prepare segment
				var arr = new CongoMove[div];
				moves.CopyTo(from, arr, 0, div);

				// plan and store new task
				taskPool[i] = Task.Run(() => {
					return negamaxSingleThread(game, arr.ToImmutableArray(),
						-Evaluator.INF, Evaluator.INF, negamaxDepth);
				});

				from += div;
			}

			{
				var arr = new CongoMove[div + rem];
				moves.CopyTo(from, arr, 0, div + rem);
				result = negamaxSingleThread(game, arr.ToImmutableArray(),
					-Evaluator.INF, Evaluator.INF, negamaxDepth);
			}
			
			foreach (var task in taskPool) {
				task.Wait();
				result = Max(result, task.Result);
			}

			return result;
		}

		private static readonly int negamaxDepth = 5;
		
		public static CongoMove Negamax(CongoGame game)
		{

			/* negamax recursion bottom assumes game predecessor
			 * and non-zero depth at first call */

			if (game.HasEnded() || negamaxDepth <= 0) { return null; }

			var (_, move) = negamaxMultiThread(game, negamaxDepth);
			
			return move;
		}

		public static CongoMove IterDeep(CongoGame game) => Rnd(game);
	}
}
