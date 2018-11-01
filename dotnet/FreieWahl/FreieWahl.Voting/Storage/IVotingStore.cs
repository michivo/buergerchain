using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Voting.Storage
{
    public interface IVotingStore
    {
        Task Insert(StandardVoting voting);

        Task Update(StandardVoting voting);

        Task<StandardVoting> GetById(string id);

        Task<IEnumerable<StandardVoting>> GetForUserId(string userId);

        Task<IEnumerable<StandardVoting>> GetAll();

        Task ClearAll();

        Task AddQuestion(string votingId, Question question);

        Task DeleteQuestion(string votingId, int questionIndex);

        Task ClearQuestions(string votingId);

        Task UpdateQuestion(string votingId, Question question);

        Task UpdateState(string votingId, VotingState state);

        Task Delete(string votingId);
    }
}
