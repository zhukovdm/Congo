using System.Collections.Immutable;

namespace Congo.Core
{
	public static class CongoEvaluator
	{
		// Shall ensure +/- Inf for all evaluation functions!
		public static int INF => 1_000_000;

		/// <summary>
		/// Ternary evaluation either win, lose or 0.
		/// </summary>
		public static int WinLose(CongoGame game)
		{
			if (game.IsWin()) { return game.WhitePlayer.HasLion ? 1 : -1; }
			return 0;
		}

		/// <summary>
		/// Max. possible score = 5 + 10 + 12 + 100 + 10 + 7 + 7*3 = 165
		/// </summary>
		private static readonly ImmutableArray<int> materialValues = new int[10] {
			0,   // ground
			0,   // river
			6,   // elephant
			7,   // zebra
			5,   // giraffe
			10,  // crocodile
			1,   // pawn
			3,   // superpawn
			100, // lion
			10   // monkey
		}.ToImmutableArray();

		private static int scoreByColor(CongoColor color, CongoBoard board)
		{
			int score = 0;
			var e = board.GetEnumerator(color);

			while(e.MoveNext()) {
				score += materialValues[(int)board.GetPiece(e.Current).Code];
			}

			return color.IsWhite() ? score : -score;
		}

		/// <summary>
		/// Count score based on material values of each piece on the board.
		/// </summary>
		public static int Material(CongoGame game)
		{
			int score = 0;

			score += scoreByColor(White.Color, game.Board);
			score += scoreByColor(Black.Color, game.Board);

			if (!game.HasEnded() &&
				game.Predecessor.ActivePlayer.Color != game.ActivePlayer.Color &&
				game.Predecessor.ActivePlayer.LionInDanger(game.ActivePlayer.Moves)) {
				score += game.ActivePlayer.Color.IsWhite() ? 100 : -100;
			}

			return score;
		}

		/// <summary>
		/// Default evaluation, all algorithms should use this method.
		/// </summary>
		public static int Default(CongoGame game) => Material(game);
	}
}
