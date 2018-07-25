using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRemoteTokenStore
    {
        Task<string> GrantRegistration(long registrationId, string signedChallengeString);
    }
}