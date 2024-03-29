﻿using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
    /// <summary>
    /// Piece is an animal, ground or river. All descendants shall implement
    /// singleton pattern.
    /// </summary>
    public abstract class CongoPiece
    {
        /* The enum is used for defining piece codes, that are used
         * all over the Congo.Core project. Do not change the order! */
        private protected enum PieceId : uint
        {
            Ground, River, Elephant, Zebra, Giraffe,
            Crocodile, Pawn, Superpawn, Lion, Monkey
        }

        private protected abstract PieceId Id { get; }

        internal uint Code => (uint)Id;

        /// <summary>
        /// The method selects capturing leaps from all possible leaps.
        /// </summary>
        protected static List<CongoMove> GetValidCapturingLeaps(List<CongoMove> moves,
            ImmutableArray<int> capturingLeaps, CongoColor color, CongoBoard board, int square)
        {
            foreach (var leap in capturingLeaps) {
                if (!board.IsOccupied(leap) || board.IsOpponentPiece(color, leap)) {
                    moves.Add(new CongoMove(square, leap));
                }
            }

            return moves;
        }

        /// <summary>
        /// The method selects non-capturing leaps from all possible leaps.
        /// </summary>
        protected static List<CongoMove> GetValidNonCapturingLeaps(List<CongoMove> moves,
            ImmutableArray<int> nonCapturingLeaps, CongoBoard board, int square)
        {
            foreach (var leap in nonCapturingLeaps) {
                if (!board.IsOccupied(leap)) {
                    moves.Add(new CongoMove(square, leap));
                }
            }

            return moves;
        }

        public bool IsAnimal() => Id != PieceId.Ground && Id != PieceId.River;

        public bool IsCrocodile() => Id == PieceId.Crocodile;

        public bool IsPawn() => Id == PieceId.Pawn;

        public bool IsSuperpawn() => Id == PieceId.Superpawn;

        public bool IsLion() => Id == PieceId.Lion;

        public bool IsMonkey() => Id == PieceId.Monkey;

        /// <summary>
        /// Returns available moves with respect to its color and position.
        /// Implementation shall generate all possible piece moves and call
        /// @b GetValidCapturingLeaps and @b GetValidNonCapturingLeaps.
        /// </summary>
        public abstract List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square);
    }

    public sealed class Ground : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Ground();

        private Ground() { }

        private protected override PieceId Id { get; } = PieceId.Ground;

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square) => new();
    }

    public sealed class River : CongoPiece
    {
        public static CongoPiece Piece { get; } = new River();

        private River() { }

        private protected override PieceId Id => PieceId.River;

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square) => new();
    }

    public sealed class Elephant : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Elephant();

        private Elephant() { }

        private protected override PieceId Id => PieceId.Elephant;

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
            => GetValidCapturingLeaps(
                new List<CongoMove>(), CongoBoard.LeapsAsElephant(square), color, board, square);
    }

    public sealed class Zebra : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Zebra();

        private Zebra() { }

        private protected override PieceId Id => PieceId.Zebra;

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
            => GetValidCapturingLeaps(
                new List<CongoMove>(), CongoBoard.LeapsAsKnight(square), color, board, square);
    }

    public sealed class Giraffe : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Giraffe();
        
        private Giraffe() { }

        private protected override PieceId Id => PieceId.Giraffe;

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
        {
            var moves = new List<CongoMove>();

            moves = GetValidCapturingLeaps(
                moves, CongoBoard.LeapsAsCapturingGiraffe(square), color, board, square);

            moves = GetValidNonCapturingLeaps(
                moves, CongoBoard.LeapsAsKing(square), board, square);

            return moves;
        }
    }

    public sealed class Crocodile : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Crocodile();

        private Crocodile() { }

        private protected override PieceId Id => PieceId.Crocodile;

        /// <summary>
        /// The Crocodile is proven to stand on the river.
        /// Slide in a given direction is possible till end of the board,
        /// opponent's piece or friendly piece.
        /// </summary>
        private static List<CongoMove> CapturingRiverSlide(List<CongoMove> moves,
            CongoColor color, CongoBoard board, int square, int direction)
        {
            var temp = square + direction;

            // scan till end of the board or some animal
            while (CongoBoard.IsRiver(temp) && !board.IsOccupied(temp)) {
                moves.Add(new CongoMove(square, temp));
                temp += direction;
            }

            // detect capturing move within the board
            if (CongoBoard.IsRiver(temp) && board.IsOpponentPiece(color, temp)) {
                moves.Add(new CongoMove(square, temp));
            }

            return moves;
        }

        /// <summary>
        /// The Crocodile is proven to stand on the ground.
        /// Slide towards the river is possible.
        /// </summary>
        private static List<CongoMove> CapturingGroundSlide(List<CongoMove> moves,
            CongoColor color, CongoBoard board, int square)
        {
            var direction = CongoBoard.IsAboveRiver(square) ? 1 : -1;
            var temp = square + direction * CongoBoard.Size;

            // slide towards the river or hit an animal
            while (!CongoBoard.IsRiver(temp) && !board.IsOccupied(temp)) {
                moves.Add(new CongoMove(square, temp));
                temp += direction * CongoBoard.Size;
            }

            // condition is satisfied by opponent's piece or the river
            if (!board.IsFriendlyPiece(color, temp)) {
                moves.Add(new CongoMove(square, temp));
            }

            return moves;
        }

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
        {
            var moves = new List<CongoMove>();

            moves = GetValidCapturingLeaps(
                moves, CongoBoard.LeapsAsCrocodile(square), color, board, square);

            // sliding in both directions of the river
            if (CongoBoard.IsRiver(square)) {
                moves = CapturingRiverSlide(moves, color, board, square, -1);
                moves = CapturingRiverSlide(moves, color, board, square,  1);
            }

            // slide towards the river
            else {
                moves = CapturingGroundSlide(moves, color, board, square);
            }

            return moves;
        }
    }

    public class Pawn : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Pawn();

        protected Pawn() { }

        private protected override PieceId Id => PieceId.Pawn;

        /// <summary>
        /// Vertical slide towards bottom of the board relative to the color.
        /// </summary>
        protected static List<CongoMove> NonCapturingVerticalSlide(List<CongoMove> moves,
            CongoColor color, CongoBoard board, int square, bool s)
        {
            var slide = color.IsWhite()
                ? CongoBoard.IsAboveRiver(square)
                : CongoBoard.IsBelowRiver(square);

            if (slide || s) {
                var direct = color.IsWhite() ? 1 : -1;
                for (int steps = 1; steps < 3; steps++) {
                    var newSquare = square + direct * steps * CongoBoard.Size;
                    if (!board.IsOccupied(newSquare) && CongoBoard.IsJungle(newSquare)) {
                        moves.Add(new CongoMove(square, newSquare));
                    } else {
                        break;
                    }
                }
            }

            return moves;
        }

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
        {
            var moves = new List<CongoMove>();

            var leaps = CongoBoard.LeapsAsPawn(color, square);
            moves = GetValidCapturingLeaps(moves, leaps, color, board, square);
            moves = NonCapturingVerticalSlide(moves, color, board, square, false);

            return moves;
        }
    }

    public sealed class Superpawn : Pawn
    {
        public static new CongoPiece Piece { get; } = new Superpawn();

        private Superpawn() { }

        private protected override PieceId Id => PieceId.Superpawn;

        /// <summary>
        /// Diagonal moves additional to the non-capturing Pawn vertical moves.
        /// </summary>
        private static List<CongoMove> NonCapturingDiagonalSlide(List<CongoMove> moves,
            CongoBoard board, int square, int rdir, int fdir)
        {
            var rank = square / CongoBoard.Size;
            var file = square % CongoBoard.Size;

            for (int steps = 1; steps < 3; steps++) {
                var newRank = rank + steps * rdir;
                var newFile = file + steps * fdir;
                var newSquare = newRank * CongoBoard.Size + newFile;
                if (CongoBoard.IsJungle(newRank, newFile) && !board.IsOccupied(newSquare)) {
                    moves.Add(new CongoMove(square, newSquare));
                }
                
                else { break; }
            }

            return moves;
        }

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
        {
            var moves = new List<CongoMove>();
            
            var leaps = CongoBoard.LeapsAsSuperpawn(color, square);
            moves = GetValidCapturingLeaps(moves, leaps, color, board, square);

            moves = NonCapturingVerticalSlide(moves, color, board, square, true);

            int rdir = color.IsWhite() ? 1 : -1;
            moves = NonCapturingDiagonalSlide(moves, board, square, rdir,  1);
            moves = NonCapturingDiagonalSlide(moves, board, square, rdir, -1);

            return moves;
        }
    }

    public sealed class Lion : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Lion();

        private Lion() { }

        private protected override PieceId Id => PieceId.Lion;

        /// <summary>
        /// Lion sees opponent's Lion in vertical direction and makes
        /// capturing move.
        /// </summary>
        private static List<CongoMove> VerticalJump(List<CongoMove> moves,
            CongoColor color, CongoBoard board, int square)
        {
            if (board.IsLionCastle(color, square)) {
                var direction = color.IsBlack() ? 1 : -1;

                // position lion could see and could jump
                var newSquare = square;

                do {
                    newSquare += direction * CongoBoard.Size;
                } while (CongoBoard.IsJungle(newSquare) && !board.IsOccupied(newSquare));

                if (CongoBoard.IsJungle(newSquare) && !CongoBoard.IsRiver(newSquare) &&
                    (CongoBoard.IsAboveRiver(square) != CongoBoard.IsAboveRiver(newSquare)) && // different castles
                    board.GetPiece(newSquare) is Lion) {
                    moves.Add(new CongoMove(square, newSquare));
                }
            }

            return moves;
        }

        /// <summary>
        /// Check special condition for Lion diagonal jump, checks all
        /// 4 possibilities.
        /// </summary>
        private static List<CongoMove> DiagonalJump(List<CongoMove> moves,
            CongoColor color, CongoBoard board, int square)
        {
            if (board.TryDiagonalJump(color, square, out int target)) {
                moves.Add(new CongoMove(square, target));
            }

            return moves;
        }

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
        {
            var moves = new List<CongoMove>();

            var leaps = CongoBoard.LeapsAsLion(color, square);
            moves = GetValidCapturingLeaps(moves, leaps, color, board, square);
            moves = VerticalJump(moves, color, board, square);
            moves = DiagonalJump(moves, color, board, square);

            return moves;
        }
    }

    public sealed class Monkey : CongoPiece
    {
        public static CongoPiece Piece { get; } = new Monkey();

        private Monkey() { }

        private protected override PieceId Id => PieceId.Monkey;

        /// <summary>
        /// Capturing jump over enemy animal if square behind exists and
        /// is a Ground or a River.
        /// </summary>
        private static List<CongoMove> AddMonkeyJump(List<CongoMove> moves,
            CongoBoard board, int square, int leap)
        {
            var newRank = 2 * (leap / CongoBoard.Size) - square / CongoBoard.Size;
            var newFile = 2 * (leap % CongoBoard.Size) - square % CongoBoard.Size;
            var newSquare = newRank * CongoBoard.Size + newFile;
            if (CongoBoard.IsJungle(newRank, newFile) && !board.IsOccupied(newSquare)) {
                moves.Add(new MonkeyJump(square, leap, newSquare));
            }

            return moves;
        }

        /// <summary>
        /// Generate only possible monkey capturing jumps from a given position.
        /// Other ordinary moves are not considered.
        /// </summary>
        public static List<CongoMove> ContinueJump(CongoColor color, CongoBoard board, int square)
        {
            var moves = new List<CongoMove>
            {
                new CongoMove(square, square) // jump can be interrupted
            };

            var leaps = CongoBoard.LeapsAsKing(square);

            // look for opponent's pieces around and try to jump over
            foreach (var leap in leaps) {
                if (board.IsOpponentPiece(color, leap)) {
                    moves = AddMonkeyJump(moves, board, square, leap);
                }

                else { /* do nothing, only captures are allowed */ }
            }

            return moves;
        }

        public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
        {
            var moves = new List<CongoMove>();
            var leaps = CongoBoard.LeapsAsKing(square);

            foreach (var leap in leaps) {

                // ground or river -> simple move
                if (!board.IsOccupied(leap)) {
                    moves.Add(new CongoMove(square, leap));
                }

                // opponent's piece -> capturing move
                else if (board.IsOpponentPiece(color, leap)) {
                    moves = AddMonkeyJump(moves, board, square, leap);
                }

                else {
                    // friendly piece -> do nothing
                }
            }

            return moves;
        }
    }
}
