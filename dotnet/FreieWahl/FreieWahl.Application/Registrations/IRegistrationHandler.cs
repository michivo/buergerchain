using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRegistrationHandler
    {
        Task GrantRegistration(long registrationId, string userId);

        Task DenyRegistration(long registrationId, string userId);
    }
}