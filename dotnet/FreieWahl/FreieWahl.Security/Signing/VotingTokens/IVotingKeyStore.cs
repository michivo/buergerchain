using System.Collections.Generic;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingKeyStore
    {
        Task StoreKeyPairs(string votingId, Dictionary<int, AsymmetricCipherKeyPair> keys);

        AsymmetricCipherKeyPair GetKeyPair(string votingId, int index);
    }
}
