using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.UserHandling;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        public async Task<bool> CheckAuthorization(string votingId, Operation operation, string authToken)
        {
            return await GetAuthorizedUser(votingId, operation, authToken).ConfigureAwait(false) != null;
        }

        public async Task<UserInformation> GetAuthorizedUser(string votingId, Operation operation, string authToken)
        {
            UserInformation user = null;
            var auth = _authentication.CheckToken(authToken);
            
            if (!auth.Success)
            {
                return null;
            }

            try
            {
                user = _userHandler.MapUser(auth.User);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting votings for user!");
                return null;
            }

            var authorized = await _authManager.IsAuthorized(user.UserId, votingId, operation).ConfigureAwait(false);
            if (!authorized)
            {
                _logger.LogWarning("User tried to open voting without being authorized");
                return null;
            }
            
            return user;
        }

    }
}