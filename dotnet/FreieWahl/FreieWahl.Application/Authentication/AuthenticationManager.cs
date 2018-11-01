﻿
using System;
using System.Threading.Tasks;
using FreieWahl.Common;
using FreieWahl.Voting.Storage;
using Microsoft.Extensions.Logging;

namespace FreieWahl.Application.Authentication
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IVotingStore _votingStore;
        private readonly ILogger _logger;

        public AuthenticationManager(IVotingStore votingStore)
        {
            _votingStore = votingStore;
            _logger = LogFactory.CreateLogger("FreieWahl.Security.TimeStamps.TimestampService");
        }

        public async Task<bool> IsAuthorized(string userId, string votingId, Operation operation)
        {
            try
            {
                if (string.IsNullOrEmpty(votingId) && (operation == Operation.Create || operation == Operation.List || operation == Operation.EditUser))
                    return !string.IsNullOrEmpty(userId);
                if (string.IsNullOrEmpty(votingId))
                    return false;

                var voting = await _votingStore.GetById(votingId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(userId) && voting.Creator.Equals(userId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"User {userId} is not authenticated to edit voting {votingId}: {ex}");
            }

            return false;
        }
    }
}