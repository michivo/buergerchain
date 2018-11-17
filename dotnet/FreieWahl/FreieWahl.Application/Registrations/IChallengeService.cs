﻿using System.Threading.Tasks;
using FreieWahl.Voting.Registrations;

namespace FreieWahl.Application.Registrations
{
    public interface IChallengeService
    {
        Task SendChallenge(string recipient, string challenge, string votingName);

        ChallengeType SupportedChallengeType { get; }
    }
}
