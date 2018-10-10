using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRegistrationHandler
    {
        Task GrantRegistration(long registrationId, string userId, string votingUrl);

        Task DenyRegistration(long registrationId, string userId);
    }
}