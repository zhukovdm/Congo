using System.Collections.Immutable;

namespace Congo.Core
{
	public static class CongoEvaluator
	{
		// Shall ensure +/- Inf for all evaluation functions!
		public static int INF => 1_000_000;

		public static int WinLose(CongoGame game)
		{
			if (game.IsWin()) { return game.WhitePlayer.HasLion ? 1 : -1; }
			return 0;
		}

		// max. possible score = 5 + 10 + 12 + 100 + 10 + 7 + 7*3 = 165
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

		public static int Material(CongoGame game)
		{
			int score = 0;

			score += scoreByColor(White.Color, game.Board);
			score += scoreByColor(Black.Color, game.Board);

			if (!game.HasEnded() &&
				game.Predecessor.ActivePlayer.Color != game.ActivePlayer.Color &&
				game.Predecessor.ActivePlayer.InCheck(game.ActivePlayer.Moves)) {
				score += game.ActivePlayer.Color.IsWhite() ? 100 : -100;
			}

			return score;
		}
	}
}
