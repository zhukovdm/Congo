using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Core.MSTest
{
    [TestClass]
    public class FromFen
    {
        [TestMethod]
        public void Null()
            => Assert.IsNull(CongoFen.FromFen(null));

        [TestMethod]
        public void Length()
            => Assert.IsNull(CongoFen.FromFen("3l3/7/7/7/7/7/3L3/7/w/-1"));

        [TestMethod]
        public void TwoLions()
        {
            var game = CongoFen.FromFen("3l3/7/7/7/7/7/3L3/w/-1");
            Assert.IsTrue(game.Board.GetPiece((int)Square.D7).IsLion() && game.Board.IsBlackPiece((int)Square.D7) &&
                          game.Board.GetPiece((int)Square.D1).IsLion() && game.Board.IsWhitePiece((int)Square.D1) &&
                          game.ActivePlayer.Color.IsWhite());
        }

        [TestMethod]
        public void InvalidShortRankUnderflow()
            => Assert.IsNull(CongoFen.FromFen("3l2/7/7/7/7/7/3L3/w/-1"));

        [TestMethod]
        public void InvalidLongRankOverflow()
            => Assert.IsNull(CongoFen.FromFen("3l3/8/7/7/7/7/3L3/w/-1"));

        [TestMethod]
        public void InvalidRankPiecesOnRankOverflow()
            => Assert.IsNull(CongoFen.FromFen("7/7/7/7/7/7/3L3P/w/-1"));
    }

    [TestClass]
    public class ToFen
    {
        [TestMethod]
        public void Standard()
        {
            var game = CongoGame.Standard();
            var actual = CongoFen.ToFen(game);
            var expected = "gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/w/-1";
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void Empty()
        {
            var board = CongoBoard.Empty;
            var white = new CongoPlayer(White.Color, board, null);
            var black = new CongoPlayer(Black.Color, board, null);
            var game = CongoGame.Unattached(board, white, black, black, null);
            var actual = CongoFen.ToFen(game);
            var expected = "7/7/7/7/7/7/7/b/-1";
            Assert.IsTrue(expected == actual);
        }

        [TestMethod]
        public void WithFirstMonkeyJump()
        {
            var board = CongoBoard.Empty;
            var white = new CongoPlayer(White.Color, board, null);
            var black = new CongoPlayer(Black.Color, board, null);
            var game = CongoGame.Unattached(board, white, black, white, new MonkeyJump(5, -1, -1));
            var actual = CongoFen.ToFen(game);
            var expected = "7/7/7/7/7/7/7/w/5";
            Assert.IsTrue(expected == actual);
        }
    }
}
