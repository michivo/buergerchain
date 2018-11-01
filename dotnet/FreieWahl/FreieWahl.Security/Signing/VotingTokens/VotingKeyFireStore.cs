using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public class VotingKeyFireStore : IVotingKeyStore
    {
        private readonly FirestoreDb _db;
        private readonly string _collection = "VotingTokenKeys";
        private readonly Dictionary<string, Dictionary<int, AsymmetricCipherKeyPair>> _keyCache;

        public VotingKeyFireStore(string projectId, string prefix = "")
        {
            _collection = prefix + _collection;
            _db = FirestoreDb.Create(projectId);
            _keyCache = new Dictionary<string, Dictionary<int, AsymmetricCipherKeyPair>>();
        }

        public async Task StoreKeyPairs(string votingId, Dictionary<int, AsymmetricCipherKeyPair> keys)
        {
            var docRef = _db.Collection(_collection).Document(votingId);
            var keyCollection = docRef.Collection("Keys");

            foreach (var key in keys)
            {
                var keyDoc = keyCollection.Document(key.Key.ToString(CultureInfo.InvariantCulture));
                await keyDoc.SetAsync(new Dictionary<string, object> { { "Key", GetPrivateKey(key.Value) } });
            }

            _keyCache[votingId] = keys;
        }

        private static string GetPrivateKey(AsymmetricCipherKeyPair keys)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Private);
            pemWriter.Writer.Flush();

            return textWriter.ToString();
        }

        public async Task<AsymmetricCipherKeyPair> GetKeyPair(string votingId, int index)
        {
            if (_keyCache.ContainsKey(votingId) && _keyCache[votingId].ContainsKey(index))
            {
                return _keyCache[votingId][index];
            }

            var docRef = _db.Collection(_collection).Document(votingId).Collection("Keys")
                .Document(index.ToString(CultureInfo.InvariantCulture));

            var key = await docRef.GetSnapshotAsync();
            if (!key.Exists)
            {
                throw new InvalidOperationException("No key for the given voting id and index!" + votingId + " " + index);
            }

            var privateKey = key.GetValue<string>("Key");

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
