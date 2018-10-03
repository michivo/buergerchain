using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Voting.Storage
{
    public interface IVotingStore
    {
        Task Insert(StandardVoting voting);

        Task Update(StandardVoting voting);

        Task<StandardVoting> GetById(long id);

        Task<IEnumerable<StandardVoting>> GetForUserId(string userId);

        Task<IEnumerable<StandardVoting>> GetAll();

        void ClearAll();

        Task AddQuestion(long votingId, Question question);

        Task DeleteQuestion(long votingId, int questionIndex);

        Task ClearQuestions(long votingId);

        Task UpdateQuestion(long votingId, Question question);

        Task UpdateState(long votingId, VotingState state);

        Task Delete(long votingId);
    }
}
