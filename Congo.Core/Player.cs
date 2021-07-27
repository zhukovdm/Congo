using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Congo.Core
{
	public abstract class CongoPlayer
	{
		protected readonly ImmutableArray<CongoMove> moves;
		protected readonly bool hasLion;
		protected readonly bool hasNonLion;
		protected readonly CongoColor color;

		public CongoPlayer(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
		{
			this.color = color;
			hasLion = false;
			hasNonLion = false;
			var allMoves = new List<CongoMove>();
			var e = board.GetEnumerator(color);

			while (e.MoveNext()) {
				var piece = board.GetPiece(e.Current);

				if (piece.IsLion()) hasLion = true;
				if (piece.IsAnimal() && !piece.IsLion()) hasNonLion = true;

				if (firstMonkeyJump == null) { // ordinary move
					allMoves.AddRange(piece.GetMoves(color, board, e.Current));
				} else { // monkey jump
					if (piece.IsMonkey()) {
						var monkey = (Monkey)piece;
						allMoves.AddRange(monkey.ContinueJump(color, board, e.Current));
						break; // there is only one monkey
					}
				}
			}

			moves = allMoves.ToImmutableArray();
		}

		public ImmutableArray<CongoMove> Moves => moves;

		public bool HasLion => hasLion;

		public bool HasNonLion => hasNonLion;

		public CongoColor Color => color;

		public abstract CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump);

		public abstract CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game);
	}

	public sealed class Ai : CongoPlayer
	{
		public Ai(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJumps)
			: base(color, board, firstMonkeyJumps) { }

		public override CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJumps)
			=> new Ai(color, board, firstMonkeyJumps);

		public override CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game)
			=> Algorithm.Rnd(game);
	}

	public sealed class Hi : CongoPlayer
	{
		private CongoMove find(CongoMove candidateMove)
		{
			var query = from validMove in Moves
						where CongoMove.AreEqual(candidateMove, validMove)
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
