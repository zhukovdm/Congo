using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Utils.MSTest
{
    [TestClass]
    public class BoardIdValidation
    {
        [TestMethod]
        public void Null()
            => Assert.IsFalse(UserInput.IsValidGameId(null));

        [TestMethod]
        public void Empty()
            => Assert.IsFalse(UserInput.IsValidGameId(""));

        [TestMethod]
        public void NonDigitChar()
            => Assert.IsFalse(UserInput.IsValidGameId("12a"));

        [TestMethod]
        public void TrailingZero()
            => Assert.IsFalse(UserInput.IsValidGameId("01"));

        [TestMethod]
        public void Valid()
            => Assert.IsTrue(UserInput.IsValidGameId("123"));
    }

    [TestClass]
    public class HostValidation
    {
        [TestMethod]
        public void Null()
            => Assert.IsFalse(UserInput.IsValidHost(null));

        [TestMethod]
        public void Empty()
            => Assert.IsFalse(UserInput.IsValidHost(""));

        [TestMethod]
        public void OnlyDots()
            => Assert.IsFalse(UserInput.IsValidHost("..."));

        [TestMethod]
        public void Short()
            => Assert.IsFalse(UserInput.IsValidHost("1.2.3"));

        [TestMethod]
        public void Long()
            => Assert.IsFalse(UserInput.IsValidHost("1.2.3.4.5"));

        [TestMethod]
        public void EmptyHolder()
            => Assert.IsFalse(UserInput.IsValidHost("1.2..4"));

        [TestMethod]
        public void LongHolder()
            => Assert.IsFalse(UserInput.IsValidHost("1.2.1234.4"));

        [TestMethod]
        public void TrailingZeroHolder()
            => Assert.IsFalse(UserInput.IsValidHost("1.2.03.4"));

        [TestMethod]
        public void HolderWithNonDigitChar()
            => Assert.IsFalse(UserInput.IsValidHost("1.2?2.3.4"));

        [TestMethod]
        public void OutBoundHolder()
            => Assert.IsFalse(UserInput.IsValidHost("1.2.256.4"));

        [TestMethod]
        public void ValidGeneralIp()
            => Assert.IsTrue(UserInput.IsValidHost("127.0.0.1"));

        [TestMethod]
        public void ValidLocalhostIp()
            => Assert.IsTrue(UserInput.IsValidHost("localhost"));

        [TestMethod]
        public void ValidLowerBound()
            => Assert.IsTrue(UserInput.IsValidHost("0.0.0.0"));

        [TestMethod]
        public void ValidUpperBound()
            => Assert.IsTrue(UserInput.IsValidHost("255.255.255.255"));
    }

    [TestClass]
    public class PortValidation
    {
        [TestMethod]
        public void Null()
            => Assert.IsFalse(UserInput.IsValidPort(null));

        [TestMethod]
        public void Empty()
            => Assert.IsFalse(UserInput.IsValidPort(""));

        [TestMethod]
        public void Short()
            => Assert.IsFalse(UserInput.IsValidPort("123"));

        [TestMethod]
        public void Long()
            => Assert.IsFalse(UserInput.IsValidPort("123456"));

        [TestMethod]
        public void TrailingZero()
            => Assert.IsFalse(UserInput.IsValidPort("0456"));

        [TestMethod]
        public void NonDigitChar()
            => Assert.IsFalse(UserInput.IsValidPort("32a4"));

        [TestMethod]
        public void BelowLowerBound()
            => Assert.IsFalse(UserInput.IsValidPort("1023"));

        [TestMethod]
        public void LowerBound()
            => Assert.IsTrue(UserInput.IsValidPort("1024"));

        [TestMethod]
        public void Middle()
            => Assert.IsTrue(UserInput.IsValidPort("10745"));

        [TestMethod]
        public void UpperBound()
            => Assert.IsTrue(UserInput.IsValidPort("65535"));

        [TestMethod]
        public void AboveUpperBound()
            => Assert.IsFalse(UserInput.IsValidPort("65536"));
    }
}
