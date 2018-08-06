using System.Collections.Generic;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.VotingResults
{
    public interface IVotingChainBuilder
    {
        string GetGenesisValue(Question q);

        string GetSignature(Vote v);

        void CheckChain(Question q, IReadOnlyList<Vote> votes);
    }
}
