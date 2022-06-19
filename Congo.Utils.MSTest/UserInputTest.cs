using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Congo.Utils.MSTest
{
    [TestClass]
    public class UserNameValidation
    {
        [TestMethod]
        public void EmptyName()
            => Assert.IsFalse(UserInput.IsUserNameValid(""));

        [TestMethod]
        public void SpecialChar()
            => Assert.IsFalse(UserInput.IsUserNameValid("abc?"));

        [TestMethod]
        public void ValidName()
            => Assert.IsTrue(UserInput.IsUserNameValid("abc"));
    }

    [TestClass]
    public class IpAddressHolderValidation
    {
        [TestMethod]
        public void EmptyHolder()
            => Assert.IsFalse(UserInput.IsIpAddressHolderValid(""));

        [TestMethod]
        public void LongHolder()
            => Assert.IsFalse(UserInput.IsIpAddressHolderValid("1234"));

        [TestMethod]
        public void NonDigitChars()
            => Assert.IsFalse(UserInput.IsIpAddressHolderValid("1O2"));

        [TestMethod]
        public void OutBound()
            => Assert.IsFalse(UserInput.IsIpAddressHolderValid("256"));

        [TestMethod]
        public void ValidLowerBound()
            => Assert.IsTrue(UserInput.IsIpAddressHolderValid("000"));

        [TestMethod]
        public void ValidMiddle()
            => Assert.IsTrue(UserInput.IsIpAddressHolderValid("123"));

        [TestMethod]
        public void ValidMiddleLeadingZeros()
            => Assert.IsTrue(UserInput.IsIpAddressHolderValid("023"));

        [TestMethod]
        public void ValidUpperBound()
            => Assert.IsTrue(UserInput.IsIpAddressHolderValid("255"));
    }

    [TestClass]
    public class IpAddressValidation
    {
        [TestMethod]
        public void Empty()
            => Assert.IsFalse(UserInput.IsIpAddressValid(""));

        [TestMethod]
        public void OnlyDots()
            => Assert.IsFalse(UserInput.IsIpAddressValid("..."));

        [TestMethod]
        public void Short()
            => Assert.IsFalse(UserInput.IsIpAddressValid("1.2.3"));

        [TestMethod]
        public void Long()
            => Assert.IsFalse(UserInput.IsIpAddressValid("1.2.3.4.5"));

        [TestMethod]
        public void WithBadHolder()
            => Assert.IsFalse(UserInput.IsIpAddressValid("1.?.3.4"));

        [TestMethod]
        public void Valid()
            => Assert.IsTrue(UserInput.IsIpAddressValid("127.0.0.1"));
    }

    [TestClass]
    public class PortValidation
    {
        [TestMethod]
        public void Empty()
            => Assert.IsFalse(UserInput.IsPortValid(""));

        [TestMethod]
        public void Short()
            => Assert.IsFalse(UserInput.IsPortValid("012"));

        [TestMethod]
        public void Long()
            => Assert.IsFalse(UserInput.IsPortValid("123456"));

        [TestMethod]
        public void SmallLeadingZero()
            => Assert.IsFalse(UserInput.IsPortValid("0456"));

        [TestMethod]
        public void LowerBound()
            => Assert.IsTrue(UserInput.IsPortValid("1024"));

        [TestMethod]
        public void LowerBoundWithLeadingZero()
            => Assert.IsTrue(UserInput.IsPortValid("01024"));

        [TestMethod]
        public void Middle()
            => Assert.IsTrue(UserInput.IsPortValid("10745"));

        [TestMethod]
        public void UpperBound()
            => Assert.IsTrue(UserInput.IsPortValid("65535"));

        [TestMethod]
        public void AboveUpperBound()
            => Assert.IsFalse(UserInput.IsPortValid("65536"));
    }

    [TestClass]
    public class BoardIdValidation
    {
        [TestMethod]
        public void Empty()
            => Assert.IsFalse(UserInput.IsBoardIdValid(""));

        [TestMethod]
        public void Letter()
            => Assert.IsFalse(UserInput.IsBoardIdValid("12a"));

        [TestMethod]
        public void SpecialChar()
            => Assert.IsFalse(UserInput.IsBoardIdValid("12?"));

        [TestMethod]
        public void Valid()
            => Assert.IsTrue(UserInput.IsBoardIdValid("123"));
    }
}
