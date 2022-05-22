﻿using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Congo.Core
{
    public delegate CongoMove AlgorithmDelegate(CongoGame game);

    /// <summary>
    /// Static class holds different algorithms for @b Move selection.
    /// </summary>
    public static class Algorithm
    {
        private enum State { OK, CANCEL, OMIT };

        private static volatile State state;

        static Algorithm() { state = State.OK; }

        #region Random

        private static readonly Random rnd = new Random();

        /// <summary>
        /// Rnd heuristic picks random move from all available.
        /// </summary>
        public static CongoMove Rnd(CongoGame game)
        {
            var upperBound = game.ActivePlayer.Moves.Length;
            return game.ActivePlayer.Moves[rnd.Next(upperBound)];
        }

        #endregion

        #region Negamax

        public static void Cancel()
            => state = State.CANCEL;

        public static void Omit()
            => state = State.OMIT;

        public static void Include()
            => state = State.OK;

        public static bool IsOmitted()
            => state == State.OMIT;

        private static readonly int negamaxDepth = 5; ///< Maximum distance from the initial node in the decision tree

        private static CongoHashTable hT;

        /// <summary>
        /// Picks pair with maximum out of two scores.
        /// </summary>
        private static (CongoMove, int) Max((CongoMove, int score) p1, (CongoMove, int score) p2)
            => p1.score >= p2.score ? p1 : p2;

        /// <summary>
        /// @b Negamax heuristic traverses all possible moves recursively
        /// and picks one with the highest reachable score. Score is obtained
        /// either by applying default evaluator to the leaf node of the
        /// decision tree or picking solution from the hash table if available.
        /// 
        /// @note To simplify source code, this method combines both, searching
        /// best score and returning best move. Constructs, such as (null, score),
        /// (_, score) and (move, _), are less transparent, but essential.
        /// </summary>
        private static (CongoMove, int) NegamaxSingleThread(ulong hash, CongoGame game,
            ImmutableArray<CongoMove> moves, int alpha, int beta, int depth)
        {
            CongoMove move = null;
            int score = -CongoEvaluator.INF;

            if (state != State.OK) { /* do nothing, cancel or invalidate */ }

            // recursion bottom
            else if (game.HasEnded() || depth <= 0) {
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
                for (int i = 0; i < moves.Length; ++i) { // lean iteration
                    var newMove = moves[i];
                    var newGame = game.Transition(newMove);

                    /* apply move on _OLD_ board -> get new hash */

                    // ordinary move
                    ulong newHash = CongoHashTable.ApplyMove(hash, game.Board, newMove);

                    // monkey jump also has piece Between
                    if (newMove is MonkeyJump jump) {
                        newHash = CongoHashTable.ApplyBetween(newHash, game.Board, jump);
                    }

                    /* Negamax recursive call */

                    /* Speculatively keep alpha-beta interval the same.
                     * Works for multiple monkey jump, when player color does
                     * not change. */
                    var newAlpha = alpha;
                    var newBeta  = beta;

                    /* Ordinary move is detected (opposite to multiple monkey 
                     * jump), because active player changes the color -> set
                     * new ab-interval */
                    if (newGame.ActivePlayer.Color != game.ActivePlayer.Color) {
                        newAlpha = -beta;
                        newBeta = -alpha;
                    }

                    (_, score) = Max((null, score), NegamaxSingleThread(newHash, newGame,
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

            var condition = (depth == negamaxDepth) ||
                (game.Predecessor.ActivePlayer.Color == game.ActivePlayer.Color);

            return condition ? (move, score) : (move, -score);
        }

        /// <summary>
        /// Multithreading based on Thread pool. Create a task with certain
        /// segment of possible moves and schedule it. Do/undo is not necessary
        /// due to the game immutability.
        /// </summary>
        private static (CongoMove, int) NegamaxMultiThread(ulong hash, CongoGame game, int depth)
        {
            var moves = game.ActivePlayer.Moves;
            (CongoMove, int) result = (null, -CongoEvaluator.INF);

            // avoid flooding the system
            var cpus = Math.Max(Environment.ProcessorCount - 2, 1);

            // adjust number of tasks, >= 1 move per task
            while (moves.Length / cpus == 0) { --cpus; }

            /* one move -> one thread */

            if (cpus == 1) {
                return NegamaxSingleThread(hash, game, game.ActivePlayer.Moves,
                    -CongoEvaluator.INF, CongoEvaluator.INF, depth);
            }

            /* otherwise, divide moves evenly */

            // current thread plans tasks and process remainder
            var div = moves.Length / (cpus - 1);
            var rem = moves.Length % (cpus - 1);

            // cpus > 1
            var taskPool = new Task<(CongoMove, int)>[cpus - 1];

            int from = 0;

            for (int i = 0; i < cpus - 1; ++i) {
                var arr = new CongoMove[div];
                moves.CopyTo(from, arr, 0, div);

                taskPool[i] = Task.Run(() => {
                    return NegamaxSingleThread(hash, game, arr.ToImmutableArray(),
                        -CongoEvaluator.INF, CongoEvaluator.INF, depth);
                });
                
                from += div;
            }

            if (rem > 0) {
                var arr = new CongoMove[rem];
                moves.CopyTo(from, arr, 0, rem);
                result = NegamaxSingleThread(hash, game, arr.ToImmutableArray(),
                    -CongoEvaluator.INF, CongoEvaluator.INF, depth);
            }

            foreach (var task in taskPool) { result = Max(result, task.Result); }

            return result;
        }

        public static CongoMove Negamax(CongoGame game)
        {
            if (state == State.OMIT) { return null; }

            if (state != State.OMIT) { state = State.OK; }

            /* negamax recursion bottom assumes game predecessor
             * and non-zero depth at first call */

            if (game.HasEnded() || negamaxDepth <= 0) { return null; }

            // forget previous table if the game is new
            if (game.IsNew() || hT == null) { hT = new CongoHashTable(); }

            var hash = CongoHashTable.InitHash(game.Board);

            var (move, _) = NegamaxMultiThread(hash, game, negamaxDepth);

            return move;
        }

        #endregion
    }
}
