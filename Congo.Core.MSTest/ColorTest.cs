using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
	[TestClass]
	public class Color_SupportingMethods_Test
	{
		[TestMethod]
		public void Color_EqualBothNull()
		{
			CongoColor c1 = null;
			CongoColor c2 = null;
			Assert.IsTrue(c1 == c2);
		}

		[TestMethod]
		public void Color_NotEqualLeftNull()
		{
			CongoColor c1 = null;
			CongoColor c2 = Black.Color;
			Assert.IsTrue(c1 != c2);
		}

		[TestMethod]
		public void Color_NotEqualRightNull()
		{
			CongoColor c1 = White.Color;
			CongoColor c2 = null;
			Assert.IsTrue(c1 != c2);
		}

		[TestMethod]
		public void Color_NotEqualBothNonNull()
		{
			CongoColor c1 = White.Color;
			CongoColor c2 = Black.Color;
			Assert.IsTrue(c1 != c2);
		}

		[TestMethod]
		public void Color_EqualBothNonNull()
		{
			CongoColor c1 = White.Color;
			CongoColor c2 = White.Color;
			Assert.IsTrue(c1 == c2);
		}
	}
}
