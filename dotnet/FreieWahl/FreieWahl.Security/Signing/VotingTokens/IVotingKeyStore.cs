using System.Collections.Generic;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;

namespace FreieWahl.Security.Signing.VotingTokens
{
    /// <summary>
    /// The voting key store stores the key pairs required for signing blinded voting tokens
    /// </summary>
    public interface IVotingKeyStore
    {
        /// <summary>
        /// Stores a list of private/public key pairs
        /// </summary>
        /// <param name="votingId">the voting id the list of key pairs belongs to</param>
        /// <param name="keys">a list of keys - the key is the question index for a key pair, the value is the key pair that is stored</param>
        /// <returns>the future of this operation</returns>
        Task StoreKeyPairs(string votingId, Dictionary<int, AsymmetricCipherKeyPair> keys);

        /// <summary>
        /// gets the key pair for the given voting id and question index
        /// </summary>
        /// <param name="votingId">a voting id</param>
        /// <param name="index">the question index</param>
        /// <returns>the key pair for the given voting id and question index</returns>
        Task<AsymmetricCipherKeyPair> GetKeyPair(string votingId, int index);
    }
}
