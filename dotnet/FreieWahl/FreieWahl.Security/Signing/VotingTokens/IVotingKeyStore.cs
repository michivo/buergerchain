using System.Collections.Generic;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingKeyStore
    {
        Task StoreKeyPairs(long votingId, Dictionary<int, AsymmetricCipherKeyPair> keys);

        AsymmetricCipherKeyPair GetKeyPair(long votingId, int index);
    }
}
