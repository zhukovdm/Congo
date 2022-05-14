using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest {

    [TestClass]
    public class BitScanTest {

        [TestMethod]
        public void DeBruijnLsb_FindBitSingle()
        {
            Assert.AreEqual(7, BitScan.DeBruijnLsb(0x00_00_00_00_00_00_00_80UL));
        }

        [TestMethod]
        public void DeBruijnLsb_FindBitSequence() {
            var result = true;
            var num = 0x1UL;

            for (int i = 0; i < 64; ++i) {
                var immed = BitScan.DeBruijnLsb(num << i);
                result &= immed == i;
            }

            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class BitScanEnumeratorTest
    {
        [TestMethod]
        public void EmptyIdxSequence()
        {
            var expected = new List<int>();
            var actual = new List<int>();

            var e = new BitScanEnumerator(0x0UL);
            while (e.MoveNext()) { actual.Add(e.Current); }

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SparseIdxSequence()
        {
            var expected = new List<int> { 3, 7, 11, 15, 19, 23, 27, 31 };
            var actual = new List<int>();

            var e = new BitScanEnumerator(0x88_88_88_88UL);
            while (e.MoveNext()) { actual.Add(e.Current); }

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FullIdxSequence()
        {
            var expected = new List<int>();
            var actual = new List<int>();

            for (int i = 0; i < 64; ++i) { expected.Add(i); }

            var e = new BitScanEnumerator(0xFF_FF_FF_FF_FF_FF_FF_FFUL);
            while (e.MoveNext()) { actual.Add(e.Current); }

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
