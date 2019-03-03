﻿using System.Threading.Tasks;
using FreieWahl.Voting.Registrations;

namespace FreieWahl.Application.Registrations
{
    /// <summary>
    /// For 
    /// </summary>
    public interface IChallengeService
    {
        string GetStandardizedRecipient(string recipient);

        Task SendChallenge(string recipient, string challenge, string votingName, string votingId);

        ChallengeType SupportedChallengeType { get; }
    }
}
