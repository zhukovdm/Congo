using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Congo.Core
{
    public interface IParametrizedEnumerator<T>
    {
        T Current { get; }
        bool MoveNext();
    }

    public interface IParametrizedEnumerable<T, U>
    {
        IParametrizedEnumerator<U> GetEnumerator(T param);
    }

    public sealed class CongoBoard : IParametrizedEnumerable<CongoColor, int>
    {
        private static readonly int size = 7;

        private static readonly CongoBoard empty = new CongoBoard(
            0, 0, new[] { 0U, 0U, 0U, 0x1111111U, 0U, 0U, 0U }.ToImmutableArray()
        );

        /// <summary>
        /// The order is critical! 4-bit indexing due to a better performance.
        /// </summary>
        private static readonly ImmutableArray<CongoPiece> pieceSample = new CongoPiece[] {
            Ground.Piece, River.Piece, Elephant.Piece, Zebra.Piece, Giraffe.Piece,
            Crocodile.Piece, Pawn.Piece, Superpawn.Piece, Lion.Piece, Monkey.Piece
        }.ToImmutableArray();

        private static bool getBit(ulong word, int position)
            => ((word >> position) & 0x1UL) == 0x1UL;

        private static ulong setBitToValue(ulong current, int position, bool value)
            => value ? current | (0x1UL << position) : current & ~(0x1UL << position);

        private static bool isJungle(int rank, int file)
            => rank >= 0 && rank < size && file >= 0 && file < size;

        private static bool isJungle(int square)
            => square >= 0 && square < size * size;

        private static bool isAboveRiver(int square) => square / size < size / 2;

        private static bool isRiver(int square) => square / size == size / 2;

        private static bool isBelowRiver(int square) => square / size > size / 2;

        /// <summary>
        /// Rank is a uint word from piece storage, it contains 4b piece codes.
        /// </summary>
        private static uint setPieceCode(uint rank, CongoPiece piece, int file)
        {
            var shift = file * 4;
            return (rank & ~(0xFU << shift)) | piece.Code << shift;
        }

        public static CongoBoard Empty => empty;

        public static ImmutableArray<CongoPiece> PieceSample => pieceSample;

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

        private static void addLeap(List<int> leaps, int rank, int file)
        {
            if (isJungle(rank, file)) leaps.Add(rank * size + file);
        }

        private static ImmutableArray<int> circleLeapGenerator(
            Func<int, int, bool> predicate, int rank, int file, int radius)
        {
            var leaps = new List<int>();

            for (int i = -radius; i < radius + 1; i++) {
                for (int j = -radius; j < radius + 1; j++) {
                    if (predicate.Invoke(i, j)) addLeap(leaps, rank + i, file + j);
                }
            }

            return leaps.ToImmutableArray();
        }

        private static ImmutableArray<int> kingLeapGenerator(int rank, int file)
            => circleLeapGenerator(
                (int i, int j) => i != 0 || j != 0,
                rank, file, 1);
        
        private static ImmutableArray<int> knightLeapGenerator(int rank, int file)
            => circleLeapGenerator(
                (int i, int j) => Math.Abs(i) + Math.Abs(j) == 3,
                rank, file, 2);

        private static ImmutableArray<int> elephantLeapGenerator(int rank, int file)
            => circleLeapGenerator(
                (int i, int j) => (Math.Abs(i) == 1 && j == 0) || (i == 0 && Math.Abs(j) == 1) ||
                                  (Math.Abs(i) == 2 && j == 0) || (i == 0 && Math.Abs(j) == 2),
                rank, file, 2);

        private static ImmutableArray<int> capturingGiraffeLeapGenerator(int rank, int file)
            => circleLeapGenerator(
                (int i, int j) => (Math.Abs(i) == 2 && (j == 0 || Math.Abs(j) == 2)) ||
                                  (Math.Abs(j) == 2 && (i == 0 || Math.Abs(i) == 2)),
                rank, file, 2);

        private static ImmutableArray<int> crocodileLeapGenerator(int rank, int file)
        {
            var square = rank * size + file;
            var direction = isAboveRiver(square) ? 1 : -1;
            
            Func<int, int, bool> riverPredicate =
                (int i, int j) => i != 0;
            
            Func<int, int, bool> groundPredicate =
                (int i, int j) => (i != 0 || j != 0) && (i != direction || j != 0);
            
            Func<int, int, bool> predicate = isRiver(rank * size + file) ?
                riverPredicate : groundPredicate;

            return circleLeapGenerator(predicate, rank, file, 1);
        }

        private static ImmutableHashSet<int> whiteLionCastle = new HashSet<int> {
            (int)Square.C3, (int)Square.D3, (int)Square.E3,
            (int)Square.C2, (int)Square.D2, (int)Square.E2,
            (int)Square.C1, (int)Square.D1, (int)Square.E1
        }.ToImmutableHashSet();

        private static ImmutableHashSet<int> blackLionCastle = new HashSet<int> {
            (int)Square.C7, (int)Square.D7, (int)Square.E7,
            (int)Square.C6, (int)Square.D6, (int)Square.E6,
            (int)Square.C5, (int)Square.D5, (int)Square.E5
        }.ToImmutableHashSet();

        private static ImmutableArray<int> lionLeapGenerator(CongoColor color, int rank, int file)
        {
            var leaps = new List<int>();
            var castle = color.IsWhite() ? whiteLionCastle : blackLionCastle;

            if (castle.Contains(rank * size + file)) {
                var possibleLeaps = kingLeapGenerator(rank, file);
                foreach (var leap in possibleLeaps) {
                    if (castle.Contains(leap)) leaps.Add(leap);
                }
            }

            return leaps.ToImmutableArray();
        }

        private static ImmutableArray<int> pawnLeapGenerator(CongoColor color, int rank, int file)
        {
            var direction = color.IsBlack() ? 1 : -1;

            return circleLeapGenerator(
                (int i, int j) => i == direction,
                rank, file, 1);
        }

        private static ImmutableArray<int> superpawnLeapGenerator(CongoColor color, int rank, int file)
        {
            var direction = color.IsBlack() ? 1 : -1;

            return circleLeapGenerator(
                (int i, int j) => (i == direction) || (i == 0 && j != 0),
                rank, file, 1);
        }

        private static ImmutableArray<ImmutableArray<int>> precalculateLeaps(LeapGenerator gen)
        {
            var allLeaps = new ImmutableArray<int>[size * size];

            for (int rank = 0; rank < size; rank++) {
                for (int file = 0; file < size; file++) {
                    allLeaps[rank * size + file] = gen.Invoke(rank, file);
                }
            }

            return allLeaps.ToImmutableArray();
        }

        private static ImmutableArray<ImmutableArray<int>> precalculateLeaps(
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
            kingLeaps = precalculateLeaps(kingLeapGenerator);
            knightLeaps = precalculateLeaps(knightLeapGenerator);
            elephantLeaps = precalculateLeaps(elephantLeapGenerator);
            capturingGiraffeLeaps = precalculateLeaps(capturingGiraffeLeapGenerator);
            crocodileLeaps = precalculateLeaps(crocodileLeapGenerator);

            // color-dependent leaps
            whiteLionLeaps = precalculateLeaps(lionLeapGenerator, White.Color);
            blackLionLeaps = precalculateLeaps(lionLeapGenerator, Black.Color);
            whitePawnLeaps = precalculateLeaps(pawnLeapGenerator, White.Color);
            blackPawnLeaps = precalculateLeaps(pawnLeapGenerator, Black.Color);
            whiteSuperpawnLeaps = precalculateLeaps(superpawnLeapGenerator, White.Color);
            blackSuperpawnLeaps = precalculateLeaps(superpawnLeapGenerator, Black.Color);
        }

        private readonly ulong whiteOccupied;
        private readonly ulong blackOccupied;
        private readonly ImmutableArray<uint> pieces;

        private CongoBoard(ulong whiteOccupied, ulong blackOccupied, ImmutableArray<uint> pieces)
        {
            this.whiteOccupied = whiteOccupied;
            this.blackOccupied = blackOccupied;
            this.pieces = pieces;
        }

        private uint getPieceCode(int square)
            => (pieces[square / size] >> (square % size * 4)) & 0xFU;

        public int Size => size;

        public bool IsJungle(int rank, int file) => isJungle(rank, file);

        public bool IsJungle(int square) => isJungle(square);

        public bool IsAboveRiver(int square) => isAboveRiver(square);

        public bool IsRiver(int square) => isRiver(square);

        public bool IsBelowRiver(int square) => isBelowRiver(square);

        public bool IsUpDownBorder(CongoColor color, int square)
        {
            return color.IsWhite()
                ? square >= (int)Square.A7 && square <= (int)Square.G7
                : square >= (int)Square.A1 && square <= (int)Square.G1;
        }

        public bool IsOccupied(int square) => getBit(whiteOccupied | blackOccupied, square);

        public bool IsWhitePiece(int square) => getBit(whiteOccupied, square);

        public bool IsBlackPiece(int square) => getBit(blackOccupied, square);

        public bool IsFirstMovePiece(int square) => IsWhitePiece(square);

        public bool IsFriendlyPiece(CongoColor color, int square)
            => getBit(color.IsWhite() ? whiteOccupied : blackOccupied, square);

        public bool IsOpponentPiece(CongoColor color, int square)
            => getBit(color.IsWhite() ? blackOccupied : whiteOccupied, square);

        public CongoPiece GetPiece(int square)
            => pieceSample[(int)getPieceCode(square)];

        public CongoBoard With(CongoColor color, CongoPiece piece, int square)
        {
            var rank  = square / size;
            var shift = square % size * 4;
            var newWhiteOccupied = setBitToValue(whiteOccupied, square, color.IsWhite());
            var newBlackOccupied = setBitToValue(blackOccupied, square, color.IsBlack());

            return new CongoBoard(newWhiteOccupied, newBlackOccupied,
                pieces.SetItem(rank, (pieces[rank] & ~(0xFU << shift)) | (piece.Code << shift)));
        }

        public CongoBoard Without(int square)
        {
            var newWhiteOccupied = setBitToValue(whiteOccupied, square, false);
            var newBlackOccupied = setBitToValue(blackOccupied, square, false);
            var newPiece = isRiver(square) ? River.Piece : Ground.Piece;
            var rank = square / size;

            return new CongoBoard(newWhiteOccupied, newBlackOccupied,
                pieces.SetItem(rank, setPieceCode(pieces[rank], newPiece, square % size)));
        }

        /// <summary>
        /// Moves as chess king.
        /// </summary>
        public ImmutableArray<int> LeapsAsKing(int square)
            => kingLeaps[square];

        /// <summary>
        /// Moves as chess knight.
        /// </summary>
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

        public bool IsLionCastle(CongoColor color, int square)
        {
            var castle = color.IsWhite() ? whiteLionCastle : blackLionCastle;
            return GetPiece(square).IsLion() && castle.Contains(square);
        }

        /// <summary>
        /// Enumerator versioning is not mandatory due to the board immutability.
        /// </summary>
        public IParametrizedEnumerator<int> GetEnumerator(CongoColor color)
            => new BitScanEnumerator(color.IsWhite() ? whiteOccupied : blackOccupied);

        public override bool Equals(object obj) => this == (CongoBoard)obj;

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            string boardView = "";

            for (int i = 0; i < size * size; i++) {
                if (i != 0 && i % size == 0) { boardView += Environment.NewLine; }
                var pieceView = GetPiece(i).ToString();
                boardView += IsWhitePiece(i) ? pieceView.ToUpper() : pieceView.ToLower();
            }

            return boardView;
        }
    }
}
