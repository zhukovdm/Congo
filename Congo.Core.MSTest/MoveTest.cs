using Microsoft.VisualStudio.TestTools.UnitTesting;

using Congo.Def;

namespace Congo.Core.MSTest {

	[TestClass]
	public class Move_Test {

		[TestMethod]
		public void StaticComparer_LessThanByFr() {
			var m1 = new CongoMove((int)SquareCode.A7, (int)SquareCode.C7);
			var m2 = new CongoMove((int)SquareCode.B7, (int)SquareCode.B7);
			var actual = CongoMove.Comparer(m1, m2); // A7 < B7 & C7 > B7
			int expected = -1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_LessThanByTo() {
			var m1 = new CongoMove((int)SquareCode.A7, (int)SquareCode.B7);
			var m2 = new CongoMove((int)SquareCode.A7, (int)SquareCode.C7);
			var actual = CongoMove.Comparer(m1, m2); // A7 == A7 & B7 < C7
			int expected = -1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_Equals() {
			var m1 = new CongoMove((int)SquareCode.A7, (int)SquareCode.B7);
			var m2 = new CongoMove((int)SquareCode.A7, (int)SquareCode.B7);
			var actual = CongoMove.Comparer(m1, m2);
			int expected = 0;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_GreaterThanByFr() {
			var m1 = new CongoMove((int)SquareCode.B7, (int)SquareCode.B7);
			var m2 = new CongoMove((int)SquareCode.A7, (int)SquareCode.C7);
			var actual = CongoMove.Comparer(m1, m2); // B7 > A7 & B7 < C7
			int expected = 1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_GreaterThanByTo() {
			var m1 = new CongoMove((int)SquareCode.A7, (int)SquareCode.E4);
			var m2 = new CongoMove((int)SquareCode.A7, (int)SquareCode.D4);
			var actual = CongoMove.Comparer(m1, m2); // A7 == A7 & D5 > D4
			int expected = 1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void ToStringOverride() {
			var m = new CongoMove((int)SquareCode.B5, (int)SquareCode.G1);
			var actual = m.ToString();
			var expected = "B5, G1";
			Assert.AreEqual(expected, actual);
		}

	}
}
