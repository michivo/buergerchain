using System;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Voting.Storage
{
    [TestClass]
    public class TestVotingStorage
    {
        [TestInitialize]
        public void Init()
        {
            var store = new VotingStore("stunning-lambda-162919", VotingStore.TestNamespace);
            store.ClearAll();
        }

        [TestCleanup]
        public void Cleanup()
        {
            var store = new VotingStore("stunning-lambda-162919", VotingStore.TestNamespace);
            store.ClearAll();
        }

        [TestMethod]
        public void TestInsert()
        {
            var store = new VotingStore("stunning-lambda-162919", VotingStore.TestNamespace);

            store.Insert(new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink
            });
        }

        [TestMethod]
        public async Task TestInsertAndRead()
        {
            var store = new VotingStore("stunning-lambda-162919", VotingStore.TestNamespace);


            var votingWritten = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = new[]
                {
                    new Question()
                    {
                        QuestionText = "You sure?",
                        AnswerOptions = new []
                        {
                            new AnswerOption()
                            {
                                AnswerText = "Yes",
                                Details = new []
                                {
                                    new AnswerDetail
                                    {
                                        DetailType = AnswerDetailType.AdditionalInfo,
                                        DetailValue = "You agree"
                                    }
                                },
                                Id = Guid.NewGuid().ToString()
                            },
                            new AnswerOption()
                            {
                                AnswerText = "No",
                                Details = new []
                                {
                                    new AnswerDetail
                                    {
                                        DetailType = AnswerDetailType.AdditionalInfo,
                                        DetailValue = "You disagree"
                                    }
                                },
                                Id = Guid.NewGuid().ToString()
                            },
                            new AnswerOption()
                            {
                                AnswerText = "Dontcare",
                                Id = Guid.NewGuid().ToString()
                            }
                        },
                        Details = new []
                        {
                            new QuestionDetail
                            {
                                DetailType = QuestionDetailType.InfoLink,
                                DetailValue = "http://www.freiewahl.eu"
                            }
                        }
                    }
                }
            };
            store.Insert(votingWritten);

            var result = (await store.GetAll()).ToList();
            Assert.AreEqual(1, result.Count());
            var votingRead = result.Single();
            Assert.AreEqual(votingWritten, votingRead);
        }
    }
}
