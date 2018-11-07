using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Google.Cloud.Datastore.V1;
using Google.Protobuf.WellKnownTypes;

namespace FreieWahl.Voting.Storage
{
    public class VotingResultStore : IVotingResultStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "VotingResult";
        public static readonly string TestNamespace = "test";
        public static readonly string DevNamespace = "dev";
        private readonly KeyFactory _keyFactory;

        public VotingResultStore(string projectId, string namespaceId = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId, namespaceId, client);
            _keyFactory = new KeyFactory(projectId, namespaceId, StoreKind);
        }
        
        public Task StoreVote(Vote v)
        {
            var entity = _ToEntity(v);
            entity.Key = _keyFactory.CreateIncompleteKey();
            entity["DateCreated"] = Timestamp.FromDateTime(DateTime.UtcNow);
            return _db.InsertAsync(entity);
        }

        public async Task<Vote> GetLastVote(string votingId, int questionIndex)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.And(Filter.Equal("QuestionIndex", questionIndex), Filter.Equal("VotingId", votingId)),
                Order = { { "DateCreated", PropertyOrder.Types.Direction.Descending} },
                Limit = 1
            };
            var result = await _db.RunQueryAsync(query);
            if(result.Entities.Count == 0)
                return null;

            return _FromEntity(result.Entities[0]);
        }

        private static Entity _ToEntity(Vote vote)
        {
            var answerIds = new ArrayValue();
            foreach (var answerId in vote.SelectedAnswerIds)
            {
                answerIds.Values.Add(answerId);
            }
            return new Entity
            {
                ["VotingId"] = vote.VotingId,
                ["QuestionIndex"] = vote.QuestionIndex,
                ["Token"] = vote.Token,
                ["SignedToken"] = vote.SignedToken,
                ["TimestampData"] = vote.TimestampData,
                ["SelectedAnswerIds"] = answerIds,
                ["PreviousBlockSignature"] = vote.PreviousBlockSignature
            };
        }

        private static Vote _FromEntity(Entity entity)
        {
            return new Vote()
            {
                PreviousBlockSignature = entity["PreviousBlockSignature"].StringValue,
                VotingId = entity["VotingId"].StringValue,
                Token = entity["Token"].StringValue,
                SignedToken = entity["SignedToken"].StringValue,
                TimestampData = entity["TimestampData"].StringValue,
                QuestionIndex = (int) entity["QuestionIndex"].IntegerValue,
                SelectedAnswerIds = entity["SelectedAnswerIds"].ArrayValue.Values.Select(x => x.StringValue).ToList()
            };
        }

        public Task StoreVote(Vote v, Func<Vote, string> getBlockSignature, Func<Task<string>> getGenesisSignature)
        { // TODO: not implemented properly!!!
            return StoreVote(v);
        }

        public async Task<IReadOnlyCollection<Vote>> GetVotes(string votingId)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.Equal("VotingId", votingId)
            };

            var results = await _db.RunQueryAsync(query).ConfigureAwait(false);

            return results.Entities.Select(_FromEntity).ToList();
        }

        public async Task<IReadOnlyCollection<Vote>> GetVotes(string votingId, int questionIndex)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.And(Filter.Equal("QuestionIndex", questionIndex), Filter.Equal("VotingId", votingId))
            };

            var results = await _db.RunQueryAsync(query).ConfigureAwait(false);

            return results.Entities.Select(_FromEntity).ToList();
        }
    }
}
