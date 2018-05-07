﻿using System;
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
        private readonly KeyFactory _keyFactory;

        public VotingStore(string projectId, string namespaceId = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId, namespaceId, client);
            _keyFactory = new KeyFactory(projectId, namespaceId, StoreKind);
        }

        public async Task Insert(StandardVoting voting)
        {
            var entity = ToEntity(voting);
            entity.Key = _keyFactory.CreateIncompleteKey();
            var key = await _db.InsertAsync(entity);
            voting.Id = key.Path.First().Id;
        }

        public async Task Update(StandardVoting voting)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.Equal("__key__", _keyFactory.CreateKey(voting.Id)),
                Limit = 1
            };

            var result = await _db.RunQueryAsync(query).ConfigureAwait(false);
            if (result.Entities.Count == 0)
            {
                throw new InvalidOperationException("No voting to update!");
            }

            var readVoting = FromEntity(result.Entities.Single());
            if (!readVoting.Creator.Equals(voting.Creator, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot modify creator of voting!");
            }

            readVoting.Description = voting.Description;
            readVoting.Questions = voting.Questions;
            readVoting.Title = voting.Title;
            readVoting.Visibility = voting.Visibility;
            await _db.UpdateAsync(ToEntity(readVoting));
        }

        public async Task<StandardVoting> GetById(long id)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.Equal("__key__", _keyFactory.CreateKey(id)),
                Limit = 1
            };

            var result = await _db.RunQueryAsync(query).ConfigureAwait(false);
            return FromEntity(result.Entities.Single());
        }

        public async Task<IEnumerable<StandardVoting>> GetAllPublic()
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.Equal("Visibility", (int)VotingVisibility.Public),
                Order = { { "DateCreated", PropertyOrder.Types.Direction.Descending } }
            };

            var results = await _db.RunQueryAsync(query).ConfigureAwait(false);

            return results.Entities.Select(FromEntity);
        }

        public async Task<IEnumerable<StandardVoting>> GetForUserId(string userId)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.Equal("Creator", userId),
                //Order = { { "DateCreated", PropertyOrder.Types.Direction.Descending } }
            };

            var results = await _db.RunQueryAsync(query).ConfigureAwait(false);

            return results.Entities.Select(FromEntity);
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
            var query = new Query(StoreKind)
            {
                Order = { { "DateCreated", PropertyOrder.Types.Direction.Descending } }
            };
            var results = await _db.RunQueryAsync(query).ConfigureAwait(false);

            return results.Entities.Select(FromEntity);
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

        private Entity ToEntity(StandardVoting standardVoting)
        {
            return new Entity
            {
                Key = _keyFactory.CreateKey(standardVoting.Id),
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