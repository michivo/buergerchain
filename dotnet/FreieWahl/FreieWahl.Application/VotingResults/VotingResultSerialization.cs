using System;
using System.Collections.Generic;
using System.Text;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.VotingResults
{
    public class VotingResultSerialization
    {
        private const int FORMAT_VERSION = 1;

        public byte[] Serialize()
        {
            return new byte[0];
        }

        public void Deserialize(byte[] data, out Vote vote)
        {
            vote = null;
        }
    }
}
