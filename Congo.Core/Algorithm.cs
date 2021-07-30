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
		 * searching best score and returning best move. That's why we use
		 * constructs such as (score, null), (score, _) or (_, move). */
		private static (int, CongoMove) negamaxSingleThread(ulong hash,
			CongoGame game, ImmutableArray<CongoMove> moves, int alpha,
			int beta, int depth)
		{
			int score = -CongoEvaluator.INF;
			CongoMove move = null;

			// recursion bottom
			if (game.HasEnded() || depth <= 0) {
				score = game.Predecessor.ActivePlayer.Color.IsWhite()
					? CongoEvaluator.Material(game)
					: -CongoEvaluator.Material(game);

				return (score, null); // predecessor knows moves[i]
			}

			// better (deeper) solution found
			else if (hT.TryGetScore(hash, game.Board, depth, out var pair)) {
				score = pair.Item1; move =  pair.Item2;
			}

			// otherwise, traverse moves
			else {
				for (int i = 0; i < moves.Length; i++) {

					var newGame = game.Transition(moves[i]);

					/* apply move using OLD board to find new hash */

					ulong newHash;
					
					if (moves[i] is MonkeyJump) {
						newHash = hT.ApplyMove(hash, game.Board, (MonkeyJump)moves[i]);
					} else {
						newHash = hT.ApplyMove(hash, game.Board, moves[i]);
					}

					/* negamax recursive call */

					// ordinary move, active player changes the color
					if (newGame.ActivePlayer.Color != game.ActivePlayer.Color) {
						(score, _) = Max((score, null), negamaxSingleThread(newHash,
							newGame, newGame.ActivePlayer.Moves, -beta, -alpha, depth - 1));
					}

					// multiple monkey jump, no color change
					else {
						(score, _) = Max((score, null), negamaxSingleThread(newHash,
							newGame, newGame.ActivePlayer.Moves, alpha, beta, depth - 1));
					}

					/* evaluate negamax results */

					// fail-hard beta cut-off
					if (score >= beta) { score = beta; break; }

					// alpha update
					if (score > alpha) { alpha = score; move = moves[i]; }
				}
			}

			_ = hT.TrySetScore(hash, game.Board, depth, score, move);

			/* root game (no predecessor) or monkey jump (predecessor's
			 * active player is of the same color) */

			var condition = depth == negamaxDepth ||
				game.Predecessor.ActivePlayer.Color == game.ActivePlayer.Color;

			return condition ? (score, move) : (-score, move);
		}

		private static (int, CongoMove) negamaxMultiThread(ulong hash,
			CongoGame game, int depth)
		{

			/* Multithreading based on Thread pool. Create a task with
			 * certain segment of possible moves and schedule it to thread 
			 * pool. Do/undo is not necessary due to the game immutability. */

			(int, CongoMove) result;
			var moves = game.ActivePlayer.Moves;

			// avoid flooding the system
			var cpus = Math.Max(Environment.ProcessorCount - 2, 1);

			// adjust number of tasks, >= 1 move per task
			while (moves.Length / cpus == 0) cpus--;

			/* one move -> one thread */

			if (cpus == 1) {
				return negamaxSingleThread(hash, game, game.ActivePlayer.Moves,
					-CongoEvaluator.INF, CongoEvaluator.INF, depth);
			}

			/* otherwise divide moves evenly */

			var div = moves.Length / cpus;
			var rem = moves.Length % cpus;

			// cpus > 1
			var taskPool = new Task<(int, CongoMove)>[cpus - 1];

			int from = 0;

			for (int i = 0; i < cpus - 1; i++) {

				// cut out segment
				var arr = new CongoMove[div];
				moves.CopyTo(from, arr, 0, div);

				// plan and store new task
				taskPool[i] = Task.Run(() => {
					return negamaxSingleThread(hash, game, arr.ToImmutableArray(),
						-CongoEvaluator.INF, CongoEvaluator.INF, depth);
				});

				from += div;
			}

			{
				var arr = new CongoMove[div + rem];
				moves.CopyTo(from, arr, 0, div + rem);
				result = negamaxSingleThread(hash, game, arr.ToImmutableArray(),
					-CongoEvaluator.INF, CongoEvaluator.INF, depth);
			}
			
			foreach (var task in taskPool) {
				task.Wait();
				result = Max(result, task.Result);
			}

			return result;
		}

		private static readonly int negamaxDepth = 5;

		private static HashTable hT;
		
		public static CongoMove Negamax(CongoGame game)
		{

			/* negamax recursion bottom assumes game predecessor
			 * and non-zero depth at first call */

			if (game.HasEnded() || negamaxDepth <= 0) { return null; }

			// forget previous table if the game is new
			if (game.IsNew()) { hT = new HashTable(); }

			var hash = hT.GetHash(game.Board);

			var (_, move) = negamaxMultiThread(hash, game, negamaxDepth);

			return move;
		}

		public static CongoMove IterDeep(CongoGame game)
			=> throw new NotImplementedException();
	}
}
