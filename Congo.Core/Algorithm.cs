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

		/// <summary>
		/// In original minimax algorithm, one of the players (white) 
		/// maximizes, another(black) minimizes.Score is calculated with 
		/// respect to both players, the value could be either high or low.
		/// For white player, the higher score is the better.On the
		/// contrary, for black the lower score is the better.
		///
		/// At a terminal node, active player cannot affect the position,
		/// it could only pass value upwards.That's why we look at the 
		/// opponent, because the opponent gets returned value. We return
		/// score for white and -score for black, because evaluator states,
		/// that higher value is better for white and lower is better
		/// for black.We then choose maximum from all such values from the 
		/// bottom and we pass *-score* further. The smaller *-score* the
		/// better for the opponent, so the player above cuts such branch
		/// via maximization.
		/// 
		/// Solution proposed above is, in my opinion, more intuitive.
		/// Sometimes another implementation is used.
		/// 
		/// In the list we may look at the *active player*. For instance,
		/// white gets higher score in the terminal node. Active player
		/// is black and by game.ActivePlayer.Color.IsWhite() ? -score,
		/// black inverts score. White gets value with opposite meaning,
		/// so we could choose Max(score, -negaMaxGetScore). We then return
		/// score instead of -score. The player above will also invert the
		/// meaning.
		/// </summary>
		private static int negaMaxGetScore(CongoGame game, int depth)
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
				score = Math.Max(score, negaMaxGetScore(newGame, depth - 1));
			}

			return -score;
		}

		private static CongoMove negaMaxSingleThread(CongoGame game, int depth)
		{
			int bestScore = int.MinValue;
			CongoGame bestFoundGame = null;

			// corner cases, final board or bad depth
			if (game.HasEnded() || depth == 0) { return null; }

			var moves = game.ActivePlayer.Moves;
			for (int i = 0; i < moves.Length; i++) {
				var newGame = game.Transition(moves[i]);
				var newScore = negaMaxGetScore(newGame, depth - 1);
				if (bestScore < newScore) {
					bestScore = newScore;
					bestFoundGame = newGame;
				}
			}

			return bestFoundGame?.TransitionMove;
		}

		public static CongoMove NegaMax(CongoGame game) => negaMaxSingleThread(game, 5);

		public static CongoMove IterDeep(CongoGame game) => Rnd(game);
	}
}
