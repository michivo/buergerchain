
using System;
using System.Threading.Tasks;
using FreieWahl.Common;
using FreieWahl.Security.UserHandling;
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

        public async Task<AuthenticationResult> IsAuthorized(UserInformation user, string votingId, Operation operation)
        {
            try
            {
                if (string.IsNullOrEmpty(votingId) && (operation == Operation.Create || operation == Operation.List || operation == Operation.EditUser))
                    return new AuthenticationResult {IsAuthorized = user != null, User = user};
                if (string.IsNullOrEmpty(votingId))
                    return AuthenticationResult.NotAuthorized;

                var voting = await _votingStore.GetById(votingId).ConfigureAwait(false);
                if (user != null && voting != null && voting.Creator.Equals(user.UserId, StringComparison.OrdinalIgnoreCase))
                    return new AuthenticationResult {IsAuthorized = true, User = user, Voting = voting};
            }
            catch (Exception ex)
            {
                var userId = user != null ? user.UserId : "undefined";
                _logger.LogWarning($"User {userId} is not authenticated to execute operation {operation} on voting {votingId}: {ex}");
            }

            return AuthenticationResult.NotAuthorized;
        }
    }
}