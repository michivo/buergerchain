using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.Authentication
{
    public class AuthenticationResult
    {
        public bool IsAuthorized { get; set; }

        public StandardVoting Voting { get; set; }

        public UserInformation User { get; set; }


        private static AuthenticationResult _notAuthorized;

        public static AuthenticationResult NotAuthorized => _notAuthorized ?? (_notAuthorized = new AuthenticationResult {IsAuthorized = false});
    }
}
