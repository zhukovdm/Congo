﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest {

	[TestClass]
	public class Move_Test {

		[TestMethod]
		public void StaticComparer_LessThanByFr() {
			var m1 = new CongoMove((int)SquareCode.a7, (int)SquareCode.c7);
			var m2 = new CongoMove((int)SquareCode.b7, (int)SquareCode.b7);
			var actual = CongoMove.Compare(m1, m2); // a7 < b7 & c7 > b7
			int expected = -1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_LessThanByTo() {
			var m1 = new CongoMove((int)SquareCode.a7, (int)SquareCode.b7);
			var m2 = new CongoMove((int)SquareCode.a7, (int)SquareCode.c7);
			var actual = CongoMove.Compare(m1, m2); // a7 == a7 & b7 < c7
			int expected = -1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_Equals() {
			var m1 = new CongoMove((int)SquareCode.a7, (int)SquareCode.b7);
			var m2 = new CongoMove((int)SquareCode.a7, (int)SquareCode.b7);
			var actual = CongoMove.Compare(m1, m2);
			int expected = 0;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_GreaterThanByFr() {
			var m1 = new CongoMove((int)SquareCode.b7, (int)SquareCode.b7);
			var m2 = new CongoMove((int)SquareCode.a7, (int)SquareCode.c7);
			var actual = CongoMove.Compare(m1, m2); // b7 > a7 & b7 < c7
			int expected = 1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void StaticComparer_GreaterThanByTo() {
			var m1 = new CongoMove((int)SquareCode.a7, (int)SquareCode.e4);
			var m2 = new CongoMove((int)SquareCode.a7, (int)SquareCode.d4);
			var actual = CongoMove.Compare(m1, m2); // a7 == a7 & d5 > d4
			int expected = 1;
			Assert.IsTrue(expected == actual);
		}

		[TestMethod]
		public void ToStringOverride() {
			var m = new CongoMove((int)SquareCode.b5, (int)SquareCode.g1);
			var actual = m.ToString();
			var expected = "b5, g1";
			Assert.AreEqual(expected, actual);
		}

	}
}
