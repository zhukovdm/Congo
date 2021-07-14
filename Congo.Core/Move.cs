using System.Collections;
using System.Collections.Generic;

namespace Congo.Core {

	public struct CongoMove {

		// lexicographic order
		public static int Comparer(CongoMove x, CongoMove y) {
			if ((x.Fr < y.Fr) || (x.Fr == y.Fr && x.To < y.To)) {
				return -1;
			}
			else if (x.Fr == y.Fr && x.To == y.To) {
				return 0;
			}
			else {
				return 1;
			}
		}

		public readonly int Fr, To;
		public CongoMove(int fr, int to) { Fr = fr; To = to; }
	}

	public class CongoMoveComparer : IComparer {

		// Not type safe due to MSTest framework limitations
		public int Compare(object x, object y) 
			=> CongoMove.Comparer((CongoMove)x, (CongoMove)y);

	}

	public class CongoMoveComparerGeneric : IComparer<CongoMove> {

		public int Compare(CongoMove x, CongoMove y)
			=> CongoMove.Comparer(x, y);

	}

}
