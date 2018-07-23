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
    public class VotingKeyStore : IVotingKeyStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "VotingTokenKeys";
        public static readonly string TestNamespace = "test";
        public static readonly string DevNamespace = "dev";
        private readonly KeyFactory _keyFactory;

        public VotingKeyStore(string projectId, string namespaceId = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId, namespaceId, client);
            _keyFactory = new KeyFactory(projectId, namespaceId, StoreKind);
        }

        public Task StoreKeyPairs(long votingId, Dictionary<int, AsymmetricCipherKeyPair> keys)
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
           
            return _db.InsertAsync(entities.ToArray());
        }

        private Value GetPublicKey(AsymmetricCipherKeyPair keys)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Public);
            pemWriter.Writer.Flush();

            return textWriter.ToString();
        }

        private string GetPrivateKey(AsymmetricCipherKeyPair keys)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Private);
            pemWriter.Writer.Flush();

            return textWriter.ToString();
        }

        public async Task<AsymmetricCipherKeyPair> GetKeyPair(long votingId, int index)
        {
            var query = new Query(StoreKind)
            {
                Filter = Filter.And(Filter.Equal("VotingId", votingId),
                    Filter.Equal("KeyIndex", index)),
                Limit = 1
            };
            var queryResult = await _db.RunQueryAsync(query);
            if (queryResult.Entities.Count != 1)
            {
                throw new InvalidOperationException(
                    "Error getting query results, no key pair for given voting id and index");
            }

            var entity = queryResult.Entities.Single();
            var privateKey = entity["PrivateKey1"].StringValue + entity["PrivateKey2"].StringValue;

            var privateKeyReader = new PemReader(new StringReader(privateKey));
            var privateKeyObj = privateKeyReader.ReadObject();

            return privateKeyObj as AsymmetricCipherKeyPair;
        }
    }
}
