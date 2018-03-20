using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Google.Cloud.Datastore.V1;

namespace FreieWahl.Voting.Storage
{
    public partial class VotingStore : IVotingStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "StandardVoting";
        public static readonly string TestNamespace = "test";
        public static readonly string DevNamespace = "dev";

        public VotingStore(string projectId, string namespaceId = "")
        {
            _db = DatastoreDb.Create(projectId, namespaceId);
        }

        public void Insert(StandardVoting voting)
        {
            var entity = ToEntity(voting);
            entity.Key = _db.CreateKeyFactory(StoreKind).CreateIncompleteKey();
            _db.Insert(entity);
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
            var query = await _db.RunQueryAsync(new Query(StoreKind));

            return query.Entities.Select(FromEntity);
        }

        private static StandardVoting FromEntity(Entity entity)
        {
            var visibility = (int?)entity["Visibility"];
            var visibilityValue = visibility == null ? VotingVisibility.OwnerOnly : (VotingVisibility)visibility;

            return new StandardVoting()
            {
                Id = entity.Key.Path.First().Id,
                Title = (string)entity["Title"],
                Creator = (string)entity["Creator"],
                Description = (string)entity["Description"],
                DateCreated = (DateTime)entity["DateCreated"],
                Visibility = visibilityValue,
                Questions = FromQuestionsEntity(entity["Questions"])
            };
        }

        private static Entity ToEntity(StandardVoting standardVoting)
        {
            return new Entity
            {
                Key = new Key().WithElement(StoreKind, standardVoting.Id),
                ["Title"] = standardVoting.Title,
                ["Creator"] = standardVoting.Creator,
                ["DateCreated"] = standardVoting.DateCreated.ToUniversalTime(),
                ["Description"] = standardVoting.Description,
                ["Visibility"] = (int)standardVoting.Visibility,
                ["Questions"] = ToEntities(standardVoting.Questions)
            };
        }
    }
}
