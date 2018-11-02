using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace FreieWahl.Voting.Registrations
{
    public class RegistrationFireStore : IRegistrationStore
    {
        private readonly FirestoreDb _db;
        private readonly string _openCollection = "OpenRegistration";
        private readonly string _closedCollection = "CompletedRegistration";


        public RegistrationFireStore(string projectId, string prefix = "")
        {
            _openCollection = prefix + _openCollection;
            _closedCollection = prefix + _closedCollection;
            _db = FirestoreDb.Create(projectId);
        }

        public async Task AddOpenRegistration(OpenRegistration openRegistration)
        {
            var duplicateQuery = _db.Collection(_openCollection)
                .WhereEqualTo("VotingId", openRegistration.VotingId)
                .WhereEqualTo("VoterIdentity", openRegistration.VoterIdentity);
            var duplicates = await duplicateQuery.GetSnapshotAsync();
            if (duplicates.Count > 0)
            {
                foreach (var duplicate in duplicates)
                {
                    await _db.Collection(_openCollection).Document(duplicate.Id).DeleteAsync().ConfigureAwait(false);
                }
            }

            var doc = _db.Collection(_openCollection).Document(openRegistration.Id);
            await doc.SetAsync(new Dictionary<string, object>
            {
                {"VotingId", openRegistration.VotingId },
                {"VoterIdentity", openRegistration.VoterIdentity },
                {"VoterName", openRegistration.VoterName },
                {"RegistrationTime", Timestamp.FromDateTime(openRegistration.RegistrationTime) }
            });
        }

        public async Task AddCompletedRegistration(CompletedRegistration completedRegistration)
        {
            var newDoc = await _db.Collection(_closedCollection).AddAsync(new Dictionary<string, object>
            {
                {"VotingId", completedRegistration.VotingId},
                {"VoterIdentity", completedRegistration.VoterIdentity},
                {"VoterName", completedRegistration.VoterName},
                {"RegistrationTime", Timestamp.FromDateTime(completedRegistration.RegistrationTime)},
                {"DecisionTime", Timestamp.FromDateTime(completedRegistration.DecisionTime) },
                {"AdminUserId", completedRegistration.AdminUserId },
                {"Decision", (int)completedRegistration.Decision }
            }).ConfigureAwait(false);

            var duplicateQuery = _db.Collection(_closedCollection)
                .WhereEqualTo("VotingId", completedRegistration.VotingId)
                .WhereEqualTo("VoterIdentity", completedRegistration.VoterIdentity);
            var duplicates = await duplicateQuery.GetSnapshotAsync();
            if (duplicates.Count > 1)
            { 
                foreach (var duplicate in duplicates)
                {
                    if (duplicate.Id.Equals(newDoc.Id))
                        continue;
                    await _db.Collection(_closedCollection).Document(duplicate.Id).DeleteAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<IReadOnlyList<OpenRegistration>> GetOpenRegistrationsForVoting(string votingId)
        {
            var query = _db.Collection(_openCollection).WhereEqualTo(
                "VotingId", votingId);
            var snapshot = await query.GetSnapshotAsync().ConfigureAwait(false);

            return snapshot.Select(_MapToOpenRegistration).ToList();
        }

        public async Task<OpenRegistration> GetOpenRegistration(string registrationStoreId)
        {
            var doc = await _db.Collection(_openCollection).Document(registrationStoreId)
                .GetSnapshotAsync().ConfigureAwait(false);
            if (doc.Exists)
                return _MapToOpenRegistration(doc);

            return null;
        }

        public async Task<IReadOnlyList<CompletedRegistration>> GetCompletedRegistrations(string votingId)
        {
            var query = _db.Collection(_closedCollection)
                .WhereEqualTo("VotingId", votingId);
            var snapshot = await query.GetSnapshotAsync().ConfigureAwait(false);

            return snapshot.Select(_MapToClosedRegistration).ToList();
        }

        public async Task<bool> IsRegistrationUnique(string dataSigneeId, string votingId)
        {
            var query = _db.Collection(_closedCollection)
                .WhereEqualTo("VoterIdentity", dataSigneeId)
                .WhereEqualTo("VotingId", votingId);
            var result = await query.GetSnapshotAsync();
            return result.Count == 0;
        }

        private OpenRegistration _MapToOpenRegistration(DocumentSnapshot doc)
        {
            return new OpenRegistration
            {
                Id = doc.Id,
                VotingId = doc.GetValue<string>("VotingId"),
                VoterIdentity = doc.GetValue<string>("VoterIdentity"),
                VoterName = doc.GetValue<string>("VoterName"),
                RegistrationTime = doc.GetValue<Timestamp>("RegistrationTime").ToDateTime()
            };
        }

        private CompletedRegistration _MapToClosedRegistration(DocumentSnapshot doc)
        {
            return new CompletedRegistration
            {
                VotingId = doc.GetValue<string>("VotingId"),
                VoterIdentity = doc.GetValue<string>("VoterIdentity"),
                VoterName = doc.GetValue<string>("VoterName"),
                RegistrationTime = doc.GetValue<Timestamp>("RegistrationTime").ToDateTime(),
                AdminUserId = doc.GetValue<string>("AdminUserId"),
                Decision = (RegistrationDecision)doc.GetValue<int>("Decision"),
                DecisionTime = doc.GetValue<Timestamp>("DecisionTime").ToDateTime()
            };
        }

        public Task RemoveOpenRegistration(string id)
        {
            return _db.Collection(_openCollection).Document(id).DeleteAsync();
        }
    }
}
