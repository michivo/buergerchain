using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingTokenHandler
    {
        Task GenerateTokens(long votingId, int numTokens);

        string Sign(string token, long votingId, int tokenIndex);

        bool Verify(string signature, string origMessage, long votingId, int tokenIndex);
    }
}
