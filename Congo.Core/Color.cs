namespace Congo.Core
{
	public abstract class CongoColor
	{
		private protected enum ColorId : int
		{
			White, Black
		}

		private protected abstract ColorId Id { get; }

		public bool IsWhite() => Id == ColorId.White;

		public bool IsBlack() => Id == ColorId.Black;

		public CongoColor Invert() => IsWhite() ? Black.Color : White.Color;

		public bool Equals(CongoColor color)
		{
			return Id == color.Id;
		}
	}

	public sealed class White : CongoColor
	{
		public static CongoColor Color { get; } = new White();

		private White() { }

		private protected override ColorId Id { get; } = ColorId.White;
	}

	public sealed class Black : CongoColor
	{
		public static CongoColor Color { get; } = new Black();

		private Black() { }

		private protected override ColorId Id { get; } = ColorId.Black;
	}
}
