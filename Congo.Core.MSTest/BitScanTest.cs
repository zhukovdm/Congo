using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest {

	[TestClass]
	public class BitScanTest {

		[TestMethod]
		public void BitScan_DeBruijnLsb() {
			var result = true;
			var num = 0x1UL;
			for (int i = 0; i < 64; i++) {
				var immed = BitScan.DeBruijnLsb(num << i);
				result &= immed == i;
			}
			Assert.IsTrue(result);
		}

	}

}
