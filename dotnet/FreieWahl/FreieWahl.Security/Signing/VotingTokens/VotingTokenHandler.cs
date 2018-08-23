using System;
using System.Collections.Generic;
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

        public async Task GenerateTokens(long votingId, int? numTokens)
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

            await _keyStore.StoreKeyPairs(votingId, keyPairs).ConfigureAwait(false);
        }

        public string Sign(string token, long votingId, int tokenIndex)
        {
            var keyPair = _keyStore.GetKeyPair(votingId, tokenIndex);
            var privateKey = (RsaPrivateCrtKeyParameters) keyPair.Private;
            BigInteger tokenInt = new BigInteger(token, 16);
            var signed = tokenInt.ModPow(privateKey.Exponent, privateKey.Modulus);
            return signed.ToString(16);
        }



        public bool Verify(string signature, string origMessage, long votingId, int tokenIndex)
        {
            var keyPair = _keyStore.GetKeyPair(votingId, tokenIndex);
            var sigInt = new BigInteger(signature, 16);
            var publicKey = (RsaKeyParameters) keyPair.Public;

            var messageHash = new BigInteger(_digest.ComputeHash(Encoding.UTF8.GetBytes(origMessage)));
            var sigIntVerification = sigInt.ModPow(publicKey.Exponent, publicKey.Modulus);
            return Equals(messageHash, sigIntVerification);
        }
    }
}