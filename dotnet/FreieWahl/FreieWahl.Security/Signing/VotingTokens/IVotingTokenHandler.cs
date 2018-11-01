﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Parameters;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingTokenHandler
    {
        Task<IEnumerable<RsaKeyParameters>> GenerateTokens(string votingId, int? numTokens = null);

        string Sign(string token, string votingId, int tokenIndex);

        bool Verify(string signature, string origMessage, string votingId, int tokenIndex);
    }
}
