using System;
using System.Collections.Immutable;

namespace Congo.Core
{
    /// <summary>
    /// Supporting structure for representing CongoHashTable cell with entities.
    /// </summary>
    class CongoHashCell
    {
        public ulong Hash;
        public CongoBoard Board;
        public CongoMove Move;
        public int Depth;
        public int Score;
    }

    /// <summary>
    /// Thread-safe hash table for storing intermediate results of the search
    /// through game decision tree. Extensively used by <b>Algorithm.Negamax</b>.
    /// </summary>
    public class CongoHashTable
    {
        private static readonly int colorFactor = 2;
        private static readonly int pieceFactor = CongoBoard.PieceSample.Length;
        private static readonly int boardFactor = CongoBoard.Size * CongoBoard.Size;
        private static readonly int colorSpan = pieceFactor * boardFactor;

        /// <summary>
        ///                    hashValues Scheme
        ///
        ///    2  colors  -> {White, Black}        -> {0, 1}
        ///    10 pieces  -> {Ground, ..., Monkey} -> {0, ..., 9}
        ///    49 squares -> {A7, ..., G1}         -> {0, ..., 48}
        ///
        /// hashValues contains a randomly generated UInt64 value for each
        /// possible combination of <b>(color, piece, square)</b> and has
        /// length of 2 * 10 * 49 = 980 values.
        /// </summary>
        private static readonly ImmutableArray<ulong> hashValues;

        private static readonly int tableSize = 262_144; // 2^18 positions

        private static readonly ulong mask = 0x03_FF_FFUL; // 18 bottom bits are set

        static CongoHashTable()
        {
            var values = new ulong[colorFactor * pieceFactor * boardFactor];

            // 4x random words of len 16b -> 1x random word of len 64b
            var words =  4;
            var len   = 16;
            var rnd   = new Random();

            for (int i = 0; i < values.Length; ++i) {
                for (int shift = 0; shift < words; ++shift) {
                    var next = rnd.Next(1 << len); // next from [0, ..., 2^16)
                    values[i] = values[i] | ((ulong)next << (len * shift));
                }
            }

            hashValues = values.ToImmutableArray();
        }

        /// <summary>
        /// Add (~ XOR-in) triple (color, piece, square) into input @b hash
        /// number.
        /// </summary>
        private static ulong AddPiece(ulong hash, CongoPiece piece, CongoColor color, int square)
        {
            var offset = color.Code * colorSpan + (int)piece.Code * boardFactor + square;

            hash ^= hashValues[offset];

            return hash;
        }

        /// <summary>
        /// Remove (~ XOR-out) triple (color, piece, square) from input @b hash
        /// number via repeated addition application.
        /// </summary>
        private static ulong RemovePiece(ulong hash, CongoPiece piece, CongoColor color, int square)
            => AddPiece(hash, piece, color, square);

        /// <summary>
        /// Apply standard move from one square to another. Monkey jump shall
        /// additionally call ApplyBetween().
        /// </summary>
        public static ulong ApplyMove(ulong hash, CongoBoard board, CongoMove move)
        {
            CongoPiece piece;
            CongoColor color;

            /* Ground and river are considered black.
             * No difference, because hash values are random. */

            // piece (captured animal, ground or river) is removed from move.To
            piece = board.GetPiece(move.To);
            color = board.IsWhitePiece(move.To)
                ? White.Color
                : Black.Color;
            hash = RemovePiece(hash, piece, color, move.To);

            // moving piece is removed from move.Fr and added to move.To
            piece = board.GetPiece(move.Fr);
            color = board.IsWhitePiece(move.Fr)
                ? White.Color
                : Black.Color;
            hash = RemovePiece(hash, piece, color, move.Fr);
            hash = AddPiece(hash, piece, color, move.To);

            // update board first to set ground or river
            board = board.Without(move.Fr);

            // ground or river is added to move.Fr
            piece = board.GetPiece(move.Fr);
            hash = AddPiece(hash, piece, Black.Color, move.Fr);

            return hash;
        }

        /// <summary>
        /// This method handles @b ONLY jump.Between. To process .Fr and .To,
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
            color = board.IsWhitePiece(jump.Bt)
                ? White.Color
                : Black.Color;
            hash = RemovePiece(hash, piece, color, jump.Bt);

            // update board first to set ground or river
            board = board.Without(jump.Bt);

            // ground or river is added to between
            piece = board.GetPiece(jump.Bt);
            hash = AddPiece(hash, piece, Black.Color, jump.Bt);

            return hash;
        }

        /// <summary>
        /// Initialize hash with pieces placed on input board.
        /// </summary>
        public static ulong InitHash(CongoBoard board)
        {
            ulong hash = 0;

            for (int square = 0; square < CongoBoard.Size; ++square) {
                var piece = board.GetPiece(square);
                var color = board.IsWhitePiece(square)
                    ? White.Color
                    : Black.Color;
                hash = AddPiece(hash, piece, color, square);
            }

            return hash;
        }

        private readonly CongoHashCell[] table;

        public CongoHashTable()
        {
            table = new CongoHashCell[tableSize];

            for (int i = 0; i < tableSize; ++i) {
                table[i] = new CongoHashCell();
            }
        }

        /// <summary>
        /// Thread-safe access to the hash table. Hashes come directly from
        /// Negamax decision tree.
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
        /// Thread-safe set new solution. Return void, because no difference
        /// if solution is indeed set.
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
    }
}
