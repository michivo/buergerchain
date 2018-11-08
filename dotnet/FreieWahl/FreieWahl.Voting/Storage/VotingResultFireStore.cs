using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Google.Cloud.Datastore.V1;
using Google.Cloud.Firestore;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace FreieWahl.Voting.Storage
{
    public class VotingResultFireStore : IVotingResultStore
    {
        private readonly FirestoreDb _db;
        private readonly string _collection = "VotingResult";

        public VotingResultFireStore(string projectId, string prefix = "")
        {
            _collection = prefix + _collection;
            _db = FirestoreDb.Create(projectId);
        }
        
        public Task StoreVote(Vote v, Func<Vote, string> getBlockSignature, Func<Task<string>> getGenesisSignature)
        {
            return _db.RunTransactionAsync(async transaction =>
            {
                var query = _db.Collection(_collection)
                    .WhereEqualTo("VotingId", v.VotingId)
                    .WhereEqualTo("QuestionIndex", v.QuestionIndex)
                    .OrderByDescending("DateCreated")
                    .Limit(1);
                var lastVoteSnapshot = await query.GetSnapshotAsync();
                string lastSignature;
                if (lastVoteSnapshot.Count == 1)
                {
                    var lastVote = _MapToVote(lastVoteSnapshot[0]);
                    lastSignature = getBlockSignature(lastVote);
                }
                else
                {
                    lastSignature = await getGenesisSignature();
                }
                await _db.Collection(_collection).AddAsync(new Dictionary<string, object>
                {
                    {"PreviousBlockSignature", lastSignature},
                    {"QuestionIndex", v.QuestionIndex},
                    {"SelectedAnswerIds", v.SelectedAnswerIds},
                    {"SignedToken", v.SignedToken},
                    {"TimestampData", v.TimestampData},
                    {"Token", v.Token},
                    {"VotingId", v.VotingId},
                    {"DateCreated", Timestamp.FromDateTime(DateTime.UtcNow)}
                });
            });
        }

        public async Task<Vote> GetLastVote(string votingId, int questionIndex)
        {
            var snapshot = await _db.Collection(_collection)
                .WhereEqualTo("VotingId", votingId)
                .WhereEqualTo("QuestionIndex", questionIndex)
                .OrderByDescending("DateCreated")
                .Limit(1)
                .GetSnapshotAsync().ConfigureAwait(false);

            if(snapshot.Count == 0)
                return null;

            var result = snapshot[0];
            return _MapToVote(result);
        }

        private Vote _MapToVote(DocumentSnapshot doc)
        {
            return new Vote
            {
                PreviousBlockSignature = doc.GetValue<string>("PreviousBlockSignature"),
                QuestionIndex = doc.GetValue<int>("QuestionIndex"),
                VotingId = doc.GetValue<string>("VotingId"),
                SelectedAnswerIds = doc.GetValue<string[]>("SelectedAnswerIds").ToList(),
                TimestampData = doc.GetValue<string>("TimestampData"),
                Token = doc.GetValue<string>("Token"),
                SignedToken = doc.GetValue<string>("SignedToken")
            };
        }
        
        public async Task<IReadOnlyCollection<Vote>> GetVotes(string votingId)
        {
            var snapshot = await _db.Collection(_collection)
                .WhereEqualTo("VotingId", votingId)
                .OrderBy("DateCreated")
                .GetSnapshotAsync().ConfigureAwait(false);
            return snapshot.Select(_MapToVote).ToList();
        }

        public async Task<IReadOnlyCollection<Vote>> GetVotes(string votingId, int questionIndex)
        {
            var snapshot = await _db.Collection(_collection)
                .WhereEqualTo("VotingId", votingId)
                .WhereEqualTo("QuestionIndex", questionIndex)
                .OrderByDescending("DateCreated")
                .GetSnapshotAsync().ConfigureAwait(false);
            var result = snapshot.Select(_MapToVote).ToList();
            result.Reverse();
            return result;
        }
    }
}
