using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public class VotingKeyFireStore : IVotingKeyStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "VotingTokenKeys";
        private readonly KeyFactory _keyFactory;
        private readonly Dictionary<string, Dictionary<int, AsymmetricCipherKeyPair>> _keyCache;

        public VotingKeyFireStore(string projectId, string prefix = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId);
            _keyFactory = new KeyFactory(projectId, string.Empty, StoreKind);
            _keyCache = new Dictionary<string, Dictionary<int, AsymmetricCipherKeyPair>>();
        }

        public async Task StoreKeyPairs(string votingId, Dictionary<int, AsymmetricCipherKeyPair> keys)
        {
            var entities = keys.Select(x =>
            {
                var privateKey = GetPrivateKey(x.Value);
                var privateKeyPart1 = privateKey.Substring(0, 1500);
                var privateKeyPart2 = string.Empty;
                if (privateKey.Length > 1500)
                {
                    privateKeyPart2 = privateKey.Substring(1500);
                }
                return new Entity()
                {
                    Key = _keyFactory.CreateIncompleteKey(),
                    ["PrivateKey1"] = privateKeyPart1,
                    ["PrivateKey2"] = privateKeyPart2,
                    ["VotingId"] = votingId,
                    ["KeyIndex"] = x.Key
                };
            });

            // TODO: what if entries with the same voting id already exist?
            await _db.InsertAsync(entities.ToArray()).ConfigureAwait(false);
            _keyCache[votingId] = keys;
        }

        private string GetPrivateKey(AsymmetricCipherKeyPair keys)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Private);
            pemWriter.Writer.Flush();

            return textWriter.ToString();
        }

        public AsymmetricCipherKeyPair GetKeyPair(string votingId, int index)
        {
            if (_keyCache.ContainsKey(votingId) && _keyCache[votingId].ContainsKey(index))
            {
                return _keyCache[votingId][index];
            }

            var query = new Query(StoreKind)
            {
                Filter = Filter.And(Filter.Equal("VotingId", votingId),
                    Filter.Equal("KeyIndex", index)),
                Limit = 1
            };

            var queryResult = _db.RunQuery(query);
            if (queryResult.Entities.Count != 1)
            {
                throw new InvalidOperationException(
                    "Error getting query results, no key pair for given voting id and index");
            }

            var entity = queryResult.Entities.Single();
            var privateKey = entity["PrivateKey1"].StringValue + entity["PrivateKey2"].StringValue;

            var privateKeyReader = new PemReader(new StringReader(privateKey));
            var privateKeyObj = privateKeyReader.ReadObject();

            var result = (AsymmetricCipherKeyPair)privateKeyObj;
            _AddToCache(votingId, index, result);

            return result;
        }

        private void _AddToCache(string votingId, int index, AsymmetricCipherKeyPair result)
        {
            if (_keyCache.ContainsKey(votingId) == false)
            {
                _keyCache.Add(votingId, new Dictionary<int, AsymmetricCipherKeyPair>());
            }

            _keyCache[votingId][index] = result;
        }
    }
}
