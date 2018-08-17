using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;

namespace FreieWahl.Voting.Registrations
{
    public class RegistrationStore : IRegistrationStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "Registration";
        public static readonly string TestNamespace = "test";
        public static readonly string DevNamespace = "dev";
        private readonly KeyFactory _keyFactory;

        public RegistrationStore(string projectId, string namespaceId = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId, namespaceId, client);
            _keyFactory = new KeyFactory(projectId, namespaceId, StoreKind);
        }

        public async Task AddRegistration(Registration registration)
        {
            Query q = new Query(StoreKind)
            {
                Filter = Filter.And(
                    Filter.Equal("VotingId", registration.VotingId),
                    Filter.Equal("VoterId", registration.VoterIdentity)),
            };
            var results = await _db.RunQueryAsync(q);
            if (results.Entities.Count > 0)
            { // avoid double-registrations
                await _db.DeleteAsync(results.Entities);
            }

            var entity = _MapToEntity(registration);

            var key = await _db.InsertAsync(entity).ConfigureAwait(false);
            registration.RegistrationId = key.Path.First().Id;
        }

        private Entity _MapToEntity(Registration registration)
        {
            return new Entity()
            {
                Key = _keyFactory.CreateIncompleteKey(),
                ["VotingId"] = registration.VotingId,
                ["VoterId"] = registration.VoterIdentity,
                ["VoterName"] = registration.VoterName
            };
        }

        public async Task<IReadOnlyList<Registration>> GetRegistrationsForVoting(long votingId)
        {
            Query q = new Query()
            {
                Filter = Filter.Equal("VotingId", votingId)
            };

            var results = await _db.RunQueryAsync(q);

            return new List<Registration>(results.Entities.Select(_FromEntity));
        }

        public async Task<Registration> GetRegistration(long id)
        {
            var entity = await _db.LookupAsync(_keyFactory.CreateKey(id));

            return _FromEntity(entity);
        }

        private static Registration _FromEntity(Entity entity)
        {
            return new Registration
            {
                RegistrationId = entity.Key.Path.First().Id,
                VoterIdentity = entity["VoterId"].StringValue,
                VotingId = ((long?)entity["VotingId"]).Value,
                VoterName = entity["VoterName"].StringValue
            };
        }

        public Task RemoveRegistration(long id)
        {
            return _db.DeleteAsync(_keyFactory.CreateKey(id));
        }
    }
}
