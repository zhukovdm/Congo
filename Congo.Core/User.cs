namespace Congo.Core
{
    public abstract class CongoUser
    {
        protected AlgorithmDelegate algo;
        public CongoMove Advise(CongoGame game) => algo.Invoke(game);
    }

    public sealed class Ai : CongoUser
    {
        public Ai(AlgorithmDelegate algo)
        {
            this.algo = algo;
        }
    }

    public sealed class Hi : CongoUser
    {
        public Hi(AlgorithmDelegate algo)
        {
            this.algo = algo;
        }
    }
}
