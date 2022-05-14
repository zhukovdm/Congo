using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
    [TestClass]
    public class Color_SupportingMethods_Test
    {
        [TestMethod]
        public void EqualBothNull()
        {
            CongoColor c1 = null;
            CongoColor c2 = null;
            Assert.IsTrue(c1 == c2);
        }

        [TestMethod]
        public void NotEqualLeftNull()
        {
            CongoColor c1 = null;
            CongoColor c2 = Black.Color;
            Assert.IsTrue(c1 != c2);
        }

        [TestMethod]
        public void NotEqualRightNull()
        {
            CongoColor c1 = White.Color;
            CongoColor c2 = null;
            Assert.IsTrue(c1 != c2);
        }

        [TestMethod]
        public void NotEqualBothNonNull()
        {
            CongoColor c1 = White.Color;
            CongoColor c2 = Black.Color;
            Assert.IsTrue(c1 != c2);
        }

        [TestMethod]
        public void EqualBothNonNull()
        {
            CongoColor c1 = White.Color;
            CongoColor c2 = White.Color;
            Assert.IsTrue(c1 == c2);
        }
    }
}
