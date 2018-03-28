using System;
using FreieWahl.Voting.Common;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Voting.Common
{
    [TestClass]
    public class TestExtensionMethods
    {
        [TestMethod]
        public void StringEquals()
        {
            string a = "ASDF";
            string b = "isdf";
            string c = "asdf";

            Assert.IsFalse(a.EqualsDefault(b));
            Assert.IsFalse(b.EqualsDefault(a));
            Assert.IsFalse(c.EqualsDefault(b));
            Assert.IsFalse(b.EqualsDefault(c));
            Assert.IsTrue(a.EqualsDefault(c));
            Assert.IsTrue(c.EqualsDefault(a));
            Assert.IsTrue(a.EqualsDefault(a)); // self
            Assert.IsTrue(c.EqualsDefault(c)); // self
            Assert.IsTrue(b.EqualsDefault(b)); // self
        }

        [TestMethod]
        public void DateTimeEquals()
        {
            DateTime t1 = DateTime.UtcNow;
            DateTime t2 = t1.AddTicks(50);
            DateTime t3 = t2.AddTicks(60);

            Assert.IsTrue(t1.EqualsDefault(t1)); // self
            Assert.IsTrue(t2.EqualsDefault(t2)); // self
            Assert.IsTrue(t3.EqualsDefault(t3)); // self

            Assert.IsTrue(t1.EqualsDefault(t2));
            Assert.IsTrue(t2.EqualsDefault(t1));
            Assert.IsTrue(t3.EqualsDefault(t2));
            Assert.IsTrue(t2.EqualsDefault(t3));
            Assert.IsFalse(t3.EqualsDefault(t1)); // delta t > 100 ticks
            Assert.IsFalse(t1.EqualsDefault(t3)); // delta t > 100 ticks
        }
    }
}
