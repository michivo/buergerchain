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
            string votingUrl);

        Task<RegistrationChallenge> GetChallenge(string registrationId);

        Task InsertPublicKeys(long votingId, IEnumerable<RsaKeyParameters> publicKeys);
    }
}