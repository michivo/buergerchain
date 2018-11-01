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
        
        public Task StoreVote(Vote v)
        {
            return _db.Collection(_collection).AddAsync(new Dictionary<string, object>
            {
                {"PreviousBlockSignature", v.PreviousBlockSignature},
                {"QuestionIndex", v.QuestionIndex},
                {"SelectedAnswerIds", v.SelectedAnswerIds},
                {"SignedToken", v.SignedToken},
                {"TimestampData", v.TimestampData },
                {"Token", v.Token },
                {"VotingId", v.VotingId },
                {"DateCreated", Timestamp.FromDateTime(DateTime.UtcNow) }
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
            return _MapToDocument(result);
        }

        private Vote _MapToDocument(DocumentSnapshot doc)
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
                .WhereEqualTo("VotingId", votingId).GetSnapshotAsync().ConfigureAwait(false);
            return snapshot.Select(_MapToDocument).ToList();
        }

        public async Task<IReadOnlyCollection<Vote>> GetVotes(string votingId, int questionIndex)
        {
            var snapshot = await _db.Collection(_collection)
                .WhereEqualTo("VotingId", votingId)
                .WhereEqualTo("QuestionIndex", questionIndex)
                .GetSnapshotAsync().ConfigureAwait(false);
            return snapshot.Select(_MapToDocument).ToList();

        }
    }
}
