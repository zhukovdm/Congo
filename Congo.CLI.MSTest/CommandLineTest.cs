using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.CLI.MSTest
{
    [TestClass]
    public class CongoArgsParserTest
    {
        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void EmptyArgs()
        {
            CongoArgs.Parser.Parse(System.Array.Empty<string>());
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void RepeatedOption()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=standard",
                "--white=hi/random",
                "--white=hi/random",
                "--black=ai/negamax",
            });
        }

        //

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void EmptyPlace()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=",
                "--board=standard",
                "--white=hi/random",
                "--black=ai/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedPlace()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=word",
                "--board=standard",
                "--white=hi/random",
                "--black=ai/negamax",
            });
        }

        //

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void EmptyLocalBoard()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=",
                "--white=hi/random",
                "--black=ai/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalBoardRandom()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=new",
                "--white=hi/random",
                "--black=ai/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalBoardId()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/random",
                "--black=ai/negamax",
            });
        }

        //

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalWhiteEmpty()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=",
                "--black=ai/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalWhiteOnlyKind()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/",
                "--black=ai/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalWhiteOnlyAlgorithm()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=/random",
                "--black=ai/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalWhiteKind()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=xx/random",
                "--black=ai/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalWhiteAlgorithm()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/xx",
                "--black=ai/negamax",
            });
        }

        //

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalBlackEmpty()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/random",
                "--black=",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalBlackOnlyKind()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/random",
                "--black=ai/",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalBlackOnlyAlgorithm()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/random",
                "--black=/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalBlackKind()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/random",
                "--black=xx/negamax",
            });
        }

        [TestMethod, ExpectedException(typeof(System.ArgumentException))]
        public void MalformedLocalBlackAlgorithm()
        {
            CongoArgs.Parser.Parse(new string[] {
                "--place=local",
                "--board=123",
                "--white=hi/random",
                "--black=xx/negamax",
            });
        }

        //

        [TestMethod]
        public void GameIsLocal()
        {
            var args = new string[] {
                "--place=local",
                "--board=standard",
                "--white=hi/random",
                "--black=ai/negamax",
            };
            Assert.IsTrue(CongoArgs.Parser.Parse(args).IsGameLocal());
        }

        [TestMethod]
        public void GameIsNetwork()
        {
            var args = new string[] {
                "--place=network",
                "--host=127.0.0.1",
                "--port=4765",
                "--board=standard",
                "--white=hi/random",
            };
            Assert.IsTrue(CongoArgs.Parser.Parse(args).IsGameNetwork());
        }
    }
}
