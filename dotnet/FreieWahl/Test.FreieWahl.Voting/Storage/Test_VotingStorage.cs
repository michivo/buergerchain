using System;
using System.Linq;
using System.Collections.Generic;
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
            Assert.AreEqual(readVoting.Questions.Count, updatedVoting.Questions.Count);
            for (int i = 0; i < readVoting.Questions.Count; i++)
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

        [TestMethod]
        public async Task GetForUserId()
        {
            // arrange 
            var voting1 = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            var voting2 = new StandardVoting
            {
                Creator = "Douglas",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            var voting3 = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            await _votingStore.Insert(voting1);
            await _votingStore.Insert(voting2);
            await _votingStore.Insert(voting3);

            // act
            var votingsForMichael = (await _votingStore.GetForUserId("Michael")).ToList();
            var votingsForDouglas = (await _votingStore.GetForUserId("Douglas")).ToList();
            var votingsForSomeoneElse = (await _votingStore.GetForUserId("douglas")).ToList();

            // assert
            Assert.AreEqual(2, votingsForMichael.Count);
            Assert.IsTrue(votingsForMichael.Exists(x => x.Id == voting1.Id));
            Assert.IsTrue(votingsForMichael.Exists(x => x.Id == voting3.Id));
            Assert.AreEqual(1, votingsForDouglas.Count);
            Assert.IsTrue(votingsForDouglas.Exists(x => x.Id == voting2.Id));
            Assert.AreEqual(0, votingsForSomeoneElse.Count);
        }

        [TestMethod]
        public async Task GetAllPublic()
        {
            // arrange 
            var voting1 = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            var voting2 = new StandardVoting
            {
                Creator = "Douglas",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.Public,
                Questions = _CreateDummyQuestions()
            };
            var voting3 = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.Public,
                Questions = _CreateDummyQuestions()
            };
            await _votingStore.Insert(voting1);
            await _votingStore.Insert(voting2);
            await _votingStore.Insert(voting3);

            // act
            var publicVotings = (await _votingStore.GetAllPublic()).ToList();

            // assert
            Assert.AreEqual(2, publicVotings.Count);
            Assert.IsTrue(publicVotings.Exists(x => x.Id == voting2.Id));
            Assert.IsTrue(publicVotings.Exists(x => x.Id == voting3.Id));
        }

        [TestMethod]
        public async Task ClearAll()
        {
            // arrange 
            var voting1 = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            var voting2 = new StandardVoting
            {
                Creator = "Douglas",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.Public,
                Questions = _CreateDummyQuestions()
            };
            var voting3 = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.Public,
                Questions = _CreateDummyQuestions()
            };
            await _votingStore.Insert(voting1);
            await _votingStore.Insert(voting2);
            await _votingStore.Insert(voting3);
            var allVotings = (await _votingStore.GetAll()).ToList();
            Assert.AreEqual(3, allVotings.Count);

            // act
            _votingStore.ClearAll();


            // assert
            allVotings = (await _votingStore.GetAll()).ToList();
            Assert.AreEqual(0, allVotings.Count);
        }

        [TestMethod]
        public async Task GetNonExistingById()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _votingStore.GetById(1234L));
        }

        [TestMethod]
        public async Task GetById()
        {
            // arrange 
            var votingWritten = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = _CreateDummyQuestions()
            };
            Assert.AreEqual(0, votingWritten.Id);
            await _votingStore.Insert(votingWritten);
            Assert.AreNotEqual(0, votingWritten.Id);

            // act
            var voting = await _votingStore.GetById(votingWritten.Id);

            // assert
            Assert.AreEqual(votingWritten, voting);
        }

        [TestMethod]
        public async Task AddQuestion()
        {
            // arrange 
            var votingWritten = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = new List<Question>()
            };
            await _votingStore.Insert(votingWritten);
            Question q = _CreateDummyQuestions().First();

            // act
            await _votingStore.AddQuestion(votingWritten.Id, q);

            // assert
            var voting = await _votingStore.GetById(votingWritten.Id);
            Assert.AreEqual(1, voting.Questions.Count);
            Assert.AreNotEqual(0, voting.Questions[0].Id);
            q.Id = voting.Questions[0].Id;
            Assert.AreEqual(q, voting.Questions[0]);
        }

        [TestMethod]
        public async Task UpdateQuestion()
        {
            // arrange 
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
            Question q = votingWritten.Questions[0];
            q.QuestionText = "Dawg, you sure?";
            q.AnswerOptions.RemoveAt(2);

            // act
            await _votingStore.UpdateQuestion(votingWritten.Id, q);

            // assert
            var voting = await _votingStore.GetById(votingWritten.Id);
            Assert.AreEqual(voting.Questions[0], q);
        }

        [TestMethod]
        public async Task DeleteQuestion()
        {
            // arrange
            var questions = _CreateDummyQuestions();
            var q = new Question
            {
                AnswerOptions = new List<AnswerOption>(),
                Details = new List<QuestionDetail>(),
                QuestionText = "Mooh mooh",
                Status = QuestionStatus.Locked
            };
            questions.Add(q);
            var votingWritten = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = questions
            };
            await _votingStore.Insert(votingWritten);

            // act
            await _votingStore.DeleteQuestion(votingWritten.Id, votingWritten.Questions[0].Id);

            // assert
            var voting = await _votingStore.GetById(votingWritten.Id);
            Assert.AreEqual(1, voting.Questions.Count);
            Assert.AreEqual(q, voting.Questions.Single());
        }

        [TestMethod]
        public async Task ClearQuestions()
        {
            // arrange
            var questions = _CreateDummyQuestions();
            var q = new Question
            {
                AnswerOptions = new List<AnswerOption>(),
                Details = new List<QuestionDetail>(),
                QuestionText = "Mooh mooh",
                Status = QuestionStatus.Locked
            };
            questions.Add(q);
            var votingWritten = new StandardVoting
            {
                Creator = "Michael",
                DateCreated = DateTime.UtcNow,
                Description = "Desc",
                Title = "Title",
                Visibility = VotingVisibility.WithLink,
                Questions = questions
            };
            await _votingStore.Insert(votingWritten);

            // act
            await _votingStore.ClearQuestions(votingWritten.Id);

            // assert
            var voting = await _votingStore.GetById(votingWritten.Id);
            Assert.AreEqual(0, voting.Questions.Count);
        }

        private static List<Question> _CreateDummyQuestions()
        {
            return new List<Question>
            {
                new Question()
                {
                    QuestionText = "You sure?",
                    AnswerOptions = new List<AnswerOption>
                    {
                        new AnswerOption()
                        {
                            AnswerText = "Yes",
                            Details = new List<AnswerDetail>
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
                            Details = new List<AnswerDetail>
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
                    Details = new List<QuestionDetail>
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
