using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using Google.Protobuf.WellKnownTypes;

namespace FreieWahl.Voting.Registrations
{
    public class RegistrationStore : IRegistrationStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "Registration";
        private const string CompletedStoreKind = "CompletedRegistration";
        public static readonly string TestNamespace = "test";
        private readonly KeyFactory _openKeyFactory;
        private readonly KeyFactory _completedKeyFactory;

        public RegistrationStore(string projectId, string namespaceId = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId, namespaceId, client);
            _openKeyFactory = new KeyFactory(projectId, namespaceId, StoreKind);
            _completedKeyFactory = new KeyFactory(projectId, namespaceId, CompletedStoreKind);
        }

        public async Task AddOpenRegistration(OpenRegistration openRegistration)
        {
            Query q = new Query(StoreKind)
            {
                Filter = Filter.And(
                    Filter.Equal("VotingId", openRegistration.VotingId),
                    Filter.Equal("VoterId", openRegistration.VoterIdentity)),
            };
            var results = await _db.RunQueryAsync(q);
            if (results.Entities.Count > 0)
            { // avoid double-registrations
                await _db.DeleteAsync(results.Entities);
            }

            var entity = _MapToEntity(openRegistration);

            var key = await _db.InsertAsync(entity).ConfigureAwait(false);
            openRegistration.RegistrationId = key.Path.First().Id;
        }

        public async Task AddCompletedRegistration(CompletedRegistration completedRegistration)
        {
            Query q = new Query(StoreKind)
            {
                Filter = Filter.And(
                    Filter.Equal("VotingId", completedRegistration.VotingId),
                    Filter.Equal("VoterId", completedRegistration.VoterIdentity)),
            };
            var results = await _db.RunQueryAsync(q);
            if (results.Entities.Count > 0)
            { // avoid double-registrations
                await _db.DeleteAsync(results.Entities);
            }

            var entity = _MapToEntity(completedRegistration);

            await _db.InsertAsync(entity).ConfigureAwait(false);
        }

        private Entity _MapToEntity(OpenRegistration openRegistration)
        {
            return new Entity()
            {
                Key = _openKeyFactory.CreateIncompleteKey(),
                ["VotingId"] = openRegistration.VotingId,
                ["VoterId"] = openRegistration.VoterIdentity,
                ["VoterName"] = openRegistration.VoterName,
                ["RegistrationTime"] = Timestamp.FromDateTime(openRegistration.RegistrationTime),
                ["Mail"] = openRegistration.EMailAdress,
                ["RegistrationStoreId"] = openRegistration.RegistrationStoreId
            };
        }

        public async Task<IReadOnlyList<OpenRegistration>> GetOpenRegistrationsForVoting(long votingId)
        {
            Query q = new Query(StoreKind)
            {
                Filter = Filter.Equal("VotingId", votingId)
            };

            var results = await _db.RunQueryAsync(q);

            return new List<OpenRegistration>(results.Entities.Select(_FromEntity));
        }

        public async Task<OpenRegistration> GetOpenRegistration(string registrationStoreId)
        {
            Query q = new Query(StoreKind)
            {
                Filter = Filter.Equal("RegistrationStoreId", registrationStoreId),
                Limit = 1
            };

            var results = await _db.RunQueryAsync(q);
            if (results.Entities.Count == 0)
            {
                return null;
            }

            return _FromEntity(results.Entities.Single());
        }

        public async Task<IReadOnlyList<CompletedRegistration>> GetCompletedRegistrations(long votingId)
        {
            Query q = new Query(CompletedStoreKind)
            {
                Filter = Filter.Equal("VotingId", votingId)
            };

            var results = await _db.RunQueryAsync(q);
            return results.Entities.Select(_FromCompletedEntity).ToList();
        }

        private CompletedRegistration _FromCompletedEntity(Entity entity)
        {
            return new CompletedRegistration
            {
                VoterIdentity = entity["VoterId"].StringValue,
                VotingId = entity["VotingId"].IntegerValue,
                VoterName = entity["VoterName"].StringValue,
                RegistrationTime = entity["RegistrationTime"].TimestampValue.ToDateTime(),
                DecisionTime = entity["DecisionTime"].TimestampValue.ToDateTime(),
                Decision = (RegistrationDecision)entity["Decision"].IntegerValue,
                AdminUserId = entity["AdminUserId"].StringValue
            };
        }

        private Entity _MapToEntity(CompletedRegistration completedRegistration)
        {
            return new Entity()
            {
                Key = _completedKeyFactory.CreateIncompleteKey(),
                ["VotingId"] = completedRegistration.VotingId,
                ["VoterId"] = completedRegistration.VoterIdentity,
                ["VoterName"] = completedRegistration.VoterName,
                ["RegistrationTime"] = Timestamp.FromDateTime(completedRegistration.RegistrationTime),
                ["DecisionTime"] = Timestamp.FromDateTime(completedRegistration.DecisionTime),
                ["AdminUserId"] = completedRegistration.AdminUserId,
                ["Decision"] = (int)completedRegistration.Decision
            };
        }

        public async Task<OpenRegistration> GetOpenRegistration(long id)
        {
            var entity = await _db.LookupAsync(_openKeyFactory.CreateKey(id));

            return _FromEntity(entity);
        }

        private static OpenRegistration _FromEntity(Entity entity)
        {
            return new OpenRegistration
            {
                RegistrationId = entity.Key.Path.First().Id,
                VoterIdentity = entity["VoterId"].StringValue,
                VotingId = entity["VotingId"].IntegerValue,
                VoterName = entity["VoterName"].StringValue,
                RegistrationTime = entity["RegistrationTime"].TimestampValue.ToDateTime(),
                EMailAdress = entity["Mail"].StringValue,
                RegistrationStoreId = entity["RegistrationStoreId"].StringValue
            };
        }

        public Task RemoveOpenRegistration(long id)
        {
            return _db.DeleteAsync(_openKeyFactory.CreateKey(id));
        }
    }
}
