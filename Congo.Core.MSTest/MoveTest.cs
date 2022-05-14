using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{

    [TestClass]
    public class Move_Test
    {

        [TestMethod]
        public void StaticComparer_LessThanByFr()
        {
            var m1 = new CongoMove((int)Square.A7, (int)Square.C7);
            var m2 = new CongoMove((int)Square.B7, (int)Square.B7);
            var actual = CongoMove.Compare(m1, m2); // a7 < b7 & c7 > b7
            int expected = -1;
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void StaticComparer_LessThanByTo()
        {
            var m1 = new CongoMove((int)Square.A7, (int)Square.B7);
            var m2 = new CongoMove((int)Square.A7, (int)Square.C7);
            var actual = CongoMove.Compare(m1, m2); // a7 == a7 & b7 < c7
            int expected = -1;
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void StaticComparer_Equals()
        {
            var m1 = new CongoMove((int)Square.A7, (int)Square.B7);
            var m2 = new CongoMove((int)Square.A7, (int)Square.B7);
            var actual = CongoMove.Compare(m1, m2);
            int expected = 0;
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void StaticComparer_GreaterThanByFr()
        {
            var m1 = new CongoMove((int)Square.B7, (int)Square.B7);
            var m2 = new CongoMove((int)Square.A7, (int)Square.C7);
            var actual = CongoMove.Compare(m1, m2); // b7 > a7 & b7 < c7
            int expected = 1;
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void StaticComparer_GreaterThanByTo()
        {
            var m1 = new CongoMove((int)Square.A7, (int)Square.E4);
            var m2 = new CongoMove((int)Square.A7, (int)Square.D4);
            var actual = CongoMove.Compare(m1, m2); // a7 == a7 & d5 > d4
            int expected = 1;
            Assert.IsTrue(expected == actual);
        }
    }
}
