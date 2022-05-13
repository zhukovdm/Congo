using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Congo.Core
{
    public static class Algorithm
    {
        #region Random

        private static readonly Random rnd = new Random();

        /// <summary>
        /// Rnd heuristic picks random move from available.
        /// </summary>
        public static CongoMove Rnd(CongoGame game)
        {
            var upperBound = game.ActivePlayer.Moves.Length;
            return game.ActivePlayer.Moves[rnd.Next(upperBound)];
        }

        #endregion

        #region Negamax

        /// <summary>
        /// Pick maximum out of two scores.
        /// </summary>
        private static (CongoMove, int) Max((CongoMove, int score) p1, (CongoMove, int score) p2)
            => p1.score >= p2.score ? p1 : p2;

        /// <summary>
        /// Negamax heuristic traverses all possible moves recursively and
        /// picks one with the highest reachable score. Score is obtained
        /// either by applying default evaluator to the terminal or picking
        /// solution from hash table if available.
        /// 
        /// To simplify source code, this method combines both, searching best
        /// score and returning best move. Constructs, such as (null, score),
        /// (_, score) and (move, _), are less transparent, but essential.
        /// </summary>
        private static (CongoMove, int) negamaxSingleThread(ulong hash, CongoGame game,
            ImmutableArray<CongoMove> moves, int alpha, int beta, int depth)
        {
            CongoMove move = null;
            int score = -CongoEvaluator.INF;

            // recursion bottom
            if (game.HasEnded() || depth <= 0) {
                score = game.Predecessor.ActivePlayer.Color.IsWhite()
                    ? CongoEvaluator.Default(game)
                    : -CongoEvaluator.Default(game);

                return (null, score); // predecessor knows transition move
            }

            // similar or better solution is found
            else if (hT.TryGetSolution(hash, game.Board, depth, out var tMove, out var tScore)) {
                move = tMove; score = tScore;
            }

            // otherwise, traverse moves
            else {
                for (int i = 0; i < moves.Length; i++) { // lean iteration
                    var newMove = moves[i];
                    var newGame = game.Transition(newMove);

                    /* apply move on _OLD_ board -> get new hash */

                    // ordinary move
                    ulong newHash = CongoHashTable.ApplyMove(hash, game.Board, newMove);

                    // monkey jump also has piece Between
                    if (newMove is MonkeyJump) {
                        newHash = CongoHashTable.ApplyBetween(newHash, game.Board, (MonkeyJump)newMove);
                    }

                    /* negamax recursive call */

                    // multiple monkey jump, no color change
                    var newAlpha = alpha;
                    var newBeta  = beta;

                    // ordinary move, active player changes the color
                    if (newGame.ActivePlayer.Color != game.ActivePlayer.Color) {
                        newAlpha = -beta;
                        newBeta = -alpha;
                    }

                    (_, score) = Max((null, score), negamaxSingleThread(newHash, newGame,
                        newGame.ActivePlayer.Moves, newAlpha, newBeta, depth - 1));

                    /* evaluate negamax results */

                    // fail-hard beta cut-off
                    if (score >= beta) { score = beta; break; }

                    // alpha update
                    if (score > alpha) { move = newMove; alpha = score; }
                }

                hT.SetSolution(hash, game.Board, depth, move, score);
            }

            /* Root game (no predecessor) or monkey jump (predecessor's active
             * player and current active player are of the same color) ->
             * no score inversion. */

            var condition = depth == negamaxDepth ||
                game.Predecessor.ActivePlayer.Color == game.ActivePlayer.Color;

            return condition ? (move, score) : (move, -score);
        }

        /// <summary>
        /// Multithreading based on Thread pool. Create a task with certain
        /// segment of possible moves and schedule it. Do/undo is not necessary
        /// due to the game immutability.
        /// </summary>
        private static (CongoMove, int) negamaxMultiThread(ulong hash, CongoGame game, int depth)
        {
            var moves = game.ActivePlayer.Moves;
            (CongoMove, int) result = (null, -CongoEvaluator.INF);
            
            // avoid flooding the system
            var cpus = Math.Max(Environment.ProcessorCount - 2, 1);

            // adjust number of tasks, >= 1 move per task
            while (moves.Length / cpus == 0) cpus--;

            /* one move -> one thread */

            if (cpus == 1) {
                return negamaxSingleThread(hash, game, game.ActivePlayer.Moves,
                    -CongoEvaluator.INF, CongoEvaluator.INF, depth);
            }

            /* otherwise, divide moves evenly */

            // current thread plans tasks and process remainder
            var div = moves.Length / (cpus - 1);
            var rem = moves.Length % (cpus - 1);

            // cpus > 1
            var taskPool = new Task<(CongoMove, int)>[cpus - 1];

            int from = 0;

            for (int i = 0; i < cpus - 1; i++) {
                var arr = new CongoMove[div];
                moves.CopyTo(from, arr, 0, div);

                taskPool[i] = Task.Run(() => {
                    return negamaxSingleThread(hash, game, arr.ToImmutableArray(),
                        -CongoEvaluator.INF, CongoEvaluator.INF, depth);
                });
                
                from += div;
            }

            if (rem > 0) {
                var arr = new CongoMove[rem];
                moves.CopyTo(from, arr, 0, rem);
                result = negamaxSingleThread(hash, game, arr.ToImmutableArray(),
                    -CongoEvaluator.INF, CongoEvaluator.INF, depth);
            }
            
            foreach (var task in taskPool) { result = Max(result, task.Result); }

            return result;
        }

        private static readonly int negamaxDepth = 5;

        private static CongoHashTable hT;
        
        public static CongoMove Negamax(CongoGame game)
        {
            /* negamax recursion bottom assumes game predecessor
             * and non-zero depth at first call */

            if (game.HasEnded() || negamaxDepth <= 0) { return null; }

            // forget previous table if the game is new
            if (game.IsNew() || hT == null) { hT = new CongoHashTable(); }

            var hash = CongoHashTable.InitHash(game.Board);

            var (move, _) = negamaxMultiThread(hash, game, negamaxDepth);

            return move;
        }

        #endregion

        #region Iterative deepening

        public static CongoMove IterDeep(CongoGame game)
            => throw new NotImplementedException();

        #endregion
    }
}
