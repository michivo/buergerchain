using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace FreieWahl.Voting.Registrations
{
    public class ChallengeFireStore : IChallengeStore
    {
        private readonly FirestoreDb _db;
        private readonly string _collection = "Challenge";

        public ChallengeFireStore(string projectId, string prefix = "")
        {
            _collection = prefix + _collection;
            _db = FirestoreDb.Create(projectId);
        }


        public Task SetChallenge(Challenge challenge)
        {
            return _db.Collection(_collection).Document(challenge.RegistrationId).SetAsync(new Dictionary<string, object>
            {
                { "votingId", challenge.VotingId },
                { "value", challenge.Value },
                { "type", (int)challenge.Type },
                { "recipientName", challenge.RecipientName },
                { "recipientAddress", challenge.RecipientAddress }
            });
        }

        public async Task<Challenge> GetChallenge(string registrationId)
        {
            var snapshot = await _db.Collection(_collection).Document(registrationId).GetSnapshotAsync().ConfigureAwait(false);
            return new Challenge
            {
                RegistrationId = registrationId,
                Type = (ChallengeType) snapshot.GetValue<int>("type"),
                Value = snapshot.GetValue<string>("value"),
                VotingId = snapshot.GetValue<string>("votingId"),
                RecipientName = snapshot.GetValue<string>("recipientName"),
                RecipientAddress = snapshot.GetValue<string>("recipientAddress")
            };
        }

        public Task DeleteChallenge(string registrationId)
        {
            return _db.Collection(registrationId).Document(registrationId).DeleteAsync();
        }

        public async Task DeleteChallenges(string votingId)
        {
            var snapshot = await _db.Collection(_collection).WhereEqualTo("votingId", votingId).GetSnapshotAsync().ConfigureAwait(false);
            foreach (var doc in snapshot.Documents)
            {
                await doc.Reference.DeleteAsync().ConfigureAwait(false);
            }
        }
    }
}
