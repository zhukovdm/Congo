using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Congo.Core
{
	public abstract class CongoPlayer
	{
		protected readonly ImmutableArray<CongoMove> moves;
		public readonly bool HasLion;
		public readonly bool HasNonLion;

		protected static ImmutableArray<CongoMove> GenerateMoves(CongoColor color,
			CongoBoard board, ImmutableList<int> monkeyCaptures, ref bool hasLion,
			ref bool hasNonLion)
		{
			hasLion = hasNonLion = false;
			var allMoves = new List<CongoMove>();
			var e = board.GetEnumerator(color);

			while (e.MoveNext()) {
				var piece = board.GetPiece(e.Current);
				if (piece.IsLion()) hasLion = true;
				if (piece.IsAnimal() && !piece.IsLion()) hasNonLion = true;
				if (monkeyCaptures == null) {               /* ordinary move */
					allMoves.AddRange(piece.GetMoves(color, board, e.Current));
				} else {                                      /* monkey jump */
					if (piece.IsMonkey()) {
						var monkey = (Monkey)piece;
						allMoves.AddRange(monkey.ContinueJump(color, board, e.Current));
					}
				}
			}

			return allMoves.ToImmutableArray();
		}

		public CongoPlayer(CongoColor color, CongoBoard board, ImmutableList<int> monkeyCaptures)
		{
			moves = GenerateMoves(color, board, monkeyCaptures, ref HasLion, ref HasNonLion);
		}

		public ImmutableArray<CongoMove> Moves => moves;

		public abstract CongoPlayer With(CongoColor color, CongoBoard board,
			ImmutableList<int> monkeyCaptures);

		public abstract CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game);
	}

	public sealed class AI : CongoPlayer
	{
		public AI(CongoColor color, CongoBoard board, ImmutableList<int> monkeyCaptures)
			: base(color, board, monkeyCaptures) { }

		public override CongoPlayer With(CongoColor color, CongoBoard board, ImmutableList<int> monkeyCaptures)
			=> new AI(color, board, monkeyCaptures);

		public override CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game)
			=> Algorithm.Rnd(game);
	}

	public sealed class HI : CongoPlayer
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

		public HI(CongoColor color, CongoBoard board, ImmutableList<int> monkeyCaptures)
			:base(color, board, monkeyCaptures) { }

		public override CongoPlayer With(CongoColor color, CongoBoard board, ImmutableList<int> monkeyCaptures)
			=> new HI(color, board, monkeyCaptures);

		public override CongoMove GetValidMove(ICongoUserInterface ui, CongoGame game)
		{
			CongoMove move;

			do {
				move = ui.GetHIMove(game);
				move = find(move);
				if (move == null) ui.ReportWrongHIMove();
			} while (move == null);
			
			return move;
		}
	}
}
