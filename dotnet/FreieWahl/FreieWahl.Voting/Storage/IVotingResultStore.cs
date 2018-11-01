using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Voting.Storage
{
    public interface IVotingResultStore
    {
        Task StoreVote(Vote v);

        Task<IReadOnlyCollection<Vote>> GetVotes(string votingId);

        Task<IReadOnlyCollection<Vote>> GetVotes(string votingId, int questionIndex);
        
        Task<Vote> GetLastVote(string votingId, int questionIndex);
    }
}