namespace Congo.Core
{
    /// <summary>
    /// Partial definition of the standard IEnumerator extended by an output
    /// parameter of type T.
    /// </summary>
    public interface IParametrizedEnumerator<out T>
    {
        T Current { get; }
        bool MoveNext();
    }

    /// <summary>
    /// Definition of the standard generic IEnumerable extended by an input
    /// parameter of type T.
    /// </summary>
    public interface IParametrizedEnumerable<in T, out U>
    {
        IParametrizedEnumerator<U> GetEnumerator(T param);
    }
}
