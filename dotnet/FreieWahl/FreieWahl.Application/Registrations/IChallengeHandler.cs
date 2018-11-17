﻿using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Registrations;

namespace FreieWahl.Application.Registrations
{
    interface IChallengeHandler
    {
        Task CreateChallenge(string recipientName, string recipientAddress, StandardVoting voting, string registrationId, ChallengeType challengeType);

        Task<Challenge> GetChallengeForResponse(string registrationId, string challengeResponse);
    }
}
