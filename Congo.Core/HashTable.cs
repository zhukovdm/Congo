using System;

namespace Congo.Core
{
	class HashCell
	{
		public int Score;
		public int Depth;
		public ulong Hash;
		public CongoMove Move;
		public CongoBoard Board;
	}

	class HashTable
	{
		private static readonly ulong[] hashValues;
		private static readonly int tableSize = 262_144; // 2^18
		private static readonly ulong mask = 0x03_FF_FF;

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

		/// <summary>
		/// Hash comes directly from Negamax due to the efficient generation
		/// method (XOR do/undo).
		/// </summary>
		public bool TryGetScore(ulong hash, CongoBoard board, int depth,
			out (int, CongoMove) pair)
		{
			var cell = table[hash & mask]; // hash ~ 64b, table ~ 18b

			lock (cell) {

				// cell content cannot be used
				if (hash != cell.Hash  || cell.Depth < depth        ||
					cell.Board == null || !board.Equals(cell.Board)) {
					pair = (-CongoEvaluator.INF, null);
					return false;
				}

				// similar or better solution is found
				else {
					pair = (cell.Score, cell.Move);
					return true;
				}
			}
		}

		private ulong addPiece(ulong hash, CongoPiece piece, CongoColor color, int square)
		{
			var colorOffset = color.IsWhite()
				? 0 * pieceSpan * boardSpan
				: 1 * pieceSpan * boardSpan;
			var pieceOffset = piece.Code * boardSpan;
			var offset = colorOffset + pieceOffset + square;

			hash ^= hashValues[offset];

			return hash;
		}

		/// <summary>
		/// XOR undo by repeated application
		/// </summary>
		private ulong removePiece(ulong hash, CongoPiece piece, CongoColor color, int square)
			=> addPiece(hash, piece, color, square);

		public ulong ApplyMove(ulong hash, CongoBoard board, CongoMove move)
		{
			// ground and river are considered black pieces, no difference

			var piece = board.GetPiece(move.To);
			var color = board.IsWhitePiece(move.To) ? White.Color : Black.Color;
			hash = removePiece(hash, piece, color, move.To);

			piece = board.GetPiece(move.Fr);
			color = board.IsWhitePiece(move.Fr) ? White.Color : Black.Color;
			hash = addPiece(hash, piece, color, move.To);
			hash = removePiece(hash, piece, color, move.Fr);

			board = board.Without(move.Fr);
			piece = board.GetPiece(move.Fr); // ground or river
			hash = addPiece(hash, piece, Black.Color, move.Fr);

			return hash;
		}

		public ulong ApplyMove(ulong hash, CongoBoard board, MonkeyJump jump)
		{
			hash = ApplyMove(hash, board, (CongoMove)jump);

			board = board.Without(jump.Bt);
			var piece = board.GetPiece(jump.Bt); // ground or river
			hash = addPiece(hash, piece, Black.Color, jump.Bt);

			return hash;
		}

		public bool TrySetScore(ulong hash, CongoBoard board, int depth,
			int score, CongoMove move)
		{
			var cell = table[hash & mask];

			lock (cell) {

				/* != or null board in the cell      -> eviction
				 * better (deeper) solution is found -> eviction */
				if (!board.Equals(cell.Board) || cell.Depth < depth)
				{
					cell.Score = score;
					cell.Depth = depth;
					cell.Hash  = hash;
					cell.Move  = move;
					cell.Board = board;
					return true;
				}

				// cell solution is as good as candidate
				else {
					return false;
				}
			}
		}

		public ulong GetHash(CongoBoard board)
		{
			ulong hash = 0;

			for (int square = 0; square < board.Size; square++) {
				var piece = board.GetPiece(square);
				var color = board.IsWhitePiece(square) ? White.Color : Black.Color;
				addPiece(hash, piece, color, square);
			}

			return hash;
		}

		public void ReportOccupancy()
		{
			var occupancy = 0;

			foreach (var cell in table) {
				if (cell.Board != null) { occupancy++; }
			}

			Console.WriteLine($"\n hash table occupancy {(double)occupancy / table.Length}");
		}
	}
}
