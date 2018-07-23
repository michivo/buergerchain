using System;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public class VotingTokenHandler : IVotingTokenHandler
    {
        private readonly int _maxNumTokens;

        public VotingTokenHandler(int maxNumTokens)
        {
            _maxNumTokens = maxNumTokens;
        }

        public Task GenerateTokens(long votingId, int numTokens)
        {
            if (numTokens < 1 || numTokens > _maxNumTokens)
            {
                throw new ArgumentException("Invalid number of tokens");
            }

            var keyGenParams = new RsaKeyGenerationParameters(
                new BigInteger("65537"), new SecureRandom(), 2048, 5);
            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(keyGenParams);
            return Task.Run(() =>
            {
                var keys = keyGen.GenerateKeyPair();
                
            });
        }

        public string Sign(string token, long votingId, int tokenIndex)
        {
            throw new NotImplementedException();
        }

        public bool Verify(string signature, string origMessage, long votingId, int tokenIndex)
        {
            throw new NotImplementedException();
        }
    }
}