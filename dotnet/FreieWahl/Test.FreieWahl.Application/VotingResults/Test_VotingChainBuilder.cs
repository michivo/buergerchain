using System;
using System.Collections.Generic;
using FreieWahl.Application.VotingResults;
using FreieWahl.Voting.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Application.VotingResults
{
    [TestClass]
    public class Test_VotingChainBuilder
    {
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
