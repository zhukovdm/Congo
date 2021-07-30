using System;

namespace Congo.Core
{
	class HashCell
	{
		public ulong Hash;
		public int Depth;
		public int Score;
		public CongoBoard Board;
	}

	class HashTable
	{
		private static ulong[] hashValues;
		private static int tableSize = 262_144; // 2^18

		private static int colorSpan = 2;
		private static int pieceSpan = CongoBoard.PieceSample.Length;
		private static int boardSpan = CongoBoard.Empty.Size * CongoBoard.Empty.Size;		

		private HashCell[] table;

		static HashTable()
		{
			hashValues = new ulong[colorSpan * pieceSpan * boardSpan];

			// 4x random 16b -> 1x 64b random word
			var rnd = new Random();
			for (int i = 0; i < hashValues.Length; i++) {
				for (int j = 0; j < 4; j++) {
					var s = rnd.Next(1 << 16);
					hashValues[i] = hashValues[i] | ((ulong)s << (16 * j));
				}
			}
		}

		public HashTable()
		{
			table = new HashCell[tableSize];

			for (int i = 0; i < tableSize; i++) {
				table[i] = new HashCell();
			}
		}

		public ulong GetHash(CongoBoard board)
		{
			ulong hash = 0;

			for (int square = 0; square < board.Size; square++) {
				WithSquare(hash, board, square);
			}

			return hash;
		}
		
		public ulong WithSquare(ulong hash, CongoBoard board, int square)
		{
			// ground and river are black by default
			var colorOffset = board.IsWhitePiece(square)
				? 0 * pieceSpan * boardSpan
				: 1 * pieceSpan * boardSpan;
			var pieceOffset = board.GetPiece(square).Code * boardSpan;
			var offset = colorOffset + pieceOffset + square;

			hash ^= hashValues[offset];

			return hash;
		}

		/// <summary>
		/// XOR undo by repeated application
		/// </summary>
		public ulong WithoutSquare(ulong hash, CongoBoard board, int square)
			=> WithSquare(hash, board, square);

		/// <summary>
		/// Hash comes directly from Negamax due to the efficient generation
		/// method (XOR do/undo).
		/// </summary>
		public bool TryGetScore(ulong hash, CongoBoard board, int depth, out int score)
		{
			var cell = table[hash & 0x03_FF_FF]; // hash ~ 64b, table ~ 18b

			lock (cell) {

				// cell content cannot be used
				if (hash != cell.Hash  || cell.Depth < depth       ||
					cell.Board == null || !board.Equals(cell.Board)) {
					score = 0;
					return false;
				}

				// similar or better solution is found
				else {
					score = cell.Score;
					return true;
				}
			}
		}

		public bool TrySetScore(int hash, CongoBoard board, int depth, int score)
		{
			var cell = table[hash];

			lock (cell) {

				/* != or null board in the cell      -> eviction
				 * better (deeper) solution is found -> eviction */
				if (!board.Equals(cell.Board) || cell.Depth < depth)
				{
					cell.Depth = depth;
					cell.Score = score;
					cell.Board = board;
					return true;
				}

				// cell solution is as good as candidate
				else {
					return false;
				}
			}
		}
	}
}
