namespace Congo.Core
{
    public abstract class CongoUser
    {
        protected AlgorithmDelegate algo;
        public CongoMove Advice(CongoGame game) => algo.Invoke(game);
    }

    public class Ai : CongoUser
    {
        public Ai(AlgorithmDelegate algo)
        {
            this.algo = algo;
        }
    }

    public class Hi : CongoUser
    {
        public Hi(AlgorithmDelegate algo)
        {
            this.algo = algo;
        }
    }
}
