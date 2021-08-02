using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Congo.Core
{
	public abstract class CongoPlayer
	{
		public static CongoPlayer GetByType(CongoBoard board, Type playerType,
			CongoColor color, MonkeyJump firstMonkeyJump)
		{
			if (playerType == typeof(Ai)) {
				return new Ai(color, board, firstMonkeyJump);
			} else if (playerType == typeof(Hi)) {
				return new Hi(color, board, firstMonkeyJump);
			} else {
				throw new InvalidOperationException($"Type {playerType} is not supported.");
			}
		}

		protected readonly int lionSquare;
		protected readonly bool hasNonLion;
		protected readonly CongoColor color;
		protected readonly ImmutableArray<CongoMove> moves;

		public CongoPlayer(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
		{
			this.color = color;
			lionSquare = -1;
			hasNonLion = false;
			var allMoves = new List<CongoMove>();
			var e = board.GetEnumerator(color);

			while (e.MoveNext()) {
				var piece = board.GetPiece(e.Current);

				if (piece.IsLion()) lionSquare = e.Current;
				if (piece.IsAnimal() && !piece.IsLion()) hasNonLion = true;

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

		public bool HasNonLion => hasNonLion;

		public CongoColor Color => color;

		public abstract CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump);

		public abstract CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game);

		public bool LionInDanger(ImmutableArray<CongoMove> opponentMoves)
		{
			foreach (var move in opponentMoves) {
				if (move.To == lionSquare) { return true; }
			}

			return false;
		}
	}

	public sealed class Ai : CongoPlayer
	{
		public Ai(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
			: base(color, board, firstMonkeyJump) { }

		public override CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)
			=> new Ai(color, board, firstMonkeyJump);

		public override CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game)
			=> Algorithm.Negamax(game);
	}

	public sealed class Hi : CongoPlayer
	{
		private CongoMove find(CongoMove candidateMove)
		{
			var query = from validMove in Moves
						where candidateMove == validMove
						select validMove;

			/* candidate move must be replaced because of the monkey jumps */

			foreach (var move in query) return move;

			return null;
		}

		public Hi(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
			:base(color, board, firstMonkeyJump) { }

		public override CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)
			=> new Hi(color, board, firstMonkeyJump);

		public override CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game)
		{
			CongoMove move;

			do {
				move = ui.GetHiMove(game);
				move = find(move);
				if (move == null) ui.ReportWrongHiMove();
			} while (move == null);
			
			return move;
		}
	}
}
