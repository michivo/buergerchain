using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public class VotingTokenHandler : IVotingTokenHandler
    {
        private readonly IVotingKeyStore _keyStore;
        private readonly int _maxNumTokens;
        private readonly SHA256Managed _digest;

        public VotingTokenHandler(IVotingKeyStore keyStore, int maxNumTokens)
        {
            _keyStore = keyStore;
            _maxNumTokens = maxNumTokens;
            _digest = new SHA256Managed();
            _digest.Initialize();
        }

        public async Task<IEnumerable<RsaKeyParameters>> GenerateTokens(string votingId, int? numTokens = null)
        {
            if (numTokens == null)
            {
                numTokens = _maxNumTokens;
            }
            else if (numTokens < 1 || numTokens > _maxNumTokens)
            {
                throw new ArgumentException("Invalid number of tokens");
            }

            var keyGenParams = new RsaKeyGenerationParameters(
                new BigInteger("65537"), new SecureRandom(), 2048, 5);
            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(keyGenParams);
            var keyPairs = new Dictionary<int, AsymmetricCipherKeyPair>();
            await Task.Run(() =>
            {
                for (int index = 0; index < numTokens; index++)
                {
                    var keys = keyGen.GenerateKeyPair();
                    keyPairs.Add(index, keys);
                }
            }).ConfigureAwait(false);
            var publicKeys = keyPairs.Select(x => x.Value.Public).Cast<RsaKeyParameters>();

            await _keyStore.StoreKeyPairs(votingId, keyPairs).ConfigureAwait(false);
            return publicKeys;
        }

        public async Task<string> Sign(string token, string votingId, int tokenIndex)
        {
            var keyPair = await _keyStore.GetKeyPair(votingId, tokenIndex);
            var privateKey = (RsaPrivateCrtKeyParameters) keyPair.Private;
            BigInteger tokenInt = new BigInteger(token, 16);
            var signed = tokenInt.ModPow(privateKey.Exponent, privateKey.Modulus);
            return signed.ToString(16);
        }

        private static readonly BigInteger MaxHashValue = new BigInteger(new byte[] 
            { 1, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0 });

        public async Task<bool> Verify(string signature, string origMessage, string votingId, int tokenIndex)
        {
            var keyPair = await _keyStore.GetKeyPair(votingId, tokenIndex);
            var sigInt = new BigInteger(signature, 16);
            var publicKey = (RsaKeyParameters) keyPair.Public;

            var bytes = _digest.ComputeHash(Encoding.UTF8.GetBytes(origMessage));
            var messageHash = new BigInteger(bytes);
            if (messageHash.CompareTo(BigInteger.Zero) < 0)
            {
                messageHash = MaxHashValue.Add(messageHash);
            }
            var sigIntVerification = sigInt.ModPow(publicKey.Exponent, publicKey.Modulus);
            return Equals(messageHash, sigIntVerification);
        }
    }
}