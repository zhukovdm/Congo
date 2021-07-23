using System.Collections;
using System.Collections.Generic;

namespace Congo.Core
{
	public class CongoMove
	{
		public static int Compare(CongoMove x, CongoMove y)
		{
			/* lexicographic order */
			if ((x.Fr < y.Fr) || (x.Fr == y.Fr && x.To < y.To)) {
				return -1;
			} else if (x.Fr == y.Fr && x.To == y.To) {
				return 0;
			} else {
				return 1;
			}
		}

		public static bool AreEqual(CongoMove m1, CongoMove m2)
			=> Compare(m1, m2) == 0;

		public readonly int Fr, To;

		public CongoMove(int fr, int to) { Fr = fr; To = to; }
	}

	public class MonkeyJump : CongoMove
	{
		public readonly int Bt;

		public MonkeyJump(int fr, int bt, int to) :base(fr, to) { Bt = bt; }
	}

	public class CongoMoveObjComparer : IComparer
	{
		public int Compare(object x, object y) /* not type-safe */
			=> CongoMove.Compare((CongoMove)x, (CongoMove)y);
	}

	public class CongoMoveGenComparer : IComparer<CongoMove>
	{
		public int Compare(CongoMove x, CongoMove y)
			=> CongoMove.Compare(x, y);
	}
}
