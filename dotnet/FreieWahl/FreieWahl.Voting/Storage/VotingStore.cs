using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Common;
using FreieWahl.Voting.Models;
using Google.Api.Gax.Grpc;
using Google.Cloud.Datastore.V1;
using Grpc.Core;

namespace FreieWahl.Voting.Storage
{
    public partial class VotingStore : IVotingStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "StandardVoting";
        public static readonly string TestNamespace = "test";
        public static readonly string DevNamespace = "dev";
        private readonly KeyFactory _keyFactory;

        public VotingStore(string projectId, string namespaceId = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId, namespaceId, client);
            _keyFactory = new KeyFactory(projectId, namespaceId, StoreKind);
        }

        public async Task Insert(StandardVoting voting)
        {
            voting.CurrentQuestionIndex = voting.Questions.Count;
            for (int i = 0; i < voting.Questions.Count; i++)
            {
                voting.Questions[i].QuestionIndex = i;
            }
            var entity = ToEntity(voting);
            entity.Key = _keyFactory.CreateIncompleteKey();
            var key = await _db.InsertAsync(entity).ConfigureAwait(false);
            voting.Id = key.Path.First().Id;
        }

        public async Task AddQuestion(long votingId, Question question)
        {
            var voting = await _GetVoting(votingId);
            question.QuestionIndex = voting.CurrentQuestionIndex;
            voting.CurrentQuestionIndex += 1;
            voting.Questions.Add(question);
            await _db.UpdateAsync(ToEntity(voting));
        }

        public async Task DeleteQuestion(long votingId, int questionIndex)
        {
            var voting = await _GetVoting(votingId);
            if (voting.Questions.All(x => x.QuestionIndex != questionIndex))
            {
                throw new InvalidOperationException("Trying to delete inexistent question");
            }

            voting.Questions = voting.Questions.Where(x => x.QuestionIndex != questionIndex).ToList();
            await _db.UpdateAsync(ToEntity(voting));
        }

        public async Task ClearQuestions(long votingId)
        {
            var voting = await _GetVoting(votingId);
            voting.Questions = new List<Question>();
            Console.WriteLine(votingId);
            await _db.UpdateAsync(ToEntity(voting));
        }

        public async Task UpdateQuestion(long votingId, Question question)
        {
            var voting = await _GetVoting(votingId);
            for (int i = 0; i < voting.Questions.Count; i++)
            {
                if (voting.Questions[i].QuestionIndex == question.QuestionIndex)
                {
                    voting.Questions[i] = question;
                    await _db.UpdateAsync(ToEntity(voting));
                    return;
                }
            }

            throw new InvalidOperationException("Tried to update inexistent question");
        }

        public async Task UpdateState(long votingId, VotingState state)
        {
            var voting = await _GetVoting(votingId);
            voting.State = state;
            await _db.UpdateAsync(ToEntity(voting));
        }

        public Task Delete(long votingId)
        {
            var key = _keyFactory.CreateKey(votingId);
            return _db.DeleteAsync(key);
        }

        public async Task Update(StandardVoting voting)
        {
            var readVoting = await _GetVoting(voting.Id);
            if (!readVoting.Creator.Equals(voting.Creator, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot modify creator of voting!");
            }

            readVoting.Description = voting.Description;
            _CheckQuestionsForEquality(voting, readVoting);
            readVoting.Questions = voting.Questions;
            readVoting.Title = voting.Title;
            readVoting.Visibility = voting.Visibility;
            if (!string.IsNullOrEmpty(voting.ImageData))
                readVoting.ImageData = voting.ImageData;

            await _db.UpdateAsync(ToEntity(readVoting));
        }

        private static void _CheckQuestionsForEquality(StandardVoting voting, StandardVoting readVoting)
        {
            if (readVoting.Questions.Count != voting.Questions.Count)
            {
                throw new InvalidOperationException("Cannot modify number of questions!");
            }

            for (int i = 0; i < voting.Questions.Count; i++)
            {
                if (voting.Questions[i].QuestionIndex != readVoting.Questions[i].QuestionIndex)
                {
                    throw new InvalidOperationException("Cannot modify index of questions!");
                }
            }
        }

        private async Task<StandardVoting> _GetVoting(long votingId)
        {
            var key = _keyFactory.CreateKey(votingId);
            var result = await _db.LookupAsync(key).ConfigureAwait(false);
            if (result == null)
            {
                throw new InvalidOperationException("No voting with id " + votingId + " was found.");
            }

            return FromEntity(result);
        }

        public Task<StandardVoting> GetById(long id)
        {
            return _GetVoting(id);
        }

        public async Task<IEnumerable<StandardVoting>> GetForUserId(string userId)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.Equal("Creator", userId),
                //Order = { { "DateCreated", PropertyOrder.Types.Direction.Descending } }
            };

            var results = await _db.RunQueryAsync(query).ConfigureAwait(false);

            return results.Entities.Select(FromEntity);
        }

        public void ClearAll()
        {
            if (_db.NamespaceId != TestNamespace)
            {
                throw new InvalidOperationException("ClearAll is only allowed in the test environment!");
            }

            var query = _db.RunQuery(new Query(StoreKind));
            _db.Delete(query.Entities);
        }

        public async Task<IEnumerable<StandardVoting>> GetAll()
        {
            var query = new Query(StoreKind)
            {
                Order = { { "DateCreated", PropertyOrder.Types.Direction.Descending } }
            };
            var results = await _db.RunQueryAsync(query).ConfigureAwait(false);

            return results.Entities.Select(FromEntity);
        }

        private static StandardVoting FromEntity(Entity entity)
        {
            var visibility = (int?)entity["Visibility"];
            var visibilityValue = visibility == null ? VotingVisibility.OwnerOnly : (VotingVisibility)visibility;
            var state = (int?)entity["State"];
            var stateValue = state == null ? VotingState.Ready : (VotingState)state;
            var mimeType = entity["ImageType"]?.StringValue;
            var imageData = string.Empty;
            if (!string.IsNullOrEmpty(mimeType))
            {
                var rawImageData = entity["ImageData"].BlobValue;
                imageData = "data:" + mimeType + ";base64," + rawImageData.ToBase64();
            }

            return new StandardVoting()
            {
                Id = entity.Key.Path.First().Id,
                Title = entity["Title"].StringValue,
                Creator = entity["Creator"].StringValue,
                Description = entity["Description"].StringValue,
                DateCreated = (DateTime)entity["DateCreated"],
                StartDate = _SafeGetDate(entity, "StartDate", DateTime.Now),
                EndDate = _SafeGetDate(entity, "EndDate", DateTime.Now.AddDays(1)),
                Visibility = visibilityValue,
                State = stateValue,
                Questions = FromQuestionsEntity(entity["Questions"]),
                CurrentQuestionIndex = (int?)entity["CurrentQuestionIndex"] ?? 0,
                ImageData = imageData
            };
        }

        private static DateTime _SafeGetDate(Entity e, string key, DateTime defaultValue)
        {
            var value = e[key];
            if (value != null)
                return (DateTime)value;
            return defaultValue;
        }

        private Entity ToEntity(StandardVoting standardVoting)
        {
            var mimeType = standardVoting.ImageData.GetMimeType();
            var rawData = standardVoting.ImageData.GetImageData();

            var result = new Entity
            {
                Key = _keyFactory.CreateKey(standardVoting.Id),
                ["Title"] = standardVoting.Title,
                ["Creator"] = standardVoting.Creator,
                ["DateCreated"] = standardVoting.DateCreated.ToUniversalTime(),
                ["Description"] = standardVoting.Description,
                ["Visibility"] = (int)standardVoting.Visibility,
                ["State"] = (int)standardVoting.State,
                ["Questions"] = ToEntities(standardVoting.Questions),
                ["CurrentQuestionIndex"] = standardVoting.CurrentQuestionIndex,
                ["ImageData"] = rawData,
                ["ImageType"] = mimeType,
                ["StartDate"] = standardVoting.StartDate.ToUniversalTime(),
                ["EndDate"] = standardVoting.EndDate.ToUniversalTime()
            };
            result["ImageData"].ExcludeFromIndexes = true;
            return result;
        }
    }
}
