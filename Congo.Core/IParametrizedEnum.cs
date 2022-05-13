namespace Congo.Core
{
    public interface IParametrizedEnumerator<T>
    {
        T Current { get; }
        bool MoveNext();
    }

    public interface IParametrizedEnumerable<T, U>
    {
        IParametrizedEnumerator<U> GetEnumerator(T param);
    }
}
