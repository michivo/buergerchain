﻿using System.Collections.Generic;
using FreieWahl.Voting.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Voting.Models
{
    [TestClass]
    public class TestAnswerOption
    {
        [TestMethod]
        public void TestCreation()
        {
            var answerOption = new AnswerOption();
            Assert.IsNotNull(answerOption.Details);
            Assert.AreEqual(0, answerOption.Details.Count);
            Assert.IsNull(answerOption.AnswerText);
            Assert.IsNotNull(answerOption.Id);
        }

        [TestMethod]
        public void TestProperties()
        {
            var answerOption = new AnswerOption
            {
                AnswerText = "Hello",
                Id = "iid",
                Details = new List<AnswerDetail>
                {
                    new AnswerDetail {DetailType = AnswerDetailType.InfoLink, DetailValue = "asdf"}
                }
            };

            Assert.AreEqual("Hello", answerOption.AnswerText);
            Assert.AreEqual("iid", answerOption.Id);
            Assert.AreEqual(1, answerOption.Details.Count);

            answerOption.Details = null;
            Assert.IsNotNull(answerOption.Details);
            Assert.AreEqual(0, answerOption.Details.Count);
        }
    }
}
