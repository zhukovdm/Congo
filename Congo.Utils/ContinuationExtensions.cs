using System;

namespace Congo.Utils
{
    public static class ContinuationExtensions
    {
        /// <summary>
        /// Left-associative ?? operator.
        /// </summary>
        public static T AndThen<T>(this T t, Func<T, T> func)
            => t ?? func(t);
    }
}
