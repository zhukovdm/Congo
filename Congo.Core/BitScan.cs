using System.Collections.Immutable;

namespace Congo.Core {

	public static class BitScan {

		// credit: https://www.chessprogramming.org/BitScan

		private static readonly ulong magicNumber = 0x03F79D71B4CB0A89;
		private static readonly ImmutableArray<int> magicHash = new int[] {
			 0,  1, 48,  2, 57, 49, 28,  3,
			61, 58, 50, 42, 38, 29, 17,  4,
			62, 55, 59, 36, 53, 51, 43, 22,
			45, 39, 33, 30, 24, 18, 12,  5,
			63, 47, 56, 27, 60, 41, 37, 16,
			54, 35, 52, 21, 44, 32, 23, 11,
			46, 26, 40, 15, 34, 20, 31, 10,
			25, 14, 19,  9, 13,  8,  7,  6
		}.ToImmutableArray();

		public static int DeBruijnLsb(ulong word) {
			return magicHash[(int)((word * magicNumber) >> 58)];
		}

	}

	// data structure versioning is not mandatory due to immutability
	public class BitScanEnumerator : IParametrizedEnumerator<int>
	{
		private ulong occupancy;

		public BitScanEnumerator(ulong occupancy)
		{
			this.occupancy = occupancy;
			Current = -1;
		}

		public int Current { get; private set; }

		public bool MoveNext()
		{
			if (occupancy == 0) return false;
			var lsb = occupancy & (ulong.MaxValue - occupancy + 1);
			Current = BitScan.DeBruijnLsb(lsb);
			occupancy &= ~lsb;

			return true;
		}
	}
}
