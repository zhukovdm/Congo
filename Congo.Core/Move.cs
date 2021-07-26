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

		public static bool AreEqual(CongoMove x, CongoMove y)
			=> Compare(x, y) == 0;

		public readonly int Fr, To;

		public CongoMove(int fr, int to) { Fr = fr; To = to; }

		/// <summary>
		/// This method is used only for testing purposes!
		/// </summary>
		public override string ToString() => (Square)Fr + ", " + (Square)To;
	}

	public class MonkeyJump : CongoMove
	{
		public readonly int Bt;

		public MonkeyJump(int fr, int bt, int to) :base(fr, to) { Bt = bt; }

		/// <summary>
		/// This method is used only for testing purposes!
		/// </summary>
		public override string ToString() => (Square)Fr + ", " + (Square)Bt + ", " + (Square)To;
	}

	/// <summary>
	/// This comparer is implemented due to MSTest framework limitations.
	/// Do not use it anywhere else!
	/// </summary>
	public class CongoMoveObjComparer : IComparer
	{
		public int Compare(object x, object y) /* not type-safe! */
			=> CongoMove.Compare((CongoMove)x, (CongoMove)y);
	}

	public class CongoMoveGenComparer : IComparer<CongoMove>
	{
		public int Compare(CongoMove x, CongoMove y)
			=> CongoMove.Compare(x, y);
	}
}
