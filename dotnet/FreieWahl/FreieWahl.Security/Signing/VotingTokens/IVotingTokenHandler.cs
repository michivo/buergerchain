using System.Threading.Tasks;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingTokenHandler
    {
        Task GenerateTokens(long votingId, int? numTokens = null);

        Task<string> Sign(string token, long votingId, int tokenIndex);

        Task<bool> Verify(string signature, string origMessage, long votingId, int tokenIndex);
    }
}
