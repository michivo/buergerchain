using System.Collections.Generic;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Parameters;

namespace FreieWahl.Application.Registrations
{
    public interface IRemoteTokenStore
    {
        Task<string> GrantRegistration(string registrationStoreId,
            long votingId,
            string signedChallengeString,
            List<string> signedTokens);

        Task<RegistrationChallenge> GetChallenge(string registrationId);

        Task InsertPublicKeys(long votingId, IEnumerable<RsaKeyParameters> publicKeys);
    }
}