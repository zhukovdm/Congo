using System;
using System.Collections.Immutable;

namespace Congo.Core
{
	class CongoHashCell
	{
		public ulong Hash;
		public CongoBoard Board;
		public CongoMove Move;
		public int Depth;
		public int Score;
	}

	public class CongoHashTable
	{
		private static readonly int colorFactor = 2;
		private static readonly int pieceFactor = CongoBoard.PieceSample.Length;
		private static readonly int boardFactor = CongoBoard.Empty.Size * CongoBoard.Empty.Size;
		private static readonly int colorSpan = pieceFactor * boardFactor;

		/*
		 *                     hashValues Scheme
		 * 
		 *     2  colors  -> {White, Black}        -> {0, 1}
		 *     10 pieces  -> {Ground, ..., Monkey} -> {0, ..., 9}
		 *     49 squares -> {A7, ..., G1}         -> {0, ..., 48}
		 * 
		 * hashValues contains a randomly generated UInt64 value for each
		 * possible combination of (color, piece, square) and has length
		 * of 2 * 10 * 49 values.
		 */

		private static readonly ImmutableArray<ulong> hashValues;

		private static readonly int tableSize = 262_144; // 2^18
		private static readonly ulong mask = 0x03_FF_FFUL;

		static CongoHashTable()
		{
			var values = new ulong[colorFactor * pieceFactor * boardFactor];

			// 4x random words of len 16b -> 1x random word of len 64b
			var words =  4;
			var len   = 16;
			var rnd   = new Random();

			for (int i = 0; i < values.Length; i++) {
				for (int shift = 0; shift < words; shift++) {
					var s = rnd.Next(1 << len); // next from [0, ..., 2^16)
					values[i] = values[i] | ((ulong)s << (len * shift));
				}
			}

			hashValues = values.ToImmutableArray();
		}

		private static ulong addPiece(ulong hash, CongoPiece piece, CongoColor color, int square)
		{
			var offset = color.Code * colorSpan + (int)piece.Code * boardFactor + square;

			hash ^= hashValues[offset];

			return hash;
		}

		/// <summary>
		/// XOR undo is performed via repeated application.
		/// </summary>
		private static ulong removePiece(ulong hash, CongoPiece piece, CongoColor color, int square)
			=> addPiece(hash, piece, color, square);

		public static ulong ApplyMove(ulong hash, CongoBoard board, CongoMove move)
		{
			CongoPiece piece;
			CongoColor color;

			/* Here ground and river are considered black.
			 * No difference, because hash values are randomly generated. */

			// captured piece is removed from move.To
			piece = board.GetPiece(move.To);
			color = board.IsWhitePiece(move.To) ? White.Color : Black.Color;
			hash = removePiece(hash, piece, color, move.To);

			// moving piece is removed from move.Fr and added to move.To
			piece = board.GetPiece(move.Fr);
			color = board.IsWhitePiece(move.Fr) ? White.Color : Black.Color;
			hash = removePiece(hash, piece, color, move.Fr);
			hash = addPiece(hash, piece, color, move.To);

			// update board first to set new piece
			board = board.Without(move.Fr);

			// ground or river is added to move.Fr
			piece = board.GetPiece(move.Fr);
			hash = addPiece(hash, piece, Black.Color, move.Fr);

			return hash;
		}

		/// <summary>
		/// This method handles _ONLY_ jump.Between. To process .Fr and .To,
		/// use ApplyMove method.
		/// </summary>
		public static ulong ApplyBetween(ulong hash, CongoBoard board, MonkeyJump jump)
		{
			CongoPiece piece;
			CongoColor color;

			/* See note regarding ground and river color inside the body of
			 * ApplyMove method */

			// captured piece is removed from between
			piece = board.GetPiece(jump.Bt);
			color = board.IsWhitePiece(jump.Bt) ? White.Color : Black.Color;
			hash = removePiece(hash, piece, color, jump.Bt);

			// update board first to set new piece
			board = board.Without(jump.Bt);

			// ground or river is added to between
			piece = board.GetPiece(jump.Bt);
			hash = addPiece(hash, piece, Black.Color, jump.Bt);

			return hash;
		}

		public static ulong InitHash(CongoBoard board)
		{
			ulong hash = 0;

			for (int square = 0; square < board.Size; square++) {
				var piece = board.GetPiece(square);
				var color = board.IsWhitePiece(square)
					? White.Color : Black.Color;

				hash = addPiece(hash, piece, color, square);
			}

			return hash;
		}

		private CongoHashCell[] table;

		public CongoHashTable()
		{
			table = new CongoHashCell[tableSize];

			for (int i = 0; i < tableSize; i++) {
				table[i] = new CongoHashCell();
			}
		}

		/// <summary>
		/// Hash comes directly from Negamax.
		/// </summary>
		public bool TryGetSolution(ulong hash, CongoBoard board, int depth,
			out CongoMove move, out int score)
		{
			var cell = table[hash & mask]; // hash ~ 64b, table ~ 18b

			lock (cell) {

				// cell content cannot be used
				if (hash != cell.Hash  || cell.Depth < depth ||
					cell.Board == null || board != cell.Board) {
					move = null; score = -CongoEvaluator.INF;
					return false;
				}

				// similar or better solution is found
				else {
					move = cell.Move; score = cell.Score;
					return true;
				}
			}
		}

		/// <summary>
		/// Returns void, does not matter if solution is indeed set.
		/// </summary>
		public void SetSolution(ulong hash, CongoBoard board, int depth,
			CongoMove move, int score)
		{
			var cell = table[hash & mask];

			lock (cell) {

				/* != or null board in the cell -> eviction
				 * better solution is found     -> eviction */

				if (board != cell.Board || cell.Depth < depth)
				{
					cell.Hash = hash;
					cell.Board = board;
					cell.Depth = depth;
					cell.Move = move;
					cell.Score = score;
				}
			}
		}

		public void ReportOccupancy()
		{
			int occupancy = 0;

			foreach (var cell in table) {
				if (cell.Board != null) { occupancy++; }
			}

			Console.WriteLine($" hash table occupancy {(double)occupancy / tableSize}");
		}
	}
}
