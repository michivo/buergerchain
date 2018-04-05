using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Voting.Storage
{
    public interface IVotingStore
    {
        void Insert(StandardVoting voting);

        Task Update(StandardVoting voting);

        Task<IEnumerable<StandardVoting>> GetAllPublic();

        Task<IEnumerable<StandardVoting>> GetForUserId(string userId);

        Task<IEnumerable<StandardVoting>> GetAll();

        void ClearAll();
    }
}
