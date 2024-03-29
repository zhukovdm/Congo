﻿namespace Congo.Core
{
    public abstract class CongoUser
    {
        protected AlgorithmDelegate algo;

        public CongoUser(AlgorithmDelegate algo)
        {
            this.algo = algo;
        }

        public virtual CongoMove Advise(CongoGame game) => algo.Invoke(game);
    }

    public sealed class Ai : CongoUser
    {
        public Ai(AlgorithmDelegate algo) : base(algo) { }
    }

    public sealed class Hi : CongoUser
    {
        public Hi(AlgorithmDelegate algo) : base(algo) { }
    }

    public sealed class Net : CongoUser
    {
        public Net(AlgorithmDelegate algo) : base(algo) { }
    }
}
