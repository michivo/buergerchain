using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.VotingResults
{
    public interface IVotingResultManager
    {
        Task StoreVote(string votingId, int questionIndex, List<string> answers, string token, string signedToken);

        void CompleteVoting(string votingId);

        void CompleteQuestion(string votingId, int questionIndex);

        Task<IReadOnlyCollection<Vote>> GetResults(string votingId, string[] tokens);

        Task<IReadOnlyCollection<Vote>> GetResults(string votingId);

        Task<IReadOnlyCollection<Vote>> GetResults(string votingId, int questionIndex);
    }
}