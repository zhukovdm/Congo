using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Congo.Core
{
    public abstract class CongoPlayer
    {
        protected readonly int lionSquare;
        protected readonly CongoColor color;
        protected readonly ImmutableArray<CongoMove> moves;

        public CongoPlayer(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
        {
            lionSquare = -1;
            this.color = color;
            var allMoves = new List<CongoMove>();
            var e = board.GetEnumerator(color);

            while (e.MoveNext()) {
                var piece = board.GetPiece(e.Current);

                if (piece.IsLion()) { lionSquare = e.Current; }

                // ordinary move
                if (firstMonkeyJump == null) {
                    allMoves.AddRange(piece.GetMoves(color, board, e.Current));
                }

                // monkey jump -> only monkey moves
                else {
                    if (piece.IsMonkey()) {
                        var monkey = (Monkey)piece;
                        allMoves.AddRange(monkey.ContinueJump(color, board, e.Current));
                    }
                }
            }

            moves = allMoves.ToImmutableArray();
        }

        public ImmutableArray<CongoMove> Moves => moves;

        public bool HasLion => lionSquare >= 0;

        public CongoColor Color => color;

        public abstract CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump);

        public bool LionInDanger(ImmutableArray<CongoMove> opponentMoves)
        {
            foreach (var move in opponentMoves) {
                if (move.To == lionSquare) { return true; }
            }

            return false;
        }

        public CongoMove Accept(CongoMove candidateMove)
        {
            var query = from validMove in Moves
                        where candidateMove == validMove
                        select validMove;

            // candidate move must be replaced because of the monkey jumps
            foreach (var move in query) { return move; }

            return null;
        }
    }

    public sealed class Ai : CongoPlayer
    {
        public Ai(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
            : base(color, board, firstMonkeyJump) { }

        public override CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)
            => new Ai(color, board, firstMonkeyJump);
    }

    public sealed class Hi : CongoPlayer
    {
        public Hi(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
            : base(color, board, firstMonkeyJump) { }

        public override CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)
            => new Hi(color, board, firstMonkeyJump);
    }
}
