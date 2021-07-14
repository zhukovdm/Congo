using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest {

	[TestClass]
	public class DecoderTest {

		[TestMethod]
		public void Decoder_DeBruijnLSB() {
			var result = true;
			var num = 0x1UL;
			for (int i = 0; i < 64; i++) {
				var immed = Decoder.DeBruijnLSB(num << i);
				result &= immed == i;
			}
			Assert.IsTrue(result);
		}

	}

}
