using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Voting.Storage
{
    public class VotingResultStore
    {
        public async Task StoreVote(long votingId, int questionId, List<long> answers, string token, string signedToken)
        {

        }
    }
}
