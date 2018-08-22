using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRemoteTokenStore
    {
        Task<string> GrantRegistration(string registrationStoreId, string signedChallengeString);
        Task<string> GetChallenge(string registrationId);
    }
}