using System;
using System.Collections.Generic;
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

        public VotingTokenHandler(IVotingKeyStore keyStore, int maxNumTokens)
        {
            _keyStore = keyStore;
            _maxNumTokens = maxNumTokens;
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

        public async Task<string> Sign(string token, long votingId, int tokenIndex)
        {
            var signer = SignerUtilities.GetSigner("SHA256withRSA");
            var keyPair = await _keyStore.GetKeyPair(votingId, tokenIndex);

            signer.Init(true, keyPair.Private);
            var rawToken = Encoding.UTF8.GetBytes(token);
            signer.BlockUpdate(rawToken, 0, rawToken.Length);
            var signedData = signer.GenerateSignature();
            var result = new BigInteger(signedData);
            return result.ToString(16);
        }

        public async Task<bool> Verify(string signature, string origMessage, long votingId, int tokenIndex)
        {
            var verifier = SignerUtilities.GetSigner("SHA256withRSA");
            var keyPair = await _keyStore.GetKeyPair(votingId, tokenIndex);
            verifier.Init(false, keyPair.Public);
            var sigRaw = new BigInteger(signature, 16);
            var sigRawData = sigRaw.ToByteArray();
            var messageRaw = Encoding.UTF8.GetBytes(origMessage);
            verifier.BlockUpdate(messageRaw, 0, messageRaw.Length);
            return verifier.VerifySignature(sigRawData);
        }
    }
}