﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Congo.Core
{
    /// <summary>
    /// This class generates and keeps possible player moves and accepts
    /// possible moves, generated by the human users via user interface.
    /// </summary>
    public class CongoPlayer
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
                        allMoves.AddRange(Monkey.ContinueJump(color, board, e.Current));
                    }
                }
            }

            moves = allMoves.ToImmutableArray();
        }

        public ImmutableArray<CongoMove> Moves => moves;

        public bool HasLion => lionSquare >= 0;

        public CongoColor Color => color;

        public bool IsWhite() => color.IsWhite();

        public CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)
            => new(color, board, firstMonkeyJump);

        public bool LionInDanger(ImmutableArray<CongoMove> opponentMoves)
        {
            foreach (var move in opponentMoves) {
                if (move.To == lionSquare) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Verifies if candidate move (from, to) exists and possibly replaces
        /// with MonkeyJump. Otherwise, returns null.
        /// </summary>
        public CongoMove Accept(CongoMove candidateMove)
        {
            if (candidateMove == null) { return null; }

            var query = from validMove in Moves
                        where candidateMove == validMove
                        select validMove;

            // candidate move must be replaced because of the monkey jumps
            foreach (var move in query) { return move; }

            return null;
        }
    }
}
