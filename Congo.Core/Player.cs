using System.Collections.Generic;
using System.Collections.Immutable;

namespace Congo.Core
{
	public enum PlayerCode : int
	{
		AI, HI
	}

	public static class PlayerCodeExtensions
	{
		public static bool IsHI(this PlayerCode player)
			=> player == PlayerCode.HI;
	}

	public abstract class CongoPlayer
	{
		protected readonly ImmutableArray<CongoMove> moves;
		
		protected static ImmutableArray<CongoMove> GenerateMoves(ColorCode color, CongoBoard board)
		{
			var allMoves = new List<CongoMove>();
			var e = board.GetEnumerator(color);
			
			while (e.MoveNext()) {
				var piece = board.GetPiece(e.Current);
				allMoves.AddRange(piece.GetMoves(color, board, e.Current));
			}

			return allMoves.ToImmutableArray();
		}

		public CongoPlayer(ColorCode color, CongoBoard board)
		{
			moves = GenerateMoves(color, board);
		}

		public ImmutableArray<CongoMove> Moves => moves;

		public abstract PlayerCode Code { get; }

		public abstract CongoPlayer With(ColorCode color, CongoBoard board);
	}

	sealed class AIPlayer : CongoPlayer
	{
		public AIPlayer(ColorCode color, CongoBoard board)
			: base(color, board) { }

		public override PlayerCode Code => PlayerCode.AI;

		public override CongoPlayer With(ColorCode color, CongoBoard board)
			=> new AIPlayer(color, board);
	}

	sealed class HIPlayer : CongoPlayer
	{
		public HIPlayer(ColorCode color, CongoBoard board)
			:base(color, board) { }

		public override PlayerCode Code => PlayerCode.HI;

		public override CongoPlayer With(ColorCode color, CongoBoard board)
			=> new HIPlayer(color, board);
	}

	static class CongoPlayerFactory
	{
		private delegate CongoPlayer D(ColorCode color, CongoBoard board);

		private static CongoPlayer GetPlayerInstance(ColorCode color, CongoBoard board, PlayerCode player)
		{
			if (player.IsHI()) return new HIPlayer(color, board);
			else			   return new AIPlayer(color, board);
		}

		public static CongoPlayer GetInstance(ColorCode color, CongoBoard board, PlayCommand command)
		{
			if (color.IsWhite()) return GetPlayerInstance(color, board, command.White);
			else				 return GetPlayerInstance(color, board, command.Black);
		}
	}
}
