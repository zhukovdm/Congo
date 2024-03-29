﻿namespace Congo.Core
{
    public abstract class CongoColor
    {
        public static bool operator ==(CongoColor c1, CongoColor c2)
        {
            if (c1 is null && c2 is null) { return true;  }
            if (c1 is null || c2 is null) { return false; }

            return c1.Id == c2.Id;
        }

        public static bool operator !=(CongoColor c1, CongoColor c2) => !(c1 == c2);

        private protected enum ColorId : int
        {
            White, Black
        }

        private protected abstract ColorId Id { get; }

        /// <summary>
        /// Codes shall not be visible outside .Core project.
        /// </summary>
        internal int Code => (int)Id;

        public bool IsWhite() => Id == ColorId.White;

        public bool IsBlack() => Id == ColorId.Black;

        public CongoColor Invert() => IsWhite() ? Black.Color : White.Color;

        public override bool Equals(object o) => this == (CongoColor)o;

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
