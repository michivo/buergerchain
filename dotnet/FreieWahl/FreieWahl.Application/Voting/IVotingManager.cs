using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.Voting
{
    public interface IVotingManager
    {
        Task Insert(StandardVoting voting);

        Task Update(StandardVoting voting);

        Task<StandardVoting> GetById(string id);

        Task<IEnumerable<StandardVoting>> GetForUserId(string userId);

        Task AddQuestion(string votingId, Question question);

        Task DeleteQuestion(string votingId, int questionIndex);

        Task UpdateQuestion(string votingId, Question question);

        Task UpdateState(string votingId, VotingState state);

        Task Delete(string votingId);
    }
}
