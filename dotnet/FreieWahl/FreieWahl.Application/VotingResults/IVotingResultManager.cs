using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Application.VotingResults
{
    public interface IVotingResultManager
    {
        Task StoreVote(long votingId, int questionIndex, List<string> answers, string token, string signedToken);

        void CompleteVoting(long votingId);

        void CompleteQuestion(long votingId, int questionIndex);
    }
}