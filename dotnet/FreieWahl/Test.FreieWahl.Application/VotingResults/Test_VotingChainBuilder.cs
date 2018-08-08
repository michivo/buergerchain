using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using FreieWahl.Application.VotingResults;
using FreieWahl.Voting.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto.Digests;

namespace Test.FreieWahl.Application.VotingResults
{
    [TestClass]
    public class Test_VotingChainBuilder
    {
        [TestMethod]
        public void CompareHashImplementations()
        {
            var bouncy1 = new Sha512Digest();
            var bouncy2 = new Sha512Digest();
            var dotNet = new SHA512Managed();

            var data = Encoding.UTF8.GetBytes("Hello world, we are digesting!");
            bouncy1.BlockUpdate(data, 0, data.Length);
            var hash1 = new byte[bouncy1.GetDigestSize()];
            bouncy1.DoFinal(hash1, 0);

            bouncy1.Reset();
            bouncy1.BlockUpdate(data, 0, data.Length);
            var hash2 = new byte[bouncy1.GetDigestSize()];
            bouncy1.DoFinal(hash2, 0);

            bouncy2.BlockUpdate(data, 0, data.Length);
            var hash3 = new byte[bouncy2.GetDigestSize()];
            bouncy2.DoFinal(hash3, 0);

            dotNet.Initialize();
            var hash4 = dotNet.ComputeHash(data);
            var hash5 = dotNet.ComputeHash(data);

            var hash1Text = Convert.ToBase64String(hash1);
            var hash2Text = Convert.ToBase64String(hash2);
            var hash3Text = Convert.ToBase64String(hash3);
            var hash4Text = Convert.ToBase64String(hash4);
            var hash5Text = Convert.ToBase64String(hash5);

            Assert.AreEqual(hash1Text, hash5Text);
            Assert.AreEqual(hash1Text, hash4Text);
            Assert.AreEqual(hash1Text, hash3Text);
            Assert.AreEqual(hash1Text, hash2Text);
        }

        [TestMethod]
        public void TestGenesisBlock()
        {
            var question = _CreateDummyQuestion();
            var question2 = _CreateDummyQuestion();
            question2.AnswerOptions[0].Id = question.AnswerOptions[0].Id;
            question2.AnswerOptions[1].Id = question.AnswerOptions[1].Id;

            var chainBuilder = new VotingChainBuilder();

            var genesis0 = chainBuilder.GetGenesisValue(question);
            var genesis1 = chainBuilder.GetGenesisValue(question);
            var genesis2 = chainBuilder.GetGenesisValue(question2);

            Assert.AreEqual(genesis0, genesis2);
            Assert.AreEqual(genesis1, genesis2);
        }

        [TestMethod]
        public void TestVoteBlock()
        {
            var genesis = "ofobar";
            var answerId = Guid.NewGuid().ToString();
            var answerId2 = Guid.NewGuid().ToString();
            var vote1 = new Vote
            {
                QuestionIndex = 234,
                PreviousBlockSignature = genesis,
                VotingId = 123456,
                SignedToken = "Foobar",
                Token = "Barfoo",
                TimestampData = "Ruffruff",
                SelectedAnswerIds = new List<string> { answerId, answerId2 }
            };
            var vote2 = new Vote
            {
                QuestionIndex = 234,
                PreviousBlockSignature = genesis,
                VotingId = 123456,
                SignedToken = "Foobar",
                Token = "Barfoo",
                TimestampData = "Ruffruff",
                SelectedAnswerIds = new List<string> { answerId }
            };

            var vote3 = new Vote
            {
                QuestionIndex = 234,
                PreviousBlockSignature = genesis,
                VotingId = 123456,
                SignedToken = "Foobar",
                Token = "Barfoo",
                TimestampData = "Ruffruff",
                SelectedAnswerIds = new List<string> { answerId2, answerId }
            };

            var chainBuilder = new VotingChainBuilder();

            // act
            var sig1 = chainBuilder.GetSignature(vote1);
            var sig2 = chainBuilder.GetSignature(vote1);
            var sig3 = chainBuilder.GetSignature(vote2);
            var sig4 = chainBuilder.GetSignature(vote3);

            Assert.AreEqual(sig1, sig2);
            Assert.AreNotEqual(sig3, sig2);
            Assert.AreNotEqual(sig1, sig4);
        }

        [TestMethod]
        public void TestDifferentGenesisBlocks()
        {
            var question = _CreateDummyQuestion();
            var question2 = _CreateDummyQuestion();

            var chainBuilder = new VotingChainBuilder();

            var genesis0 = chainBuilder.GetGenesisValue(question);
            var genesis1 = chainBuilder.GetGenesisValue(question);
            var genesis2 = chainBuilder.GetGenesisValue(question2);

            Assert.AreEqual(genesis0, genesis1);
            Assert.AreNotEqual(genesis1, genesis2);
        }

        [TestMethod]
        public void TestGeneratingBlocks()
        {
            var chainBuilder = new VotingChainBuilder();

            var question = _CreateDummyQuestion();

            var genesis = chainBuilder.GetGenesisValue(question);

            var vote = new Vote
            {
                QuestionIndex = 234,
                PreviousBlockSignature = genesis,
                VotingId = 123456,
                SignedToken = "Foobar",
                Token = "Barfoo",
                TimestampData = "Ruffruff",
                SelectedAnswerIds = new List<string> { question.AnswerOptions[0].Id }
            };

            var nextSig = chainBuilder.GetSignature(vote);

            var nextVote = new Vote
            {
                QuestionIndex = 234,
                PreviousBlockSignature = nextSig,
                VotingId = 123457,
                SignedToken = "Foobar",
                Token = "Barfoo",
                TimestampData = "Ruffruff",
                SelectedAnswerIds = new List<string> { question.AnswerOptions[1].Id }
            };

            nextSig = chainBuilder.GetSignature(nextVote);

            var lastVote = new Vote
            {
                QuestionIndex = 234,
                PreviousBlockSignature = nextSig,
                VotingId = 123458,
                SignedToken = "Foaobar",
                Token = "Barfoo",
                TimestampData = "Ruffruff",
                SelectedAnswerIds = new List<string> { question.AnswerOptions[1].Id }
            };

            var votes = new List<Vote> {vote, nextVote, lastVote};
            chainBuilder.CheckChain(question, votes);
        }

        private static Question _CreateDummyQuestion()
        {
            return new Question
            {
                QuestionIndex = 234,
                QuestionText = "How are you today?",
                Status = QuestionStatus.Locked,
                Details = new List<QuestionDetail>
                {
                    new QuestionDetail
                    {
                        DetailType = QuestionDetailType.InfoLink,
                        DetailValue = "http://www.google.at"
                    },
                    new QuestionDetail
                    {
                        DetailType = QuestionDetailType.AdditionalInfo,
                        DetailValue = "This rocks"
                    }
                },
                AnswerOptions = new List<AnswerOption>
                {
                    new AnswerOption
                    {
                        AnswerText = "Fine",
                        Details = new List<AnswerDetail>(),
                        Id = Guid.NewGuid().ToString()
                    },
                    new AnswerOption
                    {
                        AnswerText = "Great",
                        Details = new List<AnswerDetail>
                        {
                            new AnswerDetail
                            {
                                DetailType = AnswerDetailType.AdditionalInfo,
                                DetailValue = "foo bar"
                            },
                            new AnswerDetail
                            {
                                DetailType = AnswerDetailType.InfoLink,
                                DetailValue = "https://www.youtube.com"
                            }
                        },
                        Id = Guid.NewGuid().ToString()
                    }
                }
            };
        }
    }
}
