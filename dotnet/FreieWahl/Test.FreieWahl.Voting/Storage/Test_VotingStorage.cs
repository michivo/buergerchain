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
        private static readonly string ProjectId = "groovy-cider-826";

        [TestInitialize]
        public void Init()
        {
            var store = new VotingStore(ProjectId, VotingStore.TestNamespace);
            store.ClearAll();
        }

        [TestCleanup]
        public void Cleanup()
        {
            var store = new VotingStore(ProjectId, VotingStore.TestNamespace);
            store.ClearAll();
        }

        [TestMethod]
        public void TestInsert()
        {
            var store = new VotingStore(ProjectId, VotingStore.TestNamespace);

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
        public async Task TestUpdate()
        {
            // arrange
            var store = new VotingStore(ProjectId, VotingStore.TestNamespace);
            var creationDate = new DateTime(2012, 12, 20, 20, 12, 00, DateTimeKind.Utc);

            var standardVoting = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = creationDate,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink
            };

            store.Insert(standardVoting);

            // act - read and update the voting
            var readVotings = await store.GetForUserId("Michael");
            var readVoting = readVotings.Single();

            readVoting.Description = "Foobar";
            readVoting.Title = "Title2";
            readVoting.Creator = "Donald";
            readVoting.DateCreated = DateTime.UtcNow;
            readVoting.Visibility = VotingVisibility.OwnerOnly;
            readVoting.Questions = _CreateDummyQuestions();

            await store.Update(readVoting);
            var updatedVoting = (await store.GetAll()).Single();
            
            // assert that voting was updated correctly
            Assert.AreEqual("Michael", updatedVoting.Creator); // should not have been updated!
            Assert.IsTrue(Math.Abs(updatedVoting.DateCreated.Subtract(creationDate).TotalMilliseconds) < 10);
            Assert.AreEqual("Title2", updatedVoting.Title);
            Assert.AreEqual(readVoting.Questions.Length, updatedVoting.Questions.Length);
            for (int i = 0; i < readVoting.Questions.Length; i++)
            {
                Assert.AreEqual(readVoting.Questions[i], updatedVoting.Questions[i]);
            }
            Assert.AreEqual(VotingVisibility.OwnerOnly, updatedVoting.Visibility);
            Assert.AreEqual("Foobar", updatedVoting.Description);
            Assert.AreEqual(readVoting.Id, updatedVoting.Id);
        }


        [TestMethod]
        public async Task TestInsertAndRead()
        {
            var store = new VotingStore(ProjectId, VotingStore.TestNamespace);


            var votingWritten = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            store.Insert(votingWritten);

            var result = (await store.GetAll()).ToList();
            Assert.AreEqual(1, result.Count());
            var votingRead = result.Single();
            Assert.AreEqual(votingWritten, votingRead);
        }

        private static Question[] _CreateDummyQuestions()
        {
            return new[]
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
            };
        }
    }
}
