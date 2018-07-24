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

            var entity = new Entity()
                {
                    Key = _keyFactory.CreateIncompleteKey(),
                    ["VotingId"] = registration.VotingId,
                    ["VoterId"] = registration.VoterIdentity,
                    ["VoterName"] = registration.VoterName
                };

            var key = await _db.InsertAsync(entity).ConfigureAwait(false);
            registration.RegistrationId = key.Path.First().Id;
        }

        public async Task<Registration> GetRegistration(long id)
        {
            Query q = new Query(StoreKind)
            {
                Filter = Filter.Equal("__key__", _keyFactory.CreateKey(id)),
                Limit = 1
            };

            var results = await _db.RunQueryAsync(q);
            if (results.Entities.Count == 0)
            {
                return null;
            }

            var entity = results.Entities.Single();
            return new Registration
            {
                RegistrationId = entity.Key.Path.First().Id,
                VoterIdentity = entity["VoterId"].StringValue,
                VotingId = ((long?)entity["VotingId"]).Value,
                VoterName = entity["VoterName"].StringValue
            };
        }
    }
}
