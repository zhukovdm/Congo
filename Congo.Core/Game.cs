using System;
using System.Collections.Generic;

namespace Congo.Core
{
    /// <summary>
    /// Central object defines the current game.
    /// </summary>
    public class CongoGame
    {
        public static void Initialize()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CongoColor).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(White).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Black).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CongoPiece).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Ground).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(River).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Elephant).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Zebra).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Giraffe).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Crocodile).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Pawn).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Superpawn).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Lion).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Monkey).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CongoBoard).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CongoHashTable).TypeHandle);
        }

        #region Unattached game

        /// <summary>
        /// Unattached game is a game without predecessor. An example of
        /// such game is any game deserialized from Fen string.
        /// </summary>
        public static CongoGame Unattached(CongoBoard board, CongoPlayer whitePlayer,
            CongoPlayer blackPlayer, CongoPlayer activePlayer, MonkeyJump firstMonkeyJump)
        {
            return new CongoGame(null, null, board, whitePlayer, blackPlayer,
                activePlayer, firstMonkeyJump);
        }

        #endregion

        #region Standard game

        private static CongoBoard SetMixedRank(CongoBoard board, CongoColor color, int rank)
        {
            board = board.With(color, Giraffe.Piece,   rank * board.Size + 0)
                         .With(color, Monkey.Piece,    rank * board.Size + 1)
                         .With(color, Elephant.Piece,  rank * board.Size + 2)
                         .With(color, Lion.Piece,      rank * board.Size + 3)
                         .With(color, Elephant.Piece,  rank * board.Size + 4)
                         .With(color, Crocodile.Piece, rank * board.Size + 5)
                         .With(color, Zebra.Piece,     rank * board.Size + 6);

            return board;
        }

        private static CongoBoard SetPawnRank(CongoBoard board, CongoColor color, int rank)
        {
            for (int file = 0; file < board.Size; ++file) {
                board = board.With(color, Pawn.Piece, rank * board.Size + file);
            }

            return board;
        }

        /// <summary>
        /// Standard game is the starting position with full animal packs of
        /// both, White and Black, colors.
        /// </summary>
        public static CongoGame Standard(Type whitePlayerType, Type blackPlayerType)
        {
            if ((whitePlayerType != typeof(Ai) && whitePlayerType != typeof(Hi)) ||
                (blackPlayerType != typeof(Ai) && blackPlayerType != typeof(Hi))) { return null; }

            var b = CongoBoard.Empty;
            b = SetMixedRank(b, Black.Color, 0);
            b = SetPawnRank (b, Black.Color, 1);
            b = SetPawnRank (b, White.Color, 5);
            b = SetMixedRank(b, White.Color, 6);

            var playerFactory = new Dictionary<Type, Func<CongoColor, CongoPlayer>>
            {
                { typeof(Ai), (CongoColor color) => { return new Ai(color, b, null); } },
                { typeof(Hi), (CongoColor color) => { return new Hi(color, b, null); } }
            };

            // proper indexing is ensured above
            var wp = playerFactory[whitePlayerType](White.Color);
            var bp = playerFactory[blackPlayerType](Black.Color);

            return Unattached(b, wp, bp, wp, null);
        }

        #endregion

        private readonly int distance;
        private readonly CongoGame predecessor;
        private readonly CongoMove transitionMove;
        private readonly CongoBoard board;
        private readonly CongoPlayer whitePlayer;
        private readonly CongoPlayer blackPlayer;
        private readonly CongoPlayer activePlayer;
        private readonly MonkeyJump firstMonkeyJump;

        private bool IsInterruptedMonkeyJump(CongoMove move)
            => board.GetPiece(move.Fr).IsMonkey() && move.Fr == move.To;

        private bool IsPawnPromotion(CongoMove move)
        {
            return board.GetPiece(move.Fr).IsPawn() &&
                board.IsUpDownBorder(activePlayer.Color, move.To);
        }

        private bool IsFriendlyAnimal(CongoPiece piece, CongoColor color)
            => piece.IsAnimal() && color == activePlayer.Color;

        /// <summary>
        /// Game constructor is private. Game is either unattached, standard
        /// or generated by a Transition.
        /// </summary>
        private CongoGame(CongoGame predecessor, CongoMove transitionMove,
            CongoBoard board, CongoPlayer whitePlayer, CongoPlayer blackPlayer,
            CongoPlayer activePlayer, MonkeyJump firstMonkeyJump)
        {
            distance = predecessor == null ? 0 : predecessor.distance + 1;
            this.predecessor = predecessor;
            this.transitionMove = transitionMove;
            this.firstMonkeyJump = firstMonkeyJump; // jump of the active player
            this.board = board;
            this.activePlayer = activePlayer;
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;
        }

        public CongoGame Predecessor => predecessor;

        public CongoMove TransitionMove => transitionMove;

        public CongoBoard Board => board;

        public CongoPlayer WhitePlayer => whitePlayer;

        public CongoPlayer BlackPlayer => blackPlayer;

        public CongoPlayer ActivePlayer
            => activePlayer.Color.IsWhite() ? whitePlayer : blackPlayer;

        public CongoPlayer Opponent
            => activePlayer.Color.IsWhite() ? blackPlayer : whitePlayer;

        public CongoMove FirstMonkeyJump => firstMonkeyJump;

        /// <summary>
        /// Generates new game based on current board, players, current player
        /// and a given move. <b>Move must be proven valid</b>. Malformed
        /// moves are not checked and method call has an undefined behavior.
        /// </summary>
        public CongoGame Transition(CongoMove move)
        {
            var newBoard = board;
            CongoPlayer newWhitePlayer;
            CongoPlayer newBlackPlayer;
            CongoColor newActivePlayerColor = activePlayer.Color.Invert(); // speculative inversion
            MonkeyJump newFirstMonkeyJump = firstMonkeyJump;

            #region Execute move. Define newBoard, newFirstMonkeyJump, newActivePlayerColor.

            // first or consecutive monkey jump
            if (move is MonkeyJump jump) {

                newBoard = newBoard.With(activePlayer.Color, newBoard.GetPiece(jump.Fr), jump.To)
                                   .Without(jump.Bt)
                                   .Without(jump.Fr);

                if (newFirstMonkeyJump == null) { newFirstMonkeyJump = jump; }

                // the color remains unchanged -> revert speculative inversion
                newActivePlayerColor = newActivePlayerColor.Invert();
            }

            // interrupted monkey jump
            else if (IsInterruptedMonkeyJump(move)) { newFirstMonkeyJump = null; }

            // pawn -> superpawn promotion
            else if (IsPawnPromotion(move)) {
                newBoard = newBoard.With(activePlayer.Color, Superpawn.Piece, move.To)
                                   .Without(move.Fr);
            }

            // ordinary move
            else {
                newBoard = newBoard.With(activePlayer.Color, newBoard.GetPiece(move.Fr), move.To)
                                   .Without(move.Fr);
            }

            #endregion

            #region Drowning. Define newBoard.

            for (int square = (int)Square.A4; square <= (int)Square.G4; ++square) {

                var piece = newBoard.GetPiece(square);
                var color = newBoard.IsWhitePiece(square) ? White.Color : Black.Color;

                // consider only friendly non-crocodiles
                if (!IsFriendlyAnimal(piece, color) || piece.IsCrocodile()) { }

                // not-moved piece -> stayed at the river -> drown
                else if (move.To != square) { newBoard = newBoard.Without(square); }

                /* from now onwards move.To == square */

                // ground-to-river move
                else if (!newBoard.IsRiver(move.Fr)) { }

                // ordinary non-monkey river-to-river move -> drown
                else if (!piece.IsMonkey() && newBoard.IsRiver(move.Fr)) {
                    newBoard = newBoard.Without(square);
                }

                /* from now onwards monkey river-to-river move */

                // interrupted monkey jump
                else if (move.Fr == move.To) {

                    // started from the river -> drown
                    if (newBoard.IsRiver(firstMonkeyJump.Fr)) {
                        newBoard = newBoard.Without(square);
                    }
                    
                    // otherwise -> remains on the board
                    else { }
                }

                // continued monkey jump
                else if (move is MonkeyJump) { }

                // ordinary monkey river-to-river move -> drown
                else { newBoard = newBoard.Without(square); }
            }

            #endregion

            #region Finalize. Define newWhitePlayers, newBlackPlayer, newActivePlayer.

            var newWhiteMonkeyJumps = newActivePlayerColor.IsWhite()
                ? newFirstMonkeyJump : null;

            var newBlackMonkeyJumps = newActivePlayerColor.IsBlack()
                ? newFirstMonkeyJump : null;

            newWhitePlayer = whitePlayer.With(newBoard, newWhiteMonkeyJumps);
            newBlackPlayer = blackPlayer.With(newBoard, newBlackMonkeyJumps);

            var newActivePlayer = newActivePlayerColor.IsWhite()
                ? newWhitePlayer : newBlackPlayer;

            #endregion

            return new CongoGame(this, move, newBoard, newWhitePlayer,
                newBlackPlayer, newActivePlayer, newFirstMonkeyJump);
        }

        public bool IsNew() => distance == 0;

        public bool IsInvalid() => !ActivePlayer.HasLion && !Opponent.HasLion;

        public bool IsWin()
        {
            return (ActivePlayer.HasLion && !Opponent.HasLion) ||
                (!ActivePlayer.HasLion && Opponent.HasLion);
        }

        public bool HasEnded() => IsInvalid() || IsWin();
    }
}
