using System;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Google.Api.Gax.Grpc;
using Google.Cloud.Datastore.V1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Voting.Storage
{
    [TestClass]
    public class TestVotingStorage
    {
        private static readonly string ProjectId = "groovy-cider-826";
        private VotingStore _votingStore;

        [TestInitialize]
        public void Init()
        {
            _votingStore = new VotingStore(ProjectId, VotingStore.TestNamespace);
            _votingStore.ClearAll();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _votingStore.ClearAll();
        }

        [TestMethod]
        public async Task TestInsert()
        {
            await _votingStore.Insert(new StandardVoting
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
            var creationDate = new DateTime(2012, 12, 20, 20, 12, 00, DateTimeKind.Utc);

            var standardVoting = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = creationDate,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink
            };

            await _votingStore.Insert(standardVoting);

            // act - read and update the voting
            var readVotings = await _votingStore.GetForUserId("Michael");
            var readVoting = readVotings.Single();

            readVoting.Description = "Foobar";
            readVoting.Title = "Title2";
            readVoting.DateCreated = DateTime.UtcNow;
            readVoting.Visibility = VotingVisibility.OwnerOnly;
            readVoting.Questions = _CreateDummyQuestions();

            await _votingStore.Update(readVoting);
            var updatedVoting = (await _votingStore.GetAll()).Single();
            
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
            var votingWritten = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            await _votingStore.Insert(votingWritten);

            var result = (await _votingStore.GetAll()).ToList();
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
