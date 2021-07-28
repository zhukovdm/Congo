namespace Congo.Core
{
	public abstract class CongoColor
	{
		public static bool operator ==(CongoColor c1, CongoColor c2)
			=> c1.Id == c2.Id;

		public static bool operator !=(CongoColor c1, CongoColor c2)
			=> !(c1 == c2);

		private protected enum ColorId : int
		{
			White, Black
		}

		private protected abstract ColorId Id { get; }

		public bool IsWhite() => Id == ColorId.White;

		public bool IsBlack() => Id == ColorId.Black;

		public CongoColor Invert() => IsWhite() ? Black.Color : White.Color;

		public override bool Equals(object o) => Id == ((CongoColor)o).Id;

		public override int GetHashCode() => base.GetHashCode();
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
