using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRegistrationHandler
    {
        Task GrantRegistration(long registrationId);

        Task DenyRegistration(long registrationId);
    }
}