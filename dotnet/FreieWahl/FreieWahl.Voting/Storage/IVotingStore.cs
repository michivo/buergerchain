using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Voting.Storage
{
    public interface IVotingStore
    {
        void Insert(StandardVoting voting);

        Task<IEnumerable<StandardVoting>> GetAll();

        void ClearAll();
    }
}
