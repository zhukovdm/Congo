using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{

    [TestClass]
    public class Piece_ElephantMoves_Test
    {
        [TestMethod]
        public void SkipFriendlyPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Elephant.Piece, (int)Square.A7)
                .With(White.Color, Elephant.Piece, (int)Square.B7);
            var piece = board.GetPiece((int)Square.A7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.A7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.A7, (int)Square.C7),
                new CongoMove((int)Square.A7, (int)Square.A6),
                new CongoMove((int)Square.A7, (int)Square.A5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void CaptureOpponentPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Elephant.Piece, (int)Square.A7)
                .With(Black.Color, Elephant.Piece, (int)Square.B7)
                .With(Black.Color, Elephant.Piece, (int)Square.C7);
            var piece = board.GetPiece((int)Square.A7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.A7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.A7, (int)Square.C7),
                new CongoMove((int)Square.A7, (int)Square.B7),
                new CongoMove((int)Square.A7, (int)Square.A6),
                new CongoMove((int)Square.A7, (int)Square.A5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }

    [TestClass]
    public class Piece_ZebraMoves_Test
    {
        [TestMethod]
        public void SkipFriendlyPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Zebra.Piece, (int)Square.D4)
                .With(White.Color, Zebra.Piece, (int)Square.E6)
                .With(White.Color, Zebra.Piece, (int)Square.F5)
                .With(White.Color, Zebra.Piece, (int)Square.B3)
                .With(White.Color, Zebra.Piece, (int)Square.E2);
            var piece = board.GetPiece((int)Square.D4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D4, (int)Square.F3),
                new CongoMove((int)Square.D4, (int)Square.C2),
                new CongoMove((int)Square.D4, (int)Square.C6),
                new CongoMove((int)Square.D4, (int)Square.B5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void CaptureOpponentPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Zebra.Piece, (int)Square.D7)
                .With(Black.Color, Zebra.Piece, (int)Square.C5)
                .With(Black.Color, Zebra.Piece, (int)Square.B6)
                .With(Black.Color, Zebra.Piece, (int)Square.F6)
                .With(Black.Color, Zebra.Piece, (int)Square.E5);
            var piece = board.GetPiece((int)Square.D7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D7, (int)Square.B6),
                new CongoMove((int)Square.D7, (int)Square.C5),
                new CongoMove((int)Square.D7, (int)Square.E5),
                new CongoMove((int)Square.D7, (int)Square.F6)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }

    [TestClass]
    public class Piece_GiraffeMoves_Test
    {
        [TestMethod]
        public void SkipFriendlyPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Giraffe.Piece, (int)Square.A7)
                .With(White.Color, Giraffe.Piece, (int)Square.A6)
                .With(White.Color, Giraffe.Piece, (int)Square.B7)
                .With(White.Color, Giraffe.Piece, (int)Square.A5)
                .With(White.Color, Giraffe.Piece, (int)Square.C5);
            var piece = board.GetPiece((int)Square.A7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.A7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.A7, (int)Square.B6),
                new CongoMove((int)Square.A7, (int)Square.C7)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void CaptureAndSkipOpponentPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Giraffe.Piece, (int)Square.A7)
                .With(Black.Color, Giraffe.Piece, (int)Square.A6)
                .With(Black.Color, Giraffe.Piece, (int)Square.B6)
                .With(Black.Color, Giraffe.Piece, (int)Square.B7)
                .With(Black.Color, Giraffe.Piece, (int)Square.A5)
                .With(Black.Color, Giraffe.Piece, (int)Square.C5)
                .With(Black.Color, Giraffe.Piece, (int)Square.C7);
            var piece = board.GetPiece((int)Square.A7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.A7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.A7, (int)Square.A5),
                new CongoMove((int)Square.A7, (int)Square.C5),
                new CongoMove((int)Square.A7, (int)Square.C7)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }

    [TestClass]
    public class Piece_CrocodileMoves_Test
    {
        [TestMethod]
        public void SkipFriendlyPiecesWater()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Crocodile.Piece, (int)Square.D4)
                .With(White.Color, Crocodile.Piece, (int)Square.C4)
                .With(White.Color, Crocodile.Piece, (int)Square.C5)
                .With(White.Color, Crocodile.Piece, (int)Square.E5)
                .With(White.Color, Crocodile.Piece, (int)Square.D3);
            var piece = board.GetPiece((int)Square.D4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D4, (int)Square.D5),
                new CongoMove((int)Square.D4, (int)Square.E4),
                new CongoMove((int)Square.D4, (int)Square.F4),
                new CongoMove((int)Square.D4, (int)Square.G4),
                new CongoMove((int)Square.D4, (int)Square.E3),
                new CongoMove((int)Square.D4, (int)Square.C3)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void CaptureOpponentPiecesWater()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Crocodile.Piece, (int)Square.D4)
                .With(Black.Color, Crocodile.Piece, (int)Square.C4)
                .With(Black.Color, Crocodile.Piece, (int)Square.F4)
                .With(Black.Color, Crocodile.Piece, (int)Square.C5)
                .With(Black.Color, Crocodile.Piece, (int)Square.E5)
                .With(Black.Color, Crocodile.Piece, (int)Square.D3);
            var piece = board.GetPiece((int)Square.D4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D4, (int)Square.C4),
                new CongoMove((int)Square.D4, (int)Square.C5),
                new CongoMove((int)Square.D4, (int)Square.D5),
                new CongoMove((int)Square.D4, (int)Square.E5),
                new CongoMove((int)Square.D4, (int)Square.E4),
                new CongoMove((int)Square.D4, (int)Square.F4),
                new CongoMove((int)Square.D4, (int)Square.E3),
                new CongoMove((int)Square.D4, (int)Square.D3),
                new CongoMove((int)Square.D4, (int)Square.C3)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void SkipFriendlyPiecesGround()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Crocodile.Piece, (int)Square.D7)
                .With(White.Color, Crocodile.Piece, (int)Square.C7)
                .With(White.Color, Crocodile.Piece, (int)Square.C6)
                .With(White.Color, Crocodile.Piece, (int)Square.E6)
                .With(White.Color, Crocodile.Piece, (int)Square.D4);
            var piece = board.GetPiece((int)Square.D7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D7, (int)Square.D6),
                new CongoMove((int)Square.D7, (int)Square.E7),
                new CongoMove((int)Square.D7, (int)Square.D5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void CaptureOpponentPiecesGround()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Crocodile.Piece, (int)Square.D7)
                .With(Black.Color, Crocodile.Piece, (int)Square.C7)
                .With(Black.Color, Crocodile.Piece, (int)Square.C6)
                .With(Black.Color, Crocodile.Piece, (int)Square.E6)
                .With(Black.Color, Crocodile.Piece, (int)Square.D4);
            var piece = board.GetPiece((int)Square.D7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D7, (int)Square.C7),
                new CongoMove((int)Square.D7, (int)Square.C6),
                new CongoMove((int)Square.D7, (int)Square.D6),
                new CongoMove((int)Square.D7, (int)Square.E6),
                new CongoMove((int)Square.D7, (int)Square.E7),
                new CongoMove((int)Square.D7, (int)Square.D5),
                new CongoMove((int)Square.D7, (int)Square.D4)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void GroundMovesBelowRiver()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Crocodile.Piece, (int)Square.F1);
            var piece = board.GetPiece((int)Square.F1);
            var actual = piece.GetMoves(White.Color, board, (int)Square.F1);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.F1, (int)Square.E1),
                new CongoMove((int)Square.F1, (int)Square.E2),
                new CongoMove((int)Square.F1, (int)Square.F4),
                new CongoMove((int)Square.F1, (int)Square.F3),
                new CongoMove((int)Square.F1, (int)Square.F2),
                new CongoMove((int)Square.F1, (int)Square.G2),
                new CongoMove((int)Square.F1, (int)Square.G1)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }

    [TestClass]
    public class Piece_WhitePawnMoves_Test
    {
        [TestMethod]
        public void BelowRiverNoBackwardSlide()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Pawn.Piece, (int)Square.G3);
            var piece = board.GetPiece((int)Square.G3);
            var actual = piece.GetMoves(White.Color, board, (int)Square.G3);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.G3, (int)Square.F4),
                new CongoMove((int)Square.G3, (int)Square.G4)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void RiverNoBackwardSlide()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Pawn.Piece, (int)Square.D4);
            var piece = board.GetPiece((int)Square.D4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D4, (int)Square.C5),
                new CongoMove((int)Square.D4, (int)Square.D5),
                new CongoMove((int)Square.D4, (int)Square.E5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void AboveRiverBackwardSlide()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Pawn.Piece, (int)Square.B5);
            var piece = board.GetPiece((int)Square.B5);
            var actual = piece.GetMoves(White.Color, board, (int)Square.B5);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.B5, (int)Square.A6),
                new CongoMove((int)Square.B5, (int)Square.B6),
                new CongoMove((int)Square.B5, (int)Square.C6),
                new CongoMove((int)Square.B5, (int)Square.B4),
                new CongoMove((int)Square.B5, (int)Square.B3)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BackwardSlideSkipFriendlyPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Pawn.Piece, (int)Square.D5)
                .With(White.Color, Pawn.Piece, (int)Square.D3);
            var piece = board.GetPiece((int)Square.D5);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D5);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D5, (int)Square.C6),
                new CongoMove((int)Square.D5, (int)Square.D6),
                new CongoMove((int)Square.D5, (int)Square.E6),
                new CongoMove((int)Square.D5, (int)Square.D4)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BackwardSlideSkipOpponentPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Pawn.Piece, (int)Square.D5)
                .With(Black.Color, Pawn.Piece, (int)Square.D4);
            var piece = board.GetPiece((int)Square.D5);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D5);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D5, (int)Square.C6),
                new CongoMove((int)Square.D5, (int)Square.D6),
                new CongoMove((int)Square.D5, (int)Square.E6)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }

    [TestClass]
    public class Piece_WhiteSuperpawnMoves_Test
    {
        [TestMethod]
        public void BelowRiverBackwardSlide()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Superpawn.Piece, (int)Square.D2);
            var piece = board.GetPiece((int)Square.D2);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D2);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D2, (int)Square.C2),
                new CongoMove((int)Square.D2, (int)Square.C3),
                new CongoMove((int)Square.D2, (int)Square.D3),
                new CongoMove((int)Square.D2, (int)Square.E3),
                new CongoMove((int)Square.D2, (int)Square.E2),
                new CongoMove((int)Square.D2, (int)Square.E1),
                new CongoMove((int)Square.D2, (int)Square.D1),
                new CongoMove((int)Square.D2, (int)Square.C1)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void RiverBackwardSlide()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Superpawn.Piece, (int)Square.A4);
            var piece = board.GetPiece((int)Square.A4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.A4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.A4, (int)Square.A5),
                new CongoMove((int)Square.A4, (int)Square.B5),
                new CongoMove((int)Square.A4, (int)Square.B4),
                new CongoMove((int)Square.A4, (int)Square.A3),
                new CongoMove((int)Square.A4, (int)Square.A2),
                new CongoMove((int)Square.A4, (int)Square.B3),
                new CongoMove((int)Square.A4, (int)Square.C2)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void AboveRiverBackwardSlide()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Superpawn.Piece, (int)Square.F7);
            var piece = board.GetPiece((int)Square.F7);
            var actual = piece.GetMoves(White.Color, board, (int)Square.F7);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.F7, (int)Square.E7),
                new CongoMove((int)Square.F7, (int)Square.E6),
                new CongoMove((int)Square.F7, (int)Square.F6),
                new CongoMove((int)Square.F7, (int)Square.G6),
                new CongoMove((int)Square.F7, (int)Square.G7),
                new CongoMove((int)Square.F7, (int)Square.D5),
                new CongoMove((int)Square.F7, (int)Square.F5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BackwardSlideSkipFriendlyPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Superpawn.Piece, (int)Square.D4)
                .With(White.Color, Superpawn.Piece, (int)Square.B2)
                .With(White.Color, Superpawn.Piece, (int)Square.D3)
                .With(White.Color, Superpawn.Piece, (int)Square.F2);
            var piece = board.GetPiece((int)Square.D4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D4, (int)Square.C4),
                new CongoMove((int)Square.D4, (int)Square.C5),
                new CongoMove((int)Square.D4, (int)Square.D5),
                new CongoMove((int)Square.D4, (int)Square.E5),
                new CongoMove((int)Square.D4, (int)Square.E4),
                new CongoMove((int)Square.D4, (int)Square.E3),
                new CongoMove((int)Square.D4, (int)Square.C3)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BackwardSlideSkipOpponentPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Superpawn.Piece, (int)Square.D4)
                .With(Black.Color, Superpawn.Piece, (int)Square.B2)
                .With(Black.Color, Superpawn.Piece, (int)Square.D3)
                .With(Black.Color, Superpawn.Piece, (int)Square.F2);
            var piece = board.GetPiece((int)Square.D4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D4, (int)Square.C4),
                new CongoMove((int)Square.D4, (int)Square.C5),
                new CongoMove((int)Square.D4, (int)Square.D5),
                new CongoMove((int)Square.D4, (int)Square.E5),
                new CongoMove((int)Square.D4, (int)Square.E4),
                new CongoMove((int)Square.D4, (int)Square.E3),
                new CongoMove((int)Square.D4, (int)Square.C3)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }

    [TestClass]
    public class Piece_WhiteLionMoves_Test
    {
        [TestMethod]
        public void VerticalJump()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.D1)
                .With(Black.Color, Lion.Piece, (int)Square.D6);
            var piece = board.GetPiece((int)Square.D1);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D1);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D1, (int)Square.C1),
                new CongoMove((int)Square.D1, (int)Square.C2),
                new CongoMove((int)Square.D1, (int)Square.D2),
                new CongoMove((int)Square.D1, (int)Square.E2),
                new CongoMove((int)Square.D1, (int)Square.E1),
                new CongoMove((int)Square.D1, (int)Square.D6)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BlockedVerticalJumpByFriendlyPiece()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.D1)
                .With(White.Color, Pawn.Piece, (int)Square.D5)
                .With(Black.Color, Lion.Piece, (int)Square.D6);
            var piece = board.GetPiece((int)Square.D1);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D1);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D1, (int)Square.C1),
                new CongoMove((int)Square.D1, (int)Square.C2),
                new CongoMove((int)Square.D1, (int)Square.D2),
                new CongoMove((int)Square.D1, (int)Square.E2),
                new CongoMove((int)Square.D1, (int)Square.E1)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BlockedVerticalJumpByOpponentPiece()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.D1)
                .With(Black.Color, Pawn.Piece, (int)Square.D4)
                .With(Black.Color, Lion.Piece, (int)Square.D6);
            var piece = board.GetPiece((int)Square.D1);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D1);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D1, (int)Square.C1),
                new CongoMove((int)Square.D1, (int)Square.C2),
                new CongoMove((int)Square.D1, (int)Square.D2),
                new CongoMove((int)Square.D1, (int)Square.E2),
                new CongoMove((int)Square.D1, (int)Square.E1)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BlockedVerticalJumpOutsideCastle()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.B3)
                .With(Black.Color, Lion.Piece, (int)Square.B5);
            var piece = board.GetPiece((int)Square.B3);
            var actual = piece.GetMoves(White.Color, board, (int)Square.B3);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                /* no moves */
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BlockedVerticalJumpWrongRiverSideInsideCastle()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.D5)
                .With(Black.Color, Lion.Piece, (int)Square.D3);
            var piece = board.GetPiece((int)Square.D5);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D5);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                /* no moves */
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void DiagonalJump_C3_E5()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.C3)
                .With(Black.Color, Lion.Piece, (int)Square.E5);
            var piece = board.GetPiece((int)Square.C3);
            var actual = piece.GetMoves(White.Color, board, (int)Square.C3);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.C3, (int)Square.D3),
                new CongoMove((int)Square.C3, (int)Square.D2),
                new CongoMove((int)Square.C3, (int)Square.C2),
                new CongoMove((int)Square.C3, (int)Square.E5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void DiagonalJump_E3_C5()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.E3)
                .With(Black.Color, Lion.Piece, (int)Square.C5);
            var piece = board.GetPiece((int)Square.E3);
            var actual = piece.GetMoves(White.Color, board, (int)Square.E3);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.E3, (int)Square.D3),
                new CongoMove((int)Square.E3, (int)Square.D2),
                new CongoMove((int)Square.E3, (int)Square.E2),
                new CongoMove((int)Square.E3, (int)Square.C5)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BlockedDiagonalJumpByFriendlyPiece()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.C3)
                .With(White.Color, Pawn.Piece, (int)Square.D4)
                .With(Black.Color, Lion.Piece, (int)Square.E5);
            var piece = board.GetPiece((int)Square.C3);
            var actual = piece.GetMoves(White.Color, board, (int)Square.C3);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.C3, (int)Square.D3),
                new CongoMove((int)Square.C3, (int)Square.D2),
                new CongoMove((int)Square.C3, (int)Square.C2)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BlockedDiagonalJumpByOpponentPiece()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.E3)
                .With(White.Color, Pawn.Piece, (int)Square.D4)
                .With(Black.Color, Lion.Piece, (int)Square.C5);
            var piece = board.GetPiece((int)Square.E3);
            var actual = piece.GetMoves(White.Color, board, (int)Square.E3);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.E3, (int)Square.D3),
                new CongoMove((int)Square.E3, (int)Square.D2),
                new CongoMove((int)Square.E3, (int)Square.E2)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }

    [TestClass]
    public class Piece_MonkeyMoves_Test
    {
        [TestMethod]
        public void SkipFriendlyPieces()
        {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Monkey.Piece, (int)Square.D3)
                .With(White.Color, Pawn.Piece, (int)Square.C3)
                .With(White.Color, Pawn.Piece, (int)Square.D4)
                .With(White.Color, Pawn.Piece, (int)Square.E4)
                .With(White.Color, Pawn.Piece, (int)Square.E2);
            var piece = board.GetPiece((int)Square.D3);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D3);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D3, (int)Square.C4),
                new CongoMove((int)Square.D3, (int)Square.E3),
                new CongoMove((int)Square.D3, (int)Square.C2),
                new CongoMove((int)Square.D3, (int)Square.D2)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void JumpOverOpponentPieces() {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Monkey.Piece, (int)Square.D4)
                .With(Black.Color, Pawn.Piece, (int)Square.C3)
                .With(Black.Color, Pawn.Piece, (int)Square.C4)
                .With(Black.Color, Pawn.Piece, (int)Square.D5)
                .With(Black.Color, Pawn.Piece, (int)Square.E3);
            var piece = board.GetPiece((int)Square.D4);
            var actual = piece.GetMoves(White.Color, board, (int)Square.D4);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.D4, (int)Square.B2),
                new CongoMove((int)Square.D4, (int)Square.B4),
                new CongoMove((int)Square.D4, (int)Square.C5),
                new CongoMove((int)Square.D4, (int)Square.D6),
                new CongoMove((int)Square.D4, (int)Square.E5),
                new CongoMove((int)Square.D4, (int)Square.E4),
                new CongoMove((int)Square.D4, (int)Square.F2),
                new CongoMove((int)Square.D4, (int)Square.D3)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }

        [TestMethod]
        public void BoundaryJumpOverOpponentPieces() {
            var comparer = new CongoMoveGenComparer();
            var board = CongoBoard.Empty
                .With(White.Color, Monkey.Piece, (int)Square.F2)
                .With(Black.Color, Pawn.Piece, (int)Square.E1)
                .With(Black.Color, Pawn.Piece, (int)Square.E2)
                .With(Black.Color, Pawn.Piece, (int)Square.E3)
                .With(Black.Color, Pawn.Piece, (int)Square.F3)
                .With(Black.Color, Pawn.Piece, (int)Square.G3)
                .With(Black.Color, Pawn.Piece, (int)Square.G2)
                .With(Black.Color, Pawn.Piece, (int)Square.G1)
                .With(Black.Color, Pawn.Piece, (int)Square.F1);
            var piece = board.GetPiece((int)Square.F2);
            var actual = piece.GetMoves(White.Color, board, (int)Square.F2);
            actual.Sort(comparer);
            var expected = new List<CongoMove>() {
                new CongoMove((int)Square.F2, (int)Square.D2),
                new CongoMove((int)Square.F2, (int)Square.D4),
                new CongoMove((int)Square.F2, (int)Square.F4)
            };
            expected.Sort(comparer);
            CollectionAssert.AreEqual(expected, actual, new CongoMoveObjComparer());
        }
    }
}
