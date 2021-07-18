namespace Congo.Core
{
	public enum ColorCode : int
	{
		White, Black
	}

	public static class ColorCodeExtensions
	{
		public static bool IsWhite(this ColorCode color)
			=> color == ColorCode.White;

		public static bool IsBlack(this ColorCode color)
			=> color == ColorCode.Black;

		public static ColorCode Invert(this ColorCode color)
			=> color == ColorCode.White ? ColorCode.Black : ColorCode.White;
	}
}
