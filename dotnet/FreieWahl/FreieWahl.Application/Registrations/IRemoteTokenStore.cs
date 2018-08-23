using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRemoteTokenStore
    {
        Task<string> GrantRegistration(string registrationStoreId, string signedChallengeString,
            List<string> signedTokens);
        Task<RegistrationChallenge> GetChallenge(string registrationId);
    }
}