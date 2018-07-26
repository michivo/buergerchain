using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreieWahl.Security.TimeStamps;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.VotingResults
{
    public class VotingResultManager
    {
        private readonly ITimestampService _timestampService;

        public VotingResultManager(ITimestampService timestampService)
        {
            _timestampService = timestampService;
        }

        public Task StoreVote(long votingId, int questionIndex, List<long> answers, string token, string signedToken)
        {
            //var data = _GetRawData(votingId, questionIndex, answers, token, signedToken);
            //_timestampService.GetToken()
            return Task.CompletedTask;
        }

        private byte[] _GetRawData(long votingId, int questionIndex, List<long> answers, string token, string signedToken)
        {
            throw new NotImplementedException();
        }
    }
}
