using System;
using System.Threading.Tasks;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.UserHandling;
using Microsoft.Extensions.Logging;

namespace FreieWahl.Application.Authentication
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        private readonly ILogger<AuthorizationHandler> _logger;
        private readonly IJwtAuthentication _authentication;
        private readonly IAuthenticationManager _authManager;
        private readonly IUserHandler _userHandler;

        public AuthorizationHandler(
            ILogger<AuthorizationHandler> logger,
            IJwtAuthentication authentication,
            IAuthenticationManager authManager,
            IUserHandler userHandler)
        {
            _logger = logger;
            _authentication = authentication;
            _authManager = authManager;
            _userHandler = userHandler;
        }
        
        public async Task<AuthenticationResult> CheckAuthorization(string votingId, Operation operation, string authToken)
        {
            UserInformation user = null;
            var auth = _authentication.CheckToken(authToken);
            
            if (!auth.Success)
            {
                return AuthenticationResult.NotAuthorized;
            }

            try
            {
                user = _userHandler.MapUser(auth.User);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting votings for user!");
                return AuthenticationResult.NotAuthorized;
            }

            var authorized = await _authManager.IsAuthorized(user, votingId, operation).ConfigureAwait(false);
            if (!authorized.IsAuthorized)
            {
                _logger.LogWarning("User tried to open voting without being authorized");
            }

            return authorized;
        }

    }
}