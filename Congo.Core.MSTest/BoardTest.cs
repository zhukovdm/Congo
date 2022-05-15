using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
    [TestClass]
    public class Board_SupportingMethods_Test
    {
        [TestMethod]
        public void EmptyBoardExists()
        {
            var board = CongoBoard.Empty;
            Assert.IsNotNull(board);
        }

        [TestMethod]
        public void GroundRiverPiecesOnEmptyBoard()
        {
            var board = CongoBoard.Empty;
            var piece1 = board.GetPiece((int)Square.A7);
            var piece2 = board.GetPiece((int)Square.D4);
            Assert.IsTrue(
                piece1.GetType() == typeof(Ground) &&
                piece2.GetType() == typeof(River));
        }

        [TestMethod]
        public void EmptySquaresAreNotOccupied()
        {
            var board = CongoBoard.Empty;
            Assert.IsFalse(
                board.IsOccupied((int)Square.A1) ||
                board.IsOccupied((int)Square.B3) ||
                board.IsOccupied((int)Square.G3) ||
                board.IsOccupied((int)Square.G1));
        }

        [TestMethod]
        public void AddAndGetBlackLion()
        {
            var board = CongoBoard.Empty;
            board = board.With(Black.Color, Lion.Piece, (int)Square.A1);
            var piece = board.GetPiece((int)Square.A1);
            Assert.IsTrue(piece.GetType() == typeof(Lion));
        }

        [TestMethod]
        public void AddAndRemoveBlackLion()
        {
            var board = CongoBoard.Empty;
            board = board.With(Black.Color, Lion.Piece, (int)Square.A1);
            board = board.Without((int)Square.A1);
            var piece = board.GetPiece((int)Square.A1);
            Assert.IsTrue(piece.GetType() == typeof(Ground));
        }

        [TestMethod]
        public void IsSquareWater()
        {
            var board = CongoBoard.Empty;
            var isWater = true;
            for (int i = 0; i < board.Size * 3; i++) {
                isWater &= !board.IsRiver(i);
            }
            for (int i = board.Size * 4; i < board.Size * board.Size; i++) {
                isWater &= !board.IsRiver(i);
            }
            var isNotWater = true;
            for (int i = board.Size * 3; i < board.Size * 4; i++) {
                isNotWater &= board.IsRiver(i);
            }
            Assert.IsTrue(isWater & isNotWater);
        }

        [TestMethod]
        public void EqualBoardsBothNull()
        {
            CongoBoard b1 = null;
            CongoBoard b2 = null;
            Assert.IsTrue(b1 == b2);
        }

        [TestMethod]
        public void NotEqualBoardsLeftNull()
        {
            CongoBoard b1 = null;
            CongoBoard b2 = CongoBoard.Empty;
            Assert.IsTrue(b1 != b2);
        }

        [TestMethod]
        public void NotEqualBoardsRightNull()
        {
            CongoBoard b1 = CongoBoard.Empty;
            CongoBoard b2 = null;
            Assert.IsTrue(b1 != b2);
        }

        [TestMethod]
        public void NotEqualBoardsBothNonNullSameColorDifferentPieces()
        {
            CongoBoard b1 = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.D4);

            CongoBoard b2 = CongoBoard.Empty
                .With(White.Color, Pawn.Piece, (int)Square.D4);

            Assert.IsTrue(b1 != b2);
        }
        
        [TestMethod]
        public void NotEqualBoardsBothNonNullSamePiecesDifferentColor()
        {
            CongoBoard b1 = CongoBoard.Empty
                .With(White.Color, Lion.Piece, (int)Square.D4);

            CongoBoard b2 = CongoBoard.Empty
                .With(Black.Color, Lion.Piece, (int)Square.D4);

            Assert.IsTrue(b1 != b2);
        }

        [TestMethod]
        public void EqualStandardBoards()
        {
            CongoBoard b1 = CongoGame.Standard().Board;
            CongoBoard b2 = CongoGame.Standard().Board;

            Assert.IsTrue(b1 == b2);
        }
    }

    [TestClass]
    public class Board_KingLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.B7, (int)Square.A6, (int)Square.B6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.E7, (int)Square.C6,
                (int)Square.D6, (int)Square.E6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.F7, (int)Square.F6, (int)Square.G6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineLeftBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A5, (int)Square.B5, (int)Square.B4,
                (int)Square.A3, (int)Square.B3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.C5, (int)Square.D5, (int)Square.E5,
                (int)Square.C4, (int)Square.E4, (int)Square.C3,
                (int)Square.D3, (int)Square.E3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineRightBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.F5, (int)Square.G5, (int)Square.F4,
                (int)Square.F3, (int)Square.G3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.A2, (int)Square.B2, (int)Square.B1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.C2, (int)Square.D2, (int)Square.E2,
                (int)Square.C1, (int)Square.E1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKing((int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.F2, (int)Square.G2, (int)Square.F1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_KnightLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.C6, (int)Square.B5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.B6, (int)Square.C5, (int)Square.E5,
                (int)Square.F6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.E6, (int)Square.F5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineLeftBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.B6, (int)Square.C5, (int)Square.C3,
                (int)Square.B2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.B5, (int)Square.C6, (int)Square.E6,
                (int)Square.F5, (int)Square.F3, (int)Square.E2,
                (int)Square.C2, (int)Square.B3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineRightBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.F6, (int)Square.E5, (int)Square.E3,
                (int)Square.F2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.B3, (int)Square.C2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.B2, (int)Square.C3, (int)Square.E3,
                (int)Square.F2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsKnight((int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.E2, (int)Square.F3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_ElephantLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.A6, (int)Square.A5, (int)Square.B7,
                (int)Square.C7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.B7, (int)Square.C7, (int)Square.E7,
                (int)Square.F7, (int)Square.D6, (int)Square.D5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.E7, (int)Square.F7, (int)Square.G6,
                (int)Square.G5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineLeftBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A6, (int)Square.A5, (int)Square.B4,
                (int)Square.C4, (int)Square.A3, (int)Square.A2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.D6, (int)Square.D5, (int)Square.E4,
                (int)Square.F4, (int)Square.D3, (int)Square.D2,
                (int)Square.C4, (int)Square.B4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineRightBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.G6, (int)Square.G5, (int)Square.G3,
                (int)Square.G2, (int)Square.F4, (int)Square.E4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.A2, (int)Square.A3, (int)Square.B1,
                (int)Square.C1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.B1, (int)Square.C1, (int)Square.D2,
                (int)Square.D3, (int)Square.E1, (int)Square.F1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.E1, (int)Square.F1, (int)Square.G2,
                (int)Square.G3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperLeftCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.B6).Sort();
            var expected = new int[] {
                (int)Square.A6, (int)Square.B7, (int)Square.C6,
                (int)Square.D6, (int)Square.B5, (int)Square.B4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperRightCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.F6).Sort();
            var expected = new int[] {
                (int)Square.F7, (int)Square.D6, (int)Square.E6,
                (int)Square.G6, (int)Square.F5, (int)Square.F4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerLeftCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.B2).Sort();
            var expected = new int[] {
                (int)Square.B4, (int)Square.B3, (int)Square.A2,
                (int)Square.C2, (int)Square.D2, (int)Square.B1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerRightCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsElephant((int)Square.F2).Sort();
            var expected = new int[] {
                (int)Square.F4, (int)Square.F3, (int)Square.D2,
                (int)Square.E2, (int)Square.G2, (int)Square.F1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_CapturingGiraffeLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.A5, (int)Square.C5, (int)Square.C7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.B7, (int)Square.B5, (int)Square.D5,
                (int)Square.F5, (int)Square.F7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.E7, (int)Square.E5, (int)Square.G5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineLeftBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A6, (int)Square.C6, (int)Square.C4,
                (int)Square.C2, (int)Square.A2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.B6, (int)Square.D6, (int)Square.F6,
                (int)Square.F4, (int)Square.F2, (int)Square.D2,
                (int)Square.B2, (int)Square.B4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLineRightBorderLine()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.G6, (int)Square.E6, (int)Square.E4,
                (int)Square.E2, (int)Square.G2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.A3, (int)Square.C3, (int)Square.C1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.B1, (int)Square.B3, (int)Square.D3,
                (int)Square.F3, (int)Square.F1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLineRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.E1, (int)Square.E3, (int)Square.G3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperLeftCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.B6).Sort();
            var expected = new int[] {
                (int)Square.B4, (int)Square.D4, (int)Square.D6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperRightCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.F6).Sort();
            var expected = new int[] {
                (int)Square.F4, (int)Square.D4, (int)Square.D6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerLeftCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.B2).Sort();
            var expected = new int[] {
                (int)Square.B4, (int)Square.D4, (int)Square.D2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerRightCornerInside()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCapturingGiraffe((int)Square.F2).Sort();
            var expected = new int[] {
                (int)Square.D2, (int)Square.D4, (int)Square.F4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_CrocodileLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.B6, (int)Square.B7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.C6, (int)Square.E6,
                (int)Square.E7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.F7, (int)Square.F6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void InsideBoardCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.D6).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.D7, (int)Square.E7,
                (int)Square.E6, (int)Square.E5, (int)Square.C5,
                (int)Square.C6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WaterLeftBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A5, (int)Square.B5, (int)Square.B3,
                (int)Square.A3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WaterCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.C5, (int)Square.D5, (int)Square.E5,
                (int)Square.C3, (int)Square.D3, (int)Square.E3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WaterRightBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.F5, (int)Square.G5, (int)Square.F3,
                (int)Square.G3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.B2, (int)Square.B1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.C1, (int)Square.C2, (int)Square.E1,
                (int)Square.E2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsCrocodile((int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.F1, (int)Square.F2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_WhitePawnLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.A7).Sort();
            var expected = new int[] {
                /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.D7).Sort();
            var expected = new int[] {
                /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.G7).Sort();
            var expected = new int[] {
                /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLeftBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A5, (int)Square.B5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.C5, (int)Square.D5, (int)Square.E5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleRightBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.F5, (int)Square.G5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.A2, (int)Square.B2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.C2, (int)Square.D2, (int)Square.E2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(White.Color, (int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.F2, (int)Square.G2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_BlackPawnLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.A6, (int)Square.B6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.C6, (int)Square.D6, (int)Square.E6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.F6, (int)Square.G6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLeftBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A3, (int)Square.B3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.C3, (int)Square.D3, (int)Square.E3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleRightBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.F3, (int)Square.G3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.A1).Sort();
            var expected = new int[] {
                /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.D1).Sort();
            var expected = new int[] {
                /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsPawn(Black.Color, (int)Square.G1).Sort();
            var expected = new int[] {
                /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_WhiteSuperpawnLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.B7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.E7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.F7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLeftBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A5, (int)Square.B5, (int)Square.B4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.C4, (int)Square.C5, (int)Square.D5,
                (int)Square.E5, (int)Square.E4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleRightBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.F4, (int)Square.F5, (int)Square.G5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.A2, (int)Square.B2, (int)Square.B1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.C1, (int)Square.C2, (int)Square.D2,
                (int)Square.E2, (int)Square.E1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(White.Color, (int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.F1, (int)Square.F2, (int)Square.G2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_BlackSuperpawnLeapGeneration_Test
    {
        [TestMethod]
        public void UpperBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.A7).Sort();
            var expected = new int[] {
                (int)Square.A6, (int)Square.B6, (int)Square.B7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.C6, (int)Square.D6,
                (int)Square.E6, (int)Square.E7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.G7).Sort();
            var expected = new int[] {
                (int)Square.F7, (int)Square.F6, (int)Square.G6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleLeftBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.A4).Sort();
            var expected = new int[] {
                (int)Square.A3, (int)Square.B3, (int)Square.B4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.D4).Sort();
            var expected = new int[] {
                (int)Square.C4, (int)Square.C3, (int)Square.D3,
                (int)Square.E3, (int)Square.E4
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleRightBorder()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.G4).Sort();
            var expected = new int[] {
                (int)Square.F4, (int)Square.F3, (int)Square.G3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.A1).Sort();
            var expected = new int[] {
                (int)Square.B1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.D1).Sort();
            var expected = new int[] {
                (int)Square.C1, (int)Square.E1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsSuperpawn(Black.Color, (int)Square.G1).Sort();
            var expected = new int[] {
                (int)Square.F1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_WhiteLionLeapGeneration_Test
    {
        [TestMethod]
        public void OutsideCastleUpperPart()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.D6).Sort();
            var expected = new int[] { /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void OutsideCastleRiver()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.D4).Sort();
            var expected = new int[] { /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void OutsideCastleLowerPart()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.B3).Sort();
            var expected = new int[] { /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.C3).Sort();
            var expected = new int[] {
                (int)Square.C2, (int)Square.D2, (int)Square.D3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.D3).Sort();
            var expected = new int[] {
                (int)Square.C3, (int)Square.C2, (int)Square.D2,
                (int)Square.E2, (int)Square.E3
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.E3).Sort();
            var expected = new int[] {
                (int)Square.D3, (int)Square.D2, (int)Square.E2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.C2).Sort();
            var expected = new int[] {
                (int)Square.C3, (int)Square.D3, (int)Square.D2,
                (int)Square.D1, (int)Square.C1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.D2).Sort();
            var expected = new int[] {
                (int)Square.C3, (int)Square.D3, (int)Square.E3,
                (int)Square.C2, (int)Square.E2, (int)Square.C1,
                (int)Square.D1, (int)Square.E1
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(White.Color, (int)Square.E1).Sort();
            var expected = new int[] {
                (int)Square.D1, (int)Square.D2, (int)Square.E2
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }

    [TestClass]
    public class Board_BlackLionLeapGeneration_Test
    {
        [TestMethod]
        public void OutsideCastleUpperPart()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.A1).Sort();
            var expected = new int[] { /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void OutsideCastleRiver()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.D4).Sort();
            var expected = new int[] { /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void OutsideCastleLowerPart()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.D2).Sort();
            var expected = new int[] { /* no leaps */
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderLeftCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.C7).Sort();
            var expected = new int[] {
                (int)Square.C6, (int)Square.D6, (int)Square.D7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.D7).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.C6, (int)Square.D6,
                (int)Square.E6, (int)Square.E7
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UpperBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.E7).Sort();
            var expected = new int[] {
                (int)Square.D7, (int)Square.D6, (int)Square.E6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.C6).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.D7, (int)Square.D6,
                (int)Square.D5, (int)Square.C5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MiddleCenter()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.D6).Sort();
            var expected = new int[] {
                (int)Square.C7, (int)Square.D7, (int)Square.E7,
                (int)Square.C6, (int)Square.E6, (int)Square.C5,
                (int)Square.D5, (int)Square.E5
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LowerBorderRightCorner()
        {
            var board = CongoBoard.Empty;
            var actual = board.LeapsAsLion(Black.Color, (int)Square.E5).Sort();
            var expected = new int[] {
                (int)Square.D5, (int)Square.D6, (int)Square.E6
            }.ToImmutableArray().Sort();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
