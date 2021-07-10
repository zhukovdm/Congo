using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest {

	[TestClass]
	public class PlayerTest {

		[TestMethod]
		public void Player_DeBruijnLSB() {
			var result = true;
			var num = 0x1UL;
			for (int i = 0; i < 64; i++) {
				var immed = CongoPlayer.DeBruijnLSB(num << i);
				result &= immed == i;
			}
			Assert.IsTrue(result);
		}

	}

}
