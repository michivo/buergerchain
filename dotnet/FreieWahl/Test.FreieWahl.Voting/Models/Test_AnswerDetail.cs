using FreieWahl.Voting.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Voting.Models
{
    [TestClass]
    public class TestAnswerDetail
    {
        [TestMethod]
        public void TestCreation()
        {
            var detail = new AnswerDetail();
            Assert.IsNull(detail.DetailValue);
            Assert.AreEqual(0, (int)detail.DetailType);
        }

        [TestMethod]
        public void TestProperties()
        {
            var detail = new AnswerDetail
            {
                DetailType = AnswerDetailType.AdditionalInfo,
                DetailValue = "foobar"
            };

            Assert.AreEqual("foobar", detail.DetailValue);
            Assert.AreEqual(AnswerDetailType.AdditionalInfo, detail.DetailType);
        }

        [TestMethod]
        public void TestEquality()
        {
            var detail1 = new AnswerDetail() {DetailType = AnswerDetailType.AdditionalInfo, DetailValue = "fooBar"};
            var detail2 = new AnswerDetail() { DetailType = AnswerDetailType.AdditionalInfo, DetailValue = "foobar" };
            var detail3 = new AnswerDetail() { DetailType = AnswerDetailType.InfoLink, DetailValue = "fooBar" };
            var detail4 = new AnswerDetail() { DetailType = AnswerDetailType.AdditionalInfo, DetailValue = "42" };
            var detail5 = new AnswerDetail() { DetailType = AnswerDetailType.AdditionalInfo, DetailValue = null };
            var detail6 = new AnswerDetail() { DetailType = AnswerDetailType.AdditionalInfo };

            Assert.AreEqual(detail1, detail2);
            Assert.AreEqual(detail1, detail1);
            Assert.AreEqual(detail2, detail2);
            Assert.AreEqual(detail2, detail1);
            Assert.AreEqual(detail3, detail3);
            Assert.AreEqual(detail4, detail4);
            Assert.AreEqual(detail6, detail5);
            Assert.AreEqual(detail5, detail6);
            Assert.AreEqual(detail1.GetHashCode(), detail2.GetHashCode());
            Assert.AreEqual(detail6.GetHashCode(), detail5.GetHashCode());

            Assert.AreNotEqual(detail1, null);
            Assert.AreEqual(detail1, (object)detail2);
            Assert.AreEqual((object)detail1, detail2);

            Assert.AreNotEqual(detail1, detail3);
            Assert.AreNotEqual(detail3, detail1);
            Assert.AreNotEqual(detail2, detail3);
            Assert.AreNotEqual(detail3, detail2);
            Assert.AreNotEqual(detail1, detail4);
            Assert.AreNotEqual(detail4, detail1);
            Assert.AreNotEqual(detail2, detail4);
            Assert.AreNotEqual(detail4, detail2);
            Assert.AreNotEqual(detail3, detail4);
            Assert.AreNotEqual(detail4, detail3);
        }
    }
}
