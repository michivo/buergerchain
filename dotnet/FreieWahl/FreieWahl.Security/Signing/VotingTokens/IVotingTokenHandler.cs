using System.Collections.Generic;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Parameters;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingTokenHandler
    {
        Task<IEnumerable<RsaKeyParameters>> GenerateTokens(long votingId, int? numTokens = null);

        string Sign(string token, long votingId, int tokenIndex);

        bool Verify(string signature, string origMessage, long votingId, int tokenIndex);
    }
}
