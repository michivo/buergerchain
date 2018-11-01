﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Org.BouncyCastle.Crypto.Parameters;

namespace FreieWahl.Application.Registrations
{
    public interface IRemoteTokenStore
    {
        Task<string> GrantRegistration(string registrationStoreId, 
            StandardVoting voting,
            string signedChallengeString, 
            List<string> signedTokens, 
            string votingUrl,
            TimeSpan utcOffset,
            string timezoneName);

        Task<RegistrationChallenge> GetChallenge(string registrationId);

        Task InsertPublicKeys(string votingId, IEnumerable<RsaKeyParameters> publicKeys);
    }
}