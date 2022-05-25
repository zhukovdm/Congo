using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Congo.Core
{
    /// <summary>
    /// This class defines minimalistic game board containing piece occupation
    /// by the color and piece kinds.
    /// </summary>
    public sealed class CongoBoard : IParametrizedEnumerable<CongoColor, int>
    {
        private static readonly int size = 7;

        private static readonly CongoBoard empty = new(
            0, 0, new[] { 0U, 0U, 0U, 0x1111111U, 0U, 0U, 0U }.ToImmutableArray()
        );

        /// <summary>
        /// The order is critical! 4-bit indexing due to a better performance.
        /// </summary>
        private static readonly ImmutableArray<CongoPiece> pieceSample = new CongoPiece[] {
            Ground.Piece, River.Piece, Elephant.Piece, Zebra.Piece, Giraffe.Piece,
            Crocodile.Piece, Pawn.Piece, Superpawn.Piece, Lion.Piece, Monkey.Piece
        }.ToImmutableArray();

        /// <summary>
        /// Answers true if bit on a given position is set.
        /// </summary>
        private static bool GetBit(ulong word, int position)
            => ((word >> position) & 0x1UL) == 0x1UL;

        /// <summary>
        /// Set or unset bit on a given position.
        /// </summary>
        private static ulong SetBitToValue(ulong current, int position, bool value)
            => value ? current | (0x1UL << position) : current & ~(0x1UL << position);

        private static bool IsJungleImpl(int rank, int file)
            => rank >= 0 && rank < size && file >= 0 && file < size;

        private static bool IsJungleImpl(int square)
            => square >= 0 && square < size * size;

        private static bool IsAboveRiverImpl(int square) => square / size < size / 2;

        private static bool IsRiverImpl(int square) => square / size == size / 2;

        private static bool IsBelowRiverImpl(int square) => square / size > size / 2;

        /// <summary>
        /// Rank is a uint word from piece storage, it contains 4b piece codes.
        /// </summary>
        private static uint SetPieceCode(uint rank, CongoPiece piece, int file)
        {
            var shift = file * 4;
            return (rank & ~(0xFU << shift)) | piece.Code << shift;
        }

        public static CongoBoard Empty => empty;

        public static ImmutableArray<CongoPiece> PieceSample => pieceSample;

        /// <summary>
        /// Compare two boards by looking at occupation and pieces on the board.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CongoBoard b1, CongoBoard b2)
        {
            if (b1 is null && b2 is null) { return true; }
            if (b1 is null || b2 is null) { return false; }

            var result = true;

            result &= b1.whiteOccupied == b2.whiteOccupied;
            result &= b1.blackOccupied == b2.blackOccupied;

            for (int i = 0; i < size; i++) {
                result &= b1.pieces[i] == b2.pieces[i];
            }

            return result;
        }

        public static bool operator !=(CongoBoard b1, CongoBoard b2) => !(b1 == b2);

        #region Leap generation

        private delegate ImmutableArray<int> LeapGenerator(int rank, int file);

        private delegate ImmutableArray<int> ColoredLeapGenerator(
            CongoColor color, int rank, int file);

        private static void AddLeap(List<int> leaps, int rank, int file)
        {
            if (IsJungleImpl(rank, file)) { leaps.Add(rank * size + file); }
        }

        /// <summary>
        /// Main leap generation primitive. Generate circle and selects leaps
        /// based on predicate.
        /// </summary>
        private static ImmutableArray<int> CircleLeapGenerator(
            Func<int, int, bool> predicate, int rank, int file, int radius)
        {
            var leaps = new List<int>();

            for (int i = -radius; i < radius + 1; i++) {
                for (int j = -radius; j < radius + 1; j++) {
                    if (predicate.Invoke(i, j)) { AddLeap(leaps, rank + i, file + j); }
                }
            }

            return leaps.ToImmutableArray();
        }

        private static ImmutableArray<int> KingLeapGenerator(int rank, int file)
            => CircleLeapGenerator(
                (int i, int j) => i != 0 || j != 0,
                rank, file, 1);

        private static ImmutableArray<int> KnightLeapGenerator(int rank, int file)
            => CircleLeapGenerator(
                (int i, int j) => Math.Abs(i) + Math.Abs(j) == 3,
                rank, file, 2);

        private static ImmutableArray<int> ElephantLeapGenerator(int rank, int file)
            => CircleLeapGenerator(
                (int i, int j) => (Math.Abs(i) == 1 && j == 0) || (i == 0 && Math.Abs(j) == 1) ||
                                  (Math.Abs(i) == 2 && j == 0) || (i == 0 && Math.Abs(j) == 2),
                rank, file, 2);

        /// <summary>
        /// The method consider only capturing moves, non-capturing are dynamic
        /// and calculated on demand.
        /// </summary>
        private static ImmutableArray<int> CapturingGiraffeLeapGenerator(int rank, int file)
            => CircleLeapGenerator(
                (int i, int j) => (Math.Abs(i) == 2 && (j == 0 || Math.Abs(j) == 2)) ||
                                  (Math.Abs(j) == 2 && (i == 0 || Math.Abs(i) == 2)),
                rank, file, 2);

        /// <summary>
        /// Generate special circles with missing positions for slides. Slides
        /// are generated on demand, because of their dynamicity.
        /// 
        /// . x x x . . . | . . . . . . .
        /// . x c x . . . | . . . . . . .
        /// . x . x . . . | . x x x . x x
        /// . . . . . . . | . . c . . . c
        /// . . . . . . . | . x x x . x x
        /// . . . x . x . | . . . . . . .
        /// . . . x c x . | . . . . . . .
        /// </summary>
        private static ImmutableArray<int> CrocodileLeapGenerator(int rank, int file)
        {
            var square = rank * size + file;
            var direction = IsAboveRiverImpl(square) ? 1 : -1;

#pragma warning disable IDE0039 // Use local function
            Func<int, int, bool> riverPredicate =
                (int i, int j) => i != 0;

            Func<int, int, bool> groundPredicate =
                (int i, int j) => (i != 0 || j != 0) && (i != direction || j != 0);
#pragma warning restore IDE0039 // Use local function

            var predicate = IsRiverImpl(rank * size + file)
                ? riverPredicate
                : groundPredicate;

            return CircleLeapGenerator(predicate, rank, file, 1);
        }

        private static readonly ImmutableHashSet<int> whiteLionCastle = new HashSet<int> {
            (int)Square.C3, (int)Square.D3, (int)Square.E3,
            (int)Square.C2, (int)Square.D2, (int)Square.E2,
            (int)Square.C1, (int)Square.D1, (int)Square.E1
        }.ToImmutableHashSet();

        private static readonly ImmutableHashSet<int> blackLionCastle = new HashSet<int> {
            (int)Square.C7, (int)Square.D7, (int)Square.E7,
            (int)Square.C6, (int)Square.D6, (int)Square.E6,
            (int)Square.C5, (int)Square.D5, (int)Square.E5
        }.ToImmutableHashSet();

        /// <summary>
        /// Contains only ordinary moves within lion's castle. Special
        /// lion-lion captures are generated on demand.
        /// </summary>
        private static ImmutableArray<int> LionLeapGenerator(CongoColor color, int rank, int file)
        {
            var leaps = new List<int>();
            var castle = color.IsWhite() ? whiteLionCastle : blackLionCastle;

            if (castle.Contains(rank * size + file)) {
                var possibleLeaps = KingLeapGenerator(rank, file);
                foreach (var leap in possibleLeaps) {
                    if (castle.Contains(leap)) { leaps.Add(leap); }
                }
            }

            return leaps.ToImmutableArray();
        }

        /// <summary>
        /// Generates only forward pawn leaps, backward sliding is generated
        /// on demand.
        /// 
        /// . . x x x . .
        /// . . . p . . .
        /// . . . . . . .
        /// </summary>
        private static ImmutableArray<int> PawnLeapGenerator(CongoColor color, int rank, int file)
        {
            var direction = color.IsBlack() ? 1 : -1;

            return CircleLeapGenerator(
                (int i, int j) => i == direction,
                rank, file, 1);
        }

        /// <summary>
        /// Generates only forward and side superpawn leaps, backward sliding
        /// is generated on demand.
        /// 
        /// . . x x x . .
        /// . . x s x . .
        /// . . . . . . .
        /// </summary>
        private static ImmutableArray<int> SuperpawnLeapGenerator(CongoColor color, int rank, int file)
        {
            var direction = color.IsBlack() ? 1 : -1;

            return CircleLeapGenerator(
                (int i, int j) => (i == direction) || (i == 0 && j != 0),
                rank, file, 1);
        }

        private static ImmutableArray<ImmutableArray<int>> PrecalculateLeaps(LeapGenerator gen)
        {
            var allLeaps = new ImmutableArray<int>[size * size];

            for (int rank = 0; rank < size; rank++) {
                for (int file = 0; file < size; file++) {
                    allLeaps[rank * size + file] = gen.Invoke(rank, file);
                }
            }

            return allLeaps.ToImmutableArray();
        }

        private static ImmutableArray<ImmutableArray<int>> PrecalculateLeaps(
            ColoredLeapGenerator gen, CongoColor color)
        {
            var allLeaps = new ImmutableArray<int>[size * size];

            for (int rank = 0; rank < size; rank++) {
                for (int file = 0; file < size; file++) {
                    allLeaps[rank * size + file] = gen.Invoke(color, rank, file);
                }
            }

            return allLeaps.ToImmutableArray();
        }

        private static readonly ImmutableArray<ImmutableArray<int>> kingLeaps,
            knightLeaps, elephantLeaps, capturingGiraffeLeaps, crocodileLeaps,
            whiteLionLeaps, blackLionLeaps, whitePawnLeaps, blackPawnLeaps,
            whiteSuperpawnLeaps, blackSuperpawnLeaps;

        #endregion

        static CongoBoard()
        {
            // color-independent leaps
            kingLeaps = PrecalculateLeaps(KingLeapGenerator);
            knightLeaps = PrecalculateLeaps(KnightLeapGenerator);
            elephantLeaps = PrecalculateLeaps(ElephantLeapGenerator);
            capturingGiraffeLeaps = PrecalculateLeaps(CapturingGiraffeLeapGenerator);
            crocodileLeaps = PrecalculateLeaps(CrocodileLeapGenerator);

            // color-dependent leaps
            whiteLionLeaps = PrecalculateLeaps(LionLeapGenerator, White.Color);
            blackLionLeaps = PrecalculateLeaps(LionLeapGenerator, Black.Color);
            whitePawnLeaps = PrecalculateLeaps(PawnLeapGenerator, White.Color);
            blackPawnLeaps = PrecalculateLeaps(PawnLeapGenerator, Black.Color);
            whiteSuperpawnLeaps = PrecalculateLeaps(SuperpawnLeapGenerator, White.Color);
            blackSuperpawnLeaps = PrecalculateLeaps(SuperpawnLeapGenerator, Black.Color);
        }

        private readonly ulong whiteOccupied; ///< 64-bit long word accommodates 49 statuses
        private readonly ulong blackOccupied; ///< 64-bit long word accommodates 49 statuses
        private readonly ImmutableArray<uint> pieces; ///< 32-bit long word accommodates 7 piece codes of 4bit length

        private CongoBoard(ulong whiteOccupied, ulong blackOccupied, ImmutableArray<uint> pieces)
        {
            this.whiteOccupied = whiteOccupied;
            this.blackOccupied = blackOccupied;
            this.pieces = pieces;
        }

        private uint GetPieceCode(int square)
            => (pieces[square / size] >> (square % size * 4)) & 0xFU;

        public int Size => size;

        public bool IsJungle(int rank, int file) => IsJungleImpl(rank, file);

        public bool IsJungle(int square) => IsJungleImpl(square);

        public bool IsAboveRiver(int square) => IsAboveRiverImpl(square);

        public bool IsRiver(int square) => IsRiverImpl(square);

        public bool IsBelowRiver(int square) => IsBelowRiverImpl(square);

        /// <summary>
        /// True if the top or bottom side of a position is a board border.
        /// </summary>
        public bool IsUpDownBorder(CongoColor color, int square)
        {
            return color.IsWhite()
                ? square >= (int)Square.A7 && square <= (int)Square.G7
                : square >= (int)Square.A1 && square <= (int)Square.G1;
        }

        public bool IsOccupied(int square) => GetBit(whiteOccupied | blackOccupied, square);

        public bool IsWhitePiece(int square) => GetBit(whiteOccupied, square);

        public bool IsBlackPiece(int square) => GetBit(blackOccupied, square);

        public bool IsFirstMovePiece(int square) => IsWhitePiece(square);

        public bool IsFriendlyPiece(CongoColor color, int square)
            => GetBit(color.IsWhite() ? whiteOccupied : blackOccupied, square);

        public bool IsOpponentPiece(CongoColor color, int square)
            => GetBit(color.IsWhite() ? blackOccupied : whiteOccupied, square);

        public CongoPiece GetPiece(int square)
            => pieceSample[(int)GetPieceCode(square)];

        /// <summary>
        /// Creates new board with certain piece of a color on a position.
        /// </summary>
        public CongoBoard With(CongoColor color, CongoPiece piece, int square)
        {
            var rank  = square / size;
            var shift = square % size * 4;
            var newWhiteOccupied = SetBitToValue(whiteOccupied, square, color.IsWhite());
            var newBlackOccupied = SetBitToValue(blackOccupied, square, color.IsBlack());

            return new CongoBoard(newWhiteOccupied, newBlackOccupied,
                pieces.SetItem(rank, (pieces[rank] & ~(0xFU << shift)) | (piece.Code << shift)));
        }

        /// <summary>
        /// Creates new board with removed piece on a position.
        /// </summary>
        public CongoBoard Without(int square)
        {
            var newWhiteOccupied = SetBitToValue(whiteOccupied, square, false);
            var newBlackOccupied = SetBitToValue(blackOccupied, square, false);
            var newPiece = IsRiverImpl(square) ? River.Piece : Ground.Piece;
            var rank = square / size;

            return new CongoBoard(newWhiteOccupied, newBlackOccupied,
                pieces.SetItem(rank, SetPieceCode(pieces[rank], newPiece, square % size)));
        }

        public ImmutableArray<int> LeapsAsKing(int square)
            => kingLeaps[square];

        public ImmutableArray<int> LeapsAsKnight(int square)
            => knightLeaps[square];

        public ImmutableArray<int> LeapsAsElephant(int square)
            => elephantLeaps[square];

        public ImmutableArray<int> LeapsAsCapturingGiraffe(int square)
            => capturingGiraffeLeaps[square];

        public ImmutableArray<int> LeapsAsCrocodile(int square)
            => crocodileLeaps[square];

        public ImmutableArray<int> LeapsAsLion(CongoColor color, int square)
            => color.IsWhite() ? whiteLionLeaps[square] : blackLionLeaps[square];

        public ImmutableArray<int> LeapsAsPawn(CongoColor color, int square)
            => color.IsWhite() ? whitePawnLeaps[square] : blackPawnLeaps[square];

        public ImmutableArray<int> LeapsAsSuperpawn(CongoColor color, int square)
            => color.IsWhite() ? whiteSuperpawnLeaps[square] : blackSuperpawnLeaps[square];

        /// <summary>
        /// Checks if diagonal lion-lion capture is possible. Example is shown
        /// below. '?' means an arbitrary animal could keep lion from jumping.
        /// 
        ///          . . . . l . .
        /// river -> . . . ? . . .
        ///          . . l . . . .
        /// </summary>
        public bool TryDiagonalJump(CongoColor color, int square, out int target)
        {
            var freePath = !IsOccupied((int)Square.D4);

            if (GetPiece(square).IsLion()) {
                switch (square) {
                    case (int)Square.C3:
                        target = (int)Square.E5;
                        return color.IsWhite() && freePath && GetPiece(target).IsLion();
                    case (int)Square.E3:
                        target = (int)Square.C5;
                        return color.IsWhite() && freePath && GetPiece(target).IsLion();
                    case (int)Square.C5:
                        target = (int)Square.E3;
                        return color.IsBlack() && freePath && GetPiece(target).IsLion();
                    case (int)Square.E5:
                        target = (int)Square.C3;
                        return color.IsBlack() && freePath && GetPiece(target).IsLion();
                    default:
                        break;
                }
            }

            target = -1;
            return false;
        }

        /// <summary>
        /// Lion is detected in its castle. Lion is outside of its castle
        /// after lion-lion capture.
        /// 
        /// @note Don't remember why detecting lion in its castle is important.
        ///     Probably this is because of the unit tests. In theory, it is
        ///     possible to place lion into a wrong castle.
        /// </summary>
        public bool IsLionCastle(CongoColor color, int square)
        {
            var castle = color.IsWhite()
                ? whiteLionCastle
                : blackLionCastle;
            return GetPiece(square).IsLion() && castle.Contains(square);
        }

        /// <summary>
        /// Enumerator versioning (of a container copy) is not mandatory due
        /// to the board immutability.
        /// </summary>
        public IParametrizedEnumerator<int> GetEnumerator(CongoColor color)
            => new BitScanEnumerator(color.IsWhite() ? whiteOccupied : blackOccupied);

        /// <summary>
        /// Do nothing, necessary due to the operator== and operator!=.
        /// </summary>
        public override bool Equals(object obj) => this == (CongoBoard)obj;

        /// <summary>
        /// Do nothing, necessary due to the operator== and operator!=.
        /// </summary>
        public override int GetHashCode() => base.GetHashCode();
    }
}
