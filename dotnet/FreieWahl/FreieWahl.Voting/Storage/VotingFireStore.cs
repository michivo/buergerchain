using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Google.Cloud.Firestore;

namespace FreieWahl.Voting.Storage
{
    public class VotingFireStore : IVotingStore
    {
        private readonly FirestoreDb _db;
        private readonly string _collection = "Votings";

        public VotingFireStore(string projectId, string collectionPrefix = "")
        {
            _collection = collectionPrefix + _collection;
            _db = FirestoreDb.Create(projectId);
        }

        public async Task Insert(StandardVoting voting)
        {
            var refId = await _db.Collection(_collection).AddAsync(voting).ConfigureAwait(false);
            voting.Id = refId.Id;
        }

        public Task Update(StandardVoting voting)
        {
            var docRef = _db.Collection(_collection).Document(voting.Id);
            return docRef.SetAsync(voting);
        }

        public async Task<StandardVoting> GetById(string id)
        {
            var docRef = _db.Collection(_collection).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
                return null;

            return snapshot.ConvertTo<StandardVoting>();
        }

        public Task<IEnumerable<StandardVoting>> GetForUserId(string userId)
        {
            return GetAll();
        }

        public async Task<IEnumerable<StandardVoting>> GetAll()
        {
            Query query = _db.Collection(_collection);
            var snapshot = await query.GetSnapshotAsync();
            var result = new List<StandardVoting>();
            foreach (var snapshotDocument in snapshot.Documents)
            {
                if (!snapshotDocument.Exists)
                    continue;

                result.Add(snapshotDocument.ConvertTo<StandardVoting>());
            }

            return result;
        }

        public async Task ClearAll()
        {
            var collectionReference = _db.Collection(_collection);
            int batchSize = 100;

            QuerySnapshot snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
            IReadOnlyList<DocumentSnapshot> documents = snapshot.Documents;
            while (documents.Count > 0)
            {
                foreach (DocumentSnapshot document in documents)
                {
                    await document.Reference.DeleteAsync();
                }

                snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
                documents = snapshot.Documents;
            }
        }

        public Task AddQuestion(string votingId, Question question)
        {
            DocumentReference votingRef = _db.Collection(_collection).Document(votingId);
            return votingRef.UpdateAsync("Questions", FieldValue.ArrayUnion(question));
        }

        public Task DeleteQuestion(string votingId, int questionIndex)
        {
            throw new NotImplementedException();
        }

        public Task ClearQuestions(string votingId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateQuestion(string votingId, Question question)
        {
            throw new NotImplementedException();
        }

        public Task UpdateState(string votingId, VotingState state)
        {
            DocumentReference votingRef = _db.Collection(_collection).Document(votingId);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "State", state }
            };
            return votingRef.UpdateAsync(updates);
        }

        public Task Delete(string votingId)
        {
            var docRef = _db.Collection(_collection).Document(votingId);
            return docRef.DeleteAsync();
        }
    }
}

